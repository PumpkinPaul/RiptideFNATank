// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using RiptideFNATank.Gameplay.Components;
using System;

namespace RiptideFNATank.Gameplay.Systems;

public readonly record struct BallSpawnMessage(
    Vector2 Position,
    Color Color
);

/// <summary>
/// Responsible for spawning Ball entities with the correct components.
/// </summary>
public class BallSpawnSystem : MoonTools.ECS.System
{
    readonly Random _random;
    public BallSpawnSystem(World world) : base(world)
    {
        _random = new Random();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var message in ReadMessages<BallSpawnMessage>())
        {
            var entity = CreateEntity();

            Set(entity, new PositionComponent(message.Position));
            Set(entity, new ScaleComponent(new Vector2(16, 16)));
            Set(entity, new ColorComponent(message.Color));
            Set(entity, new VelocityComponent());
            Set(entity, new CanBounceComponent());
            //Set(entity, new ColliderComponent(
            //    EntityCollisionType.Avatar,
            //    EntityCollisionType.Enemy | EntityCollisionType.EnemyBullet,
            //    new BoxF(-4, -8, 8, 13)));

            var direction = (_random.NextSingle() - 0.5f) * 2 * MathHelper.PiOver4;
            direction += _random.Next(0, 2) < 1 ? MathHelper.Pi : 0;

            Set(entity, new DirectionalSpeedComponent(
                DirectionInRadians: direction,
                Speed: 5));
        }
    }
}