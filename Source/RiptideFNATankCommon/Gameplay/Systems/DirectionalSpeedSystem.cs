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
using RiptideFNATankCommon.Gameplay.Components;
using Wombat.Engine;

namespace RiptideFNATankCommon.Gameplay.Systems;

/// <summary>
/// Responsible for turning directional speed into a velocity.
/// </summary>
public class DirectionalSpeedSystem : MoonTools.ECS.System
{
    readonly Filter _filter;

    public DirectionalSpeedSystem(World world) : base(world)
    {
        _filter = FilterBuilder
           .Include<DirectionalSpeedComponent>()
           .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in _filter.Entities)
        {
            ref readonly var component = ref Get<DirectionalSpeedComponent>(entity);

            Set(entity, new VelocityComponent(VectorHelper.Polar(component.DirectionInRadians, component.Speed)));
        }
    }
}