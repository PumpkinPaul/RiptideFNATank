/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankCommon.Gameplay.Components;
using System;

namespace RiptideFNATankCommon.Gameplay.Systems;

/// <summary>
/// Interpolates a local entity's display state towards the correct authoritative state over a series of updates.
/// </summary>
public sealed class LerpDisplayStateSystem : MoonTools.ECS.System
{
    /// <summary>
    /// The time it takes to smoothly interpolate to the entity's actual position when receiving corrected data.
    /// </summary>
    const float LERP_TIME_IN_SECONDS = 0.05f;

    readonly Filter _filter;

    public LerpDisplayStateSystem(World world) : base(world)
    {
        _filter = FilterBuilder
            .Include<LerpPositionComponent>()
            .Include<DisplayStateComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in _filter.Entities)
        {
            ref var lerp = ref Get<LerpPositionComponent>(entity);
            ref var displayState = ref Get<DisplayStateComponent>(entity);

            lerp.Timer += (float)delta.TotalSeconds;

            // Interpolate the player's position based on the lerp timer progress.
            displayState.Position = Vector2.Lerp(lerp.FromPosition, lerp.ToPosition, lerp.Timer / LERP_TIME_IN_SECONDS);

            // If we have reached the end of the lerp timer, explicitly force the player to the last known correct position.
            if (lerp.Timer >= LERP_TIME_IN_SECONDS)
            {
                displayState.Position = lerp.ToPosition;
                Remove<LerpPositionComponent>(entity);
                Remove<DisplayStateComponent>(entity);
            }
        }
    }
}
