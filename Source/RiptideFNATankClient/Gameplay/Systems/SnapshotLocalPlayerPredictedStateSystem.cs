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
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankCommon;
using RiptideFNATankCommon.Components;
using RiptideFNATankCommon.Networking;
using System;

namespace RiptideFNATankClient.Gameplay.Systems;

/// <summary>
/// Caches player state in the snaphot buffer.
/// </summary>
public sealed class SnapshotLocalPlayerPredictedStateSystem : MoonTools.ECS.System
{
    readonly CircularBuffer<LocalPlayerPredictedState> _localPlayerStateSnapshots;

    readonly Filter _filter;

    public SnapshotLocalPlayerPredictedStateSystem(
        World world,
        CircularBuffer<LocalPlayerPredictedState> localPlayerStateSnapshots
    ) : base(world)
    {
        _localPlayerStateSnapshots = localPlayerStateSnapshots;

        _filter = FilterBuilder 
            .Include<PlayerInputComponent>()
            .Include<PositionComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        ref readonly var simulationState = ref GetSingleton<SimulationStateComponent>();

        foreach (var entity in _filter.Entities)
        {
            ref readonly var position = ref Get<PositionComponent>(entity);

            var localPlayerState = new LocalPlayerPredictedState(
                position.Value);

            var idx = _localPlayerStateSnapshots.Set(simulationState.CurrentClientTick, localPlayerState);
            Logger.Info($"Wrote local player state CurrentClientTick: {simulationState.CurrentClientTick}, resolves to idx: {idx}, LastReceivedServerTick: {simulationState.LastReceivedServerTick}, Position: {localPlayerState.Position}");
        }
    }
}