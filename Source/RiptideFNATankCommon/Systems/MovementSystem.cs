/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using MoonTools.ECS;
using RiptideFNATankCommon.Components;

namespace RiptideFNATankCommon.Systems;

/// <summary>
/// Responsible for moving entities by updating their positions from their velocity.
/// </summary>
public sealed class MovementSystem : MoonTools.ECS.System
{
    readonly Filter _filter;

    public MovementSystem(World world) : base(world)
    {
        _filter = FilterBuilder
            .Include<PositionComponent>()
            .Include<VelocityComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in _filter.Entities)
        {
            ref readonly var position = ref Get<PositionComponent>(entity);
            ref readonly var velocity = ref Get<VelocityComponent>(entity);

            Set(entity, new PositionComponent(position.Value + velocity.Value));
        }
    }
}
