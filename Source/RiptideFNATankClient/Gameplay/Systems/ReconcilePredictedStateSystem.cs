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
using RiptideFNATankCommon;
using RiptideFNATankCommon.Components;
using RiptideFNATankCommon.Gameplay;
using RiptideFNATankCommon.Networking;
using System;

namespace RiptideFNATankClient.Gameplay.Systems;

/// <summary>
/// Reconciles the local player's predicted state with the actual world state received from the server.
/// </summary>
public sealed class ReconcilePredictedStateSystem : MoonTools.ECS.System
{
    readonly CircularBuffer<PlayerActionsComponent> _localPlayerActionsSnapshots;
    readonly CircularBuffer<LocalPlayerPredictedState> _localPlayerStateSnapshots;
    readonly CircularBuffer<ServerPlayerState> _serverPlayerStateSnapshots;

    readonly Filter _filter;

    public ReconcilePredictedStateSystem(
        World world,
        CircularBuffer<PlayerActionsComponent> localPlayerActionsSnapshots,
        CircularBuffer<LocalPlayerPredictedState> localPlayerStateSnapshots,
        CircularBuffer<ServerPlayerState> serverPlayerStateSnapshots
    ) : base(world)
    {
        _localPlayerActionsSnapshots = localPlayerActionsSnapshots;
        _localPlayerStateSnapshots = localPlayerStateSnapshots;
        _serverPlayerStateSnapshots = serverPlayerStateSnapshots;

        _filter = FilterBuilder
            .Include<PlayerActionsComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        ref readonly var simulationState = ref GetSingleton<SimulationStateComponent>();

        if (simulationState.LastReceivedServerTick == 0)
            return;

        foreach (var entity in _filter.Entities)
        {
            // Note - currently client and server tick clocks are sync'd
            var predictedState = _localPlayerStateSnapshots.Get(simulationState.ServerProcessedClientInputAtClientTick);
            var serverState = _serverPlayerStateSnapshots.Get(simulationState.ServerProcessedClientInputAtClientTick);

            if (predictedState.Position != serverState.Position)
            {
                // Reconcile and replay the local input
                Logger.Error($"Local client prediction error at ServerProcessedClientInputAtClientTick: {simulationState.ServerProcessedClientInputAtClientTick}");
                Logger.Warning($"Predicted position: {predictedState.Position} vs actual position: {serverState.Position}");
            }
        }
    }
}
