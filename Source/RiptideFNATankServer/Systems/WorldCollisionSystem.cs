// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using RiptideFNATankServer.Gameplay.Components;
using Wombat.Engine;

namespace RiptideFNATankServer.Gameplay.Systems;

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

            //Clamp the velocity to take the entity to the collision point...
            Set(entity, new VelocityComponent(newVelocity));
        }
    }
}
