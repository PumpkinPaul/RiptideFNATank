/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using MoonTools.ECS;
using Riptide;
using RiptideFNATankClient.Networking;
using RiptideFNATankCommon.Components;
using RiptideFNATankCommon.Networking;
using System;
using Wombat.Engine;

namespace RiptideFNATankClient.Gameplay.Systems;

/// <summary>
/// Sends the local player's actions to the server
/// </summary>
public sealed class PlayerSendNetworkCommandsSystem : MoonTools.ECS.System
{
    readonly CircularBuffer<PlayerActionsComponent> _playerActions;
    readonly NetworkGameManager _networkGameManager;

    public PlayerSendNetworkCommandsSystem(
        World world,
        CircularBuffer<PlayerActionsComponent> playerActions,
        NetworkGameManager networkGameManager
    ) : base(world)
    {
        _playerActions = playerActions;
        _networkGameManager = networkGameManager;
    }

    public override void Update(TimeSpan delta)
    {
        if (RandomHelper.FastRandom.PercentageChance(_networkGameManager.DroppedPacketPercentage))
            return;

        ref readonly var simulationState = ref GetSingleton<SimulationStateComponent>();

        var message = Message.Create(MessageSendMode.Unreliable, ClientMessageType.SendPlayerCommands);

        // Header
        byte gameId = 0;
        message.AddByte(gameId);
        message.AddUInt(simulationState.LastReceivedServerTick);

        // Payload
        message.AddUInt(simulationState.CurrentClientTick);

        // Send all the user commands that the server has yet to ack...
        var commandCount = (byte)(simulationState.CurrentClientTick - simulationState.ServerProcessedClientInputAtClientTick);
        message.AddByte(commandCount);
        for (var i = simulationState.ServerProcessedClientInputAtClientTick + 1; i < simulationState.CurrentClientTick; i++)
        {
            var playerActions = _playerActions.Get(i);
            message.AddBool(playerActions.MoveUp);
            message.AddBool(playerActions.MoveDown);
        }

        // Send a network packet containing the player's state.
        _networkGameManager.SendMessage(message);
    }
}
