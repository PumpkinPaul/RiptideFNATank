// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using RiptideFNATank.Gameplay.Components;
using System;

namespace RiptideFNATank.Gameplay.Systems;

/// <summary>
/// Interpolates a network entity's position from a source to a target postion over a series of updates
/// </summary>
public sealed class LerpPositionSystem : MoonTools.ECS.System
{
    /// <summary>
    /// The speed (in seconds) in which to smoothly interpolate to the entity's actual position when receiving corrected data.
    /// </summary>
    const float LERP_TIME = 0.025f;

    readonly Filter _filter;

    public LerpPositionSystem(World world) : base(world)
    {
        _filter = FilterBuilder
            .Include<LerpPositionComponent>()
            .Include<PositionComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in _filter.Entities)
        {
            ref var lerp = ref GetMutable<LerpPositionComponent>(entity);
            ref readonly var position = ref Get<PositionComponent>(entity);

            // Interpolate the player's position based on the lerp timer progress.
            var newPosition = Vector2.Lerp(lerp.FromPosition, lerp.ToPosition, lerp.Timer / LERP_TIME);
            lerp.Timer += (float)delta.TotalSeconds;

            // If we have reached the end of the lerp timer, explicitly force the player to the last known correct position.
            if (lerp.Timer >= LERP_TIME)
            {
                newPosition = lerp.ToPosition;
                Remove<LerpPositionComponent>(entity);
            }

            Set(entity, new PositionComponent(newPosition));
        }
    }
}
