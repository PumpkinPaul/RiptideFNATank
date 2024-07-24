// Copyright Pumpkin Games Ltd. All Rights Reserved.

using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankClient.Networking;
using System;

namespace RiptideFNATankClient.Gameplay.Systems;

/// <summary>
/// Applies smoothing by interpolating the display state somewhere
/// in between the previous state and current simulation state.
/// </summary>
public sealed class PlayerNetworkRemoteUpdateRemoteSystem : UpdatePaddleStateSystem
{
    readonly NetworkOptions _networkOptions;

    readonly Filter _filter;

    public PlayerNetworkRemoteUpdateRemoteSystem(
        World world,
        NetworkOptions networkOptions
    ) : base(world)
    {
        _networkOptions = networkOptions;

        _filter = FilterBuilder
            .Include<SmoothingComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (_networkOptions.EnablePrediction == false)
            return;

        foreach (var entity in _filter.Entities)
        {
            // Predict how the remote paddle will move by updating our local copy of its simultation state.
            ref var simulationState = ref GetMutable<SimulationStateComponent>(entity);
            UpdateState(entity, ref simulationState.PaddleState);

            // If both smoothing and prediction are active, also apply prediction to the previous state.
            ref readonly var smoothing = ref Get<SmoothingComponent>(entity);
            if (smoothing.Value > 0)
            {
                ref var previousState = ref GetMutable<PreviousStateComponent>(entity);
                UpdateState(entity, ref previousState.PaddleState);
            }
        }
    }
}
