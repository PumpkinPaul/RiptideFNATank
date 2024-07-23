// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using Wombat.Engine;
using Wombat.Engine.Extensions;
using RiptideFNATank.Gameplay.Components;
using System;

namespace RiptideFNATank.Gameplay.Systems;

/// <summary>
/// Responsible for moving entities by updating their positions from their velocity.
/// </summary>
public sealed class AngledBounceSystem : MoonTools.ECS.System
{
    readonly Filter _filter;

    public AngledBounceSystem(World world) : base(world)
    {
        _filter = FilterBuilder
            .Include<DirectionalSpeedComponent>()
            .Include<AngledBounceResponseComponent>()
            .Include<PositionComponent>()
            .Include<VelocityComponent>()
            .Include<ScaleComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in _filter.Entities)
        {
            ref readonly var directionalSpeed = ref Get<DirectionalSpeedComponent>(entity);
            ref readonly var bounce = ref Get<AngledBounceResponseComponent>(entity);

            ref readonly var position = ref Get<PositionComponent>(entity);
            ref readonly var velocity = ref Get<VelocityComponent>(entity);
            ref readonly var scale = ref Get<ScaleComponent>(entity);

            ref readonly var bouncedByPosition = ref Get<PositionComponent>(bounce.BouncedBy);
            ref readonly var bouncedByScale = ref Get<ScaleComponent>(bounce.BouncedBy);

            var bouncedY = position.Value.Y + scale.Value.Y / 2;
            var bouncerY = bouncedByPosition.Value.Y + bouncedByScale.Value.Y / 2;

            var horizontal = velocity.Value.X < 0 ? 1 : -1;
            var diff = bouncedY - bouncerY;
            var mod = (float)diff / (bouncedByScale.Value.Y / 2);
            var rotation = mod * MathHelper.Pi / 4;

            Vector2 newVelocity = new Vector2(directionalSpeed.Speed * 1.0f, 0).Rotate((float)rotation) * new Vector2(horizontal, 1);

            Set(entity, new DirectionalSpeedComponent(VectorHelper.GetAngle(newVelocity), directionalSpeed.Speed));

            Remove<AngledBounceResponseComponent>(entity);
        }
    }
}
