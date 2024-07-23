// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using Wombat.Engine;
using RiptideFNATank.Gameplay.Components;
using System;

namespace RiptideFNATank.Gameplay.Systems;

/// <summary>
/// Responsible for performing entity to world collision calculations.
/// </summary>
/// <remarks>
/// This is VERY simple and not at all accurate but it will keeps bats and balls in the play area
/// </remarks>
public sealed class WorldCollisionSystem : MoonTools.ECS.System
{
    readonly GameState _gameState;
    Point _worldSize;
    readonly Filter _filter;

    public WorldCollisionSystem(
        World world,
        GameState gameState,
    Point worldSize) : base(world)
    {
        _gameState = gameState;
        _worldSize = worldSize;

        _filter = FilterBuilder
            .Include<PositionComponent>()
            .Include<ScaleComponent>()
            .Include<VelocityComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in _filter.Entities)
        {
            ref readonly var position = ref Get<PositionComponent>(entity);
            ref readonly var scale = ref Get<ScaleComponent>(entity);
            ref readonly var velocity = ref Get<VelocityComponent>(entity);

            //This is our intended location...
            var desiredPosition = position.Value + velocity.Value;

            //...derive a bounding box in world space for the entity
            var halfSize = scale.Value / 2;
            var bounds = new BoxF(-halfSize, scale.Value);
            var worldBounds = bounds.Translated(desiredPosition);

            //Real simple basic bounds checking and velocity modification
            var collisionEdge = CollisionEdge.None;
            var newVelocity = velocity.Value;

            var player1ScoreIncrement = 0;
            var player2ScoreIncrement = 0;

            if (velocity.Value.X < 0 && worldBounds.Left < 0)
            {
                newVelocity.X -= worldBounds.Left;
                collisionEdge |= CollisionEdge.Left;

                player2ScoreIncrement = 1;
            }
            else if (velocity.Value.X > 0 && worldBounds.Right > _worldSize.X)
            {
                newVelocity.X -= worldBounds.Right - _worldSize.X;
                collisionEdge |= CollisionEdge.Right;

                player1ScoreIncrement = 1;
            }

            if (player1ScoreIncrement > 0 || player2ScoreIncrement > 0)
                Send(new GoalScoredMessage(player1ScoreIncrement, player2ScoreIncrement));

            if (velocity.Value.Y < 0 && worldBounds.Bottom < 0)
            {
                newVelocity.Y -= worldBounds.Bottom;
                collisionEdge |= CollisionEdge.Bottom;
            }
            else if (velocity.Value.Y > 0 && worldBounds.Top > _worldSize.Y)
            {
                newVelocity.Y -= worldBounds.Top - _worldSize.Y;
                collisionEdge |= CollisionEdge.Top;
            }

            //Clamp the velocity to take the entity to the collision point...
            Set(entity, new VelocityComponent(newVelocity));

            if (collisionEdge != CollisionEdge.None && Has<CanBounceComponent>(entity))
                Set(entity, new BounceResponseComponent(collisionEdge));
        }
    }
}
