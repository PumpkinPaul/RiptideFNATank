// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using Wombat.Engine;
using System;
using RiptideFNATankClient.Gameplay.Components;

namespace RiptideFNATankClient.Gameplay.Systems;

/// <summary>
/// Responsible for moving entities by updating their positions from their velocity.
/// </summary>
public sealed class BounceSystem : MoonTools.ECS.System
{
    readonly Filter _filter;

    public BounceSystem(World world) : base(world)
    {
        _filter = FilterBuilder
            .Include<DirectionalSpeedComponent>()
            .Include<BounceResponseComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in _filter.Entities)
        {
            ref readonly var directionalSpeed = ref Get<DirectionalSpeedComponent>(entity);
            ref readonly var bounce = ref Get<BounceResponseComponent>(entity);

            var edgeNormal = Vector2.Zero;
            if (bounce.CollisionEdge.HasFlag(CollisionEdge.Left))
                edgeNormal = new Vector2(1, 0);
            else if (bounce.CollisionEdge.HasFlag(CollisionEdge.Right))
                edgeNormal = new Vector2(-1, 0);

            if (bounce.CollisionEdge.HasFlag(CollisionEdge.Top))
                edgeNormal = new Vector2(0, -1);
            else if (bounce.CollisionEdge.HasFlag(CollisionEdge.Bottom))
                edgeNormal = new Vector2(0, 1);

            var directionVector = VectorHelper.Polar(directionalSpeed.DirectionInRadians, directionalSpeed.Speed);
            var newDirectionVector = Vector2.Reflect(directionVector, edgeNormal);

            var angle = VectorHelper.GetAngle(newDirectionVector);

            Set(entity, new DirectionalSpeedComponent(angle, directionalSpeed.Speed));

            Remove<BounceResponseComponent>(entity);
        }
    }
}
