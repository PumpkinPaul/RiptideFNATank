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
    uint ServerTick,
    uint ClientTick,
    Vector2 Position
);

public record struct SimulationStateComponent(
    uint InitialServerTick,
    uint LastReceivedServerTick,
    uint CurrentClientTick,
    uint ServerProcessedClientInputAtClientTick
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
        //TODO: I think we need to buffer these to avoid jitter.
        var span = ReadMessages<ReceivedWorldStateMessage>();
        if (span.Length > 1)
        {
            Logger.Warning($"Received multiple server messages");
        }

        ref var simulationState = ref GetSingleton<SimulationStateComponent>();

        foreach (var message in span)
        {
            var entity = _playerEntityMapper.GetEntityFromClientId(message.ClientId);

            if (entity == PlayerEntityMapper.INVALID_ENTITY)
                continue;

            if (Has<PlayerInputComponent>(entity))
            {
                // Local player
                // Ensure the new state > the last state received
                if (message.ServerTick < simulationState.LastReceivedServerTick)
                {
                    // Discard packet
                    Logger.Warning($"Received an old packet from server for sequence: {message.ServerTick}. Client has already received state for sequence: {simulationState.LastReceivedServerTick}.");
                }
                else if (message.ServerTick == simulationState.LastReceivedServerTick)
                {
                    // Duplicate packet?
                    Logger.Warning($"Received a duplicate packet from server for sequence: {message.ServerTick}.");
                }
                else //else if (newState.sequence > lastState.sequence)
                {
                    //Logger.Info($"Received a new packet from server for sequence: {message.ServerTick}.");

                    //_masterWorldState = new WorldState
                    //{
                    //    WorldTick = message.ServerTick
                    //};

                    //if (_masterWorldState.PlayerStates.TryGetValue(message.ClientId, out var lastPlayerState) == false)
                    //{
                    //    lastPlayerState = new();
                    //    _masterWorldState.PlayerStates[message.ClientId] = lastPlayerState;
                    //}

                    simulationState.LastReceivedServerTick = message.ServerTick;
                    simulationState.ServerProcessedClientInputAtClientTick = message.ClientTick;

                    if (simulationState.ServerProcessedClientInputAtClientTick > 0)
                    {

                        var serverPlayerState = new ServerPlayerState(message.Position);
                        var idx = _serverPlayerStateSnapshots.Set(simulationState.ServerProcessedClientInputAtClientTick, serverPlayerState);

                        Logger.Info($"Wrote server state snpshot for ServerProcessedClientInputAtClientTick: {simulationState.ServerProcessedClientInputAtClientTick}, resolves to idx: {idx}, CurrentClientTick: {simulationState.CurrentClientTick}, Position: {serverPlayerState.Position}");
                    }
                }

                continue;
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
