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
using Microsoft.Xna.Framework;
using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankCommon.Gameplay.Components;
using RiptideFNATankCommon.Networking;
using System;
using Wombat.Engine;
using Wombat.Engine.Logging;

namespace RiptideFNATankClient.Gameplay.Systems;

public readonly record struct ReceivedWorldStateMessage(
    ushort ClientId,
    uint ServerCommandFrame,
    uint ServerReceivedClientCommandFrame,
    Vector2 Position
);

static partial class Log
{
    [LoggerMessage(Message = "Received a valid packet from server for sequence: {sequence}.")]
    public static partial void PacketRecieved(this ILogger logger, LogLevel logLevel, uint sequence);

    [LoggerMessage(Message = "Got server state for command frame: {commandFrame} (idx: {idx}), Position: {position}.")]
    public static partial void LocalPlayerStateReceived(this ILogger logger, LogLevel logLevel, uint commandFrame, uint idx, Vector2 position);
}

/// <summary>
/// Handles messages from the network manager indicating new world state has arrived.
/// <para>
/// Will update the "master game" and add a new snapshot into the dtate buffer.
/// </para>
/// </summary>
public class WorldStateReceivedSystem : MoonTools.ECS.System
{
    readonly PlayerEntityMapper _playerEntityMapper;
    readonly CircularBuffer<ServerPlayerState> _serverPlayerStateSnapshots;

    public WorldStateReceivedSystem(
        World world,
        PlayerEntityMapper playerEntityMapper,
        CircularBuffer<ServerPlayerState> serverPlayerStateSnapshots
    ) : base(world)
    {
        _playerEntityMapper = playerEntityMapper;
        _serverPlayerStateSnapshots = serverPlayerStateSnapshots;
    }

    public override void Update(TimeSpan delta)
    {
        ref var simulationState = ref GetSingleton<SimulationStateComponent>();

        foreach (var message in ReadMessages<ReceivedWorldStateMessage>())
        {
            if (UDPHelper.IsValidPacket(message.ServerCommandFrame, simulationState.LastReceivedServerCommandFrame) == false)
                continue;

            Logger.Log.PacketRecieved(logLevel: LogLevel.Debug, sequence: message.ServerCommandFrame);

            var entity = _playerEntityMapper.GetEntityFromClientId(message.ClientId);

            if (entity == PlayerEntityMapper.INVALID_ENTITY)
                continue;

            if (Has<PlayerInputComponent>(entity))
            {
                simulationState.LastReceivedServerCommandFrame = Math.Max(message.ServerCommandFrame, simulationState.LastReceivedServerCommandFrame);
                simulationState.ServerReceivedClientCommandFrame = Math.Max(message.ServerReceivedClientCommandFrame, message.ServerReceivedClientCommandFrame);

                if (simulationState.LastReceivedServerCommandFrame < simulationState.InitialClientCommandFrame)
                    continue;

                var serverPlayerState = new ServerPlayerState(message.Position);
                var idx = _serverPlayerStateSnapshots.Set(simulationState.LastReceivedServerCommandFrame, serverPlayerState);

                Logger.Log.LocalPlayerStateReceived(LogLevel.Debug, simulationState.LastReceivedServerCommandFrame, idx, serverPlayerState.Position);

                var actual = simulationState.CurrentClientCommandFrame - simulationState.LastReceivedServerCommandFrame;
                var desired = NetworkSettings.COMMAND_BUFFER_SIZE + 3;

                if (simulationState.CurrentClientCommandFrame > simulationState.LastReceivedServerCommandFrame + NetworkSettings.COMMAND_BUFFER_SIZE + 3)
                    ((ClientGame)BaseGame.Instance).ChangeSimulationRate(-1, desired, actual);
                else if (simulationState.CurrentClientCommandFrame < simulationState.LastReceivedServerCommandFrame + NetworkSettings.COMMAND_BUFFER_SIZE + 3)
                    ((ClientGame)BaseGame.Instance).ChangeSimulationRate(1, desired, actual);
            }
            else
            {
                // Remote player
                if (Has<PositionComponent>(entity))
                {
                    ref var position = ref Get<PositionComponent>(entity);
                    position.Value = message.Position;
                }
            }
        }
    }
}
