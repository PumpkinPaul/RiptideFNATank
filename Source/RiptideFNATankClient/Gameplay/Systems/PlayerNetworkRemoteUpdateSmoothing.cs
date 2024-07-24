// Copyright Pumpkin Games Ltd. All Rights Reserved.

using MoonTools.ECS;
using Wombat.Engine;
using System;
using RiptideFNATankClient.Gameplay.Components;

namespace RiptideFNATankClient.Gameplay.Systems;

/// <summary>
/// Applies smoothing by interpolating the display state somewhere
/// in between the previous state and current simulation state.
/// </summary>
public sealed class PlayerNetworkRemoteUpdateSmoothingSystem : UpdatePaddleStateSystem
{

    readonly Filter _filter;

    public PlayerNetworkRemoteUpdateSmoothingSystem(World world) : base(world)
    {
        _filter = FilterBuilder
            .Include<SmoothingComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in _filter.Entities)
        {
            // Update the smoothing amount, which interpolates from the previous
            // state toward the current simultation state. The speed of this decay
            // depends on the number of frames between packets: we want to finish
            // our smoothing interpolation at the same time the next packet is due.
            //float smoothingDecay = 1.0f / framesBetweenPackets;
            float smoothingDecay = (float)BaseGame.Instance.TargetElapsedTime.TotalSeconds * PlayerNetworkSendLocalStateSystem.UPDATES_PER_SECOND;

            ref var smoothing = ref GetMutable<SmoothingComponent>(entity);

            smoothing.Value -= smoothingDecay;

            if (smoothing.Value < 0)
                smoothing.Value = 0;

            if (smoothing.Value == 0)
            {
                ref var simulationState = ref GetMutable<SimulationStateComponent>(entity);
                Set(entity, new DisplayStateComponent
                {
                    PaddleState = simulationState.PaddleState
                });
            }
        }
    }
}
