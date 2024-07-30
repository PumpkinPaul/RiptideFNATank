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
using RiptideFNATankCommon.Gameplay;
using RiptideFNATankCommon.Networking;
using System;
using System.Collections.Generic;

namespace RiptideFNATankClient.Gameplay.Systems;

public readonly record struct ReceivedWorldStateMessage(
    ushort ClientId,
    uint ServerSequenceId,
    Vector2 Position
);

public record struct SimulationStateComponent(
    uint InitialServerSequenceId,
    uint LastReceivedServerSequenceId,
    uint CurrentWorldTick
);

/// <summary>
/// Handles messages from the network manager indicating new world state has arrived.
/// <para>
/// Will update the "master game" and add a new snapshot into the dtate buffer.
/// </para>
/// </summary>
public class WorldStateReceivedSystem : MoonTools.ECS.System
{
    WorldState _masterWorldState = new();
    readonly Queue<WorldState> _worldStates;

    readonly PlayerEntityMapper _playerEntityMapper;

    public WorldStateReceivedSystem(
        World world,
        Queue<WorldState> worldStates,
        PlayerEntityMapper playerEntityMapper
    ) : base(world)
    {
        _worldStates = worldStates;
        _playerEntityMapper = playerEntityMapper;
    }

    public override void Update(TimeSpan delta)
    {
        //TODO: I think we need to buffer these to avoid jitter.
        foreach (var message in ReadMessages<ReceivedWorldStateMessage>())
        {
            var entity = _playerEntityMapper.GetEntityFromClientId(message.ClientId);

            if (entity == PlayerEntityMapper.INVALID_ENTITY)
                continue;

            ref var simulationState = ref GetSingleton<SimulationStateComponent>();

            if (Has<PlayerInputComponent>(entity))
            {
                // Local player
                // Ensure the new state > the last state received
                if (message.ServerSequenceId < simulationState.LastReceivedServerSequenceId)
                {
                    // Discard packet
                    Logger.Warning($"Received an old packet from server for sequence: {message.ServerSequenceId}. Client has already received state for sequence: {simulationState.LastReceivedServerSequenceId}.");
                }
                else if (message.ServerSequenceId == simulationState.LastReceivedServerSequenceId)
                {
                    // Duplicate packet?
                    Logger.Warning($"Received a duplicate packet from server for sequence: {message.ServerSequenceId}.");
                }
                else //else if (newState.sequence > lastState.sequence)
                {
                    //Logger.Info($"Received a new packet from server for sequence: {message.ServerSequenceId}.");

                    //_masterWorldState = new WorldState
                    //{
                    //    WorldTick = message.ServerSequenceId
                    //};

                    //if (_masterWorldState.PlayerStates.TryGetValue(message.ClientId, out var lastPlayerState) == false)
                    //{
                    //    lastPlayerState = new();
                    //    _masterWorldState.PlayerStates[message.ClientId] = lastPlayerState;
                    //}

                    simulationState.LastReceivedServerSequenceId = message.ServerSequenceId;
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
