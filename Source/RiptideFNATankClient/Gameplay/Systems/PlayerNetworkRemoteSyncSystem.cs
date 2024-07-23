// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using RiptideFNATank.Gameplay.Components;
using System;

namespace RiptideFNATank.Gameplay.Systems;

public readonly record struct ReceivedRemotePaddleStateMessage(
    Entity Entity,
    float TotalSeconds,
    Vector2 Position,
    Vector2 Velocity,
    bool MoveUp,
    bool MoveDown
);

/// <summary>
/// Reads remote match data received messages and applies the new values to the 'simulation state' - e.g. the normal component data
/// </summary>
public sealed class PlayerNetworkRemoteSyncSystem : MoonTools.ECS.System
{
    public PlayerNetworkRemoteSyncSystem(World world) : base(world)
    {
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var message in ReadMessages<ReceivedRemotePaddleStateMessage>())
        {
            ref var simulationState = ref GetMutable<SimulationStateComponent>(message.Entity);

            // Read simulation state from the network packet.
            simulationState.PaddleState.Position = message.Position;
            simulationState.PaddleState.Velocity = message.Velocity;

            // Read remote inputs from the network packet.
            simulationState.PaddleState.MoveUp = message.MoveUp;
            simulationState.PaddleState.MoveDown = message.MoveDown;
        }
    }
}
