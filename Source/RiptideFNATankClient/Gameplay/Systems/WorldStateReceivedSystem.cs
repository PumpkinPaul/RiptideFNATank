/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankCommon;
using RiptideFNATankCommon.Components;
using RiptideFNATankCommon.Networking;
using System;

namespace RiptideFNATankClient.Gameplay.Systems;

public readonly record struct ReceivedWorldStateMessage(
    ushort ClientId,
    uint ServerCommandFrame,
    uint ServerReceivedClientCommandFrame,
    Vector2 Position
);

public record struct SimulationStateComponent(
    uint InitialClientCommandFrame,
    uint LastReceivedServerCommandFrame,
    uint CurrentClientCommandFrame,
    uint ServerReceivedClientCommandFrame
);

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
        // TODO: I think we need to buffer these to avoid jitter.
        var span = ReadMessages<ReceivedWorldStateMessage>();
        if (span.Length > 1)
        {
            Logger.Warning($"Received multiple server messages");
        }

        ref var simulationState = ref GetSingleton<SimulationStateComponent>();

        foreach (var message in span)
        {
            if (UDPHelper.IsValidPacket(message.ServerCommandFrame, simulationState.LastReceivedServerCommandFrame) == false)
                continue;

            //Logger.Info($"Received a valid packet from server for sequence: {message.ServerCommandFrame}.");

            var entity = _playerEntityMapper.GetEntityFromClientId(message.ClientId);

            if (entity == PlayerEntityMapper.INVALID_ENTITY)
                continue;

            if (Has<PlayerInputComponent>(entity))
            {
                simulationState.LastReceivedServerCommandFrame = message.ServerCommandFrame;
             
                if (message.ServerReceivedClientCommandFrame > simulationState.ServerReceivedClientCommandFrame)
                    simulationState.ServerReceivedClientCommandFrame = message.ServerReceivedClientCommandFrame;

                if (simulationState.ServerReceivedClientCommandFrame > 0)
                {
                    var serverPlayerState = new ServerPlayerState(message.Position);
                    var idx = _serverPlayerStateSnapshots.Set(simulationState.ServerReceivedClientCommandFrame, serverPlayerState);

                    Logger.Info($"Wrote server state snpshot for ServerReceivedClientCommandFrame: {simulationState.ServerReceivedClientCommandFrame}, resolves to idx: {idx}, CurrentClientCommandFrame: {simulationState.CurrentClientCommandFrame}, Position: {serverPlayerState.Position}");
                }
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
