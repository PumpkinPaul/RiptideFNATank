/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Microsoft.Extensions.Logging;
using MoonTools.ECS;
using Riptide;
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankClient.Networking;
using RiptideFNATankCommon.Gameplay.Components;
using RiptideFNATankCommon.Networking;
using System;
using Wombat.Engine;
using Wombat.Engine.Logging;

namespace RiptideFNATankClient.Gameplay.Systems;

static partial class Log
{
    [LoggerMessage(Message = "Sending client commands to server for command frame: {currentClientCommandFrame}, commandCount: {commandCount}")]
    public static partial void SendPlayerCommands(this ILogger logger, LogLevel logLevel, uint currentClientCommandFrame, uint commandCount);

    [LoggerMessage(Message = "Effective frame: {effectiveCommandFrame}, moveUp: {moveUp}, moveDown: {moveDown}")]
    public static partial void PlayerCommandsForFrame(this ILogger logger, LogLevel logLevel, uint effectiveCommandFrame, bool moveUp, bool moveDown);

    [LoggerMessage(Message = "Client is too far behind server, commandCount: {commandCount}, maxCommandCount: {maxCommandCount}")]
    public static partial void ClientIsTooFarBegindServer(this ILogger logger, LogLevel logLevel, uint commandCount, uint maxCommandCount);
}

/// <summary>
/// Sends the local player's actions to the server
/// </summary>
public sealed class PlayerSendNetworkCommandsSystem : MoonTools.ECS.System
{
    readonly CircularBuffer<PlayerCommandsComponent> _playerActions;
    readonly NetworkGameManager _networkGameManager;

    readonly Filter _filter;

    public PlayerSendNetworkCommandsSystem(
        World world,
        CircularBuffer<PlayerCommandsComponent> playerActions,
        NetworkGameManager networkGameManager
    ) : base(world)
    {
        _playerActions = playerActions;
        _networkGameManager = networkGameManager;

        _filter = FilterBuilder
            .Include<PlayerCommandsComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (RandomHelper.FastRandom.PercentageChance(_networkGameManager.DroppedPacketPercentage))
            return;

        ref readonly var simulationState = ref GetSingleton<SimulationStateComponent>();
        if (simulationState.ServerReceivedClientCommandFrame == 0)
            return;

        foreach (var entity in _filter.Entities)
        {
            // Send all the user commands that the server has yet to ack...
            var commandCount = simulationState.CurrentClientCommandFrame - simulationState.ServerReceivedClientCommandFrame;

            const uint MAX_COMMAND_COUNT = 120;
            if (commandCount > MAX_COMMAND_COUNT)
            {
                Logger.Log.ClientIsTooFarBegindServer(LogLevel.Error, simulationState.CurrentClientCommandFrame, commandCount);
                continue;
            }

            var message = Message.Create(MessageSendMode.Unreliable, ClientMessageType.PlayerCommands);

            // Header
            byte gameId = 0;
            message.AddByte(gameId);
            message.AddUInt(simulationState.LastReceivedServerCommandFrame);

            // Payload
            message.AddUInt(simulationState.CurrentClientCommandFrame);
            message.AddByte((byte)commandCount);

            Logger.Log.SendPlayerCommands(LogLevel.Information, simulationState.CurrentClientCommandFrame, commandCount);

            for (uint clientCommandFrame = simulationState.ServerReceivedClientCommandFrame + 1; clientCommandFrame <= simulationState.CurrentClientCommandFrame; clientCommandFrame++)
            {
                message.AddUInt(clientCommandFrame);
                var playerActions = _playerActions.Get(clientCommandFrame);
                message.AddBool(playerActions.MoveUp);
                message.AddBool(playerActions.MoveDown);

                Logger.Log.PlayerCommandsForFrame(LogLevel.Debug, simulationState.CurrentClientCommandFrame, playerActions.MoveUp, playerActions.MoveDown);
            }

            // Send a network packet containing the player's state.
            _networkGameManager.SendMessage(message);
        }
    }
}
