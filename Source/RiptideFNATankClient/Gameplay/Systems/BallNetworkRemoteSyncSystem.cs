// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using System;

namespace RiptideFNATankClient.Gameplay.Systems;

public readonly record struct MatchDataDirectionAndPositionMessage(
    float Direction,
    Vector2 LerpToPosition
);

/// <summary>
/// Reads remote match data received messages and tags each relevant entity with a lerpig component to smooth the movement.
/// </summary>
public sealed class BallNetworkRemoteSyncSystem : MoonTools.ECS.System
{
    readonly Filter _filter;

    public BallNetworkRemoteSyncSystem(World world) : base(world)
    {
        _filter = FilterBuilder
            .Include<PositionComponent>()
            .Include<DirectionalSpeedComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var message in ReadMessages<MatchDataDirectionAndPositionMessage>())
        {
            var ball = _filter.NthEntity(0);

            ref readonly var position = ref Get<PositionComponent>(ball);
            ref readonly var direction = ref Get<DirectionalSpeedComponent>(ball);

            Set(ball, new DirectionalSpeedComponent(
                direction.DirectionInRadians,
                direction.Speed));

            Set(ball, new LerpPositionComponent
            {
                ToPosition = message.LerpToPosition,
                FromPosition = position.Value
            });
        }
    }
}
