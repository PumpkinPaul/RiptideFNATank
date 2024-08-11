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
using Wombat.Engine.Logging;

namespace RiptideFNATankClient.Gameplay.Systems;

static partial class Log
{
    [LoggerMessage(Message = "Local client prediction error at command frame: {commandFrame}. Predicted {predictedPosition} vs actual {actualPosition}")]
    public static partial void ClientPredictionError(this ILogger logger, LogLevel logLevel, uint commandFrame, Vector2 predictedPosition, Vector2 actualPosition);
}

/// <summary>
/// Reconciles the local player's predicted state with the actual world state received from the server.
/// </summary>
public sealed class ReconcilePredictedStateSystem : MoonTools.ECS.System
{
    readonly CircularBuffer<PlayerCommandsComponent> _localPlayerActionsSnapshots;
    readonly CircularBuffer<LocalPlayerPredictedState> _localPlayerStateSnapshots;
    readonly CircularBuffer<ServerPlayerState> _serverPlayerStateSnapshots;

    readonly Filter _filter;

    public ReconcilePredictedStateSystem(
        World world,
        CircularBuffer<PlayerCommandsComponent> localPlayerActionsSnapshots,
        CircularBuffer<LocalPlayerPredictedState> localPlayerStateSnapshots,
        CircularBuffer<ServerPlayerState> serverPlayerStateSnapshots
    ) : base(world)
    {
        _localPlayerActionsSnapshots = localPlayerActionsSnapshots;
        _localPlayerStateSnapshots = localPlayerStateSnapshots;
        _serverPlayerStateSnapshots = serverPlayerStateSnapshots;

        _filter = FilterBuilder
            .Include<PlayerCommandsComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        ref readonly var simulationState = ref GetSingleton<SimulationStateComponent>();

        foreach (var entity in _filter.Entities)
        {
            var predictedState = _localPlayerStateSnapshots.Get(simulationState.LastReceivedServerCommandFrame);
            var serverState = _serverPlayerStateSnapshots.Get(simulationState.LastReceivedServerCommandFrame);

            if (predictedState.Position != serverState.Position)
            {
                // TODO: Reconcile and replay the local input
                Logger.Log.ClientPredictionError(LogLevel.Error, simulationState.LastReceivedServerCommandFrame, predictedState.Position, serverState.Position);
            }
        }
    }
}
