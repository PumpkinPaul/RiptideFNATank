// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankClient.Networking;
using System;

namespace RiptideFNATankClient.Gameplay.Systems;

/// <summary>
/// Applies smoothing by interpolating the display state somewhere
/// in between the previous state and current simulation state.
/// </summary>
public sealed class PlayerNetworkRemoteApplySmoothingSystem : UpdatePaddleStateSystem
{
    readonly NetworkOptions _networkOptions;

    readonly Filter _filter;

    public PlayerNetworkRemoteApplySmoothingSystem(
        World world,
        NetworkOptions networkOptions
    ) : base(world)
    {
        _networkOptions = networkOptions;

        _filter = FilterBuilder
            .Include<SmoothingComponent>()
            .Include<SimulationStateComponent>()
            .Include<PreviousStateComponent>()
            .Include<DisplayStateComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (_networkOptions.EnableSmoothing == false)
            return;

        foreach (var entity in _filter.Entities)
        {
            ref readonly var smoothing = ref Get<SmoothingComponent>(entity);
            ref readonly var simulationState = ref Get<SimulationStateComponent>(entity);
            ref readonly var previousState = ref Get<PreviousStateComponent>(entity);
            ref var displayState = ref GetMutable<DisplayStateComponent>(entity);

            displayState.PaddleState.Position = Vector2.Lerp(simulationState.PaddleState.Position, previousState.PaddleState.Position, smoothing.Value);
            displayState.PaddleState.Velocity = Vector2.Lerp(simulationState.PaddleState.Velocity, previousState.PaddleState.Velocity, smoothing.Value);
        }
    }
}
