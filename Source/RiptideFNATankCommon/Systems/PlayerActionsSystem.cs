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
using RiptideFNATankCommon.Components;

namespace RiptideFNATankCommon.Systems;

/// <summary>
/// Handles player actions (initiate a jump, fire a weapon, move a paddle)
/// </summary>
public sealed class PlayerActionsSystem : MoonTools.ECS.System
{
    public const int PADDLE_SPEED = 5;

    readonly Filter _filter;

    //TODO: HACK
    readonly bool _isClient;

    public PlayerActionsSystem(World world, bool isClient) : base(world)
    {
        _isClient = isClient;

        _filter = FilterBuilder
            .Include<PlayerActionsComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in _filter.Entities)
        {
            ref readonly var gameInput = ref Get<PlayerActionsComponent>(entity);

            var moveUpSpeed = gameInput.MoveUp ? PADDLE_SPEED : 0;
            var moveDownSpeed = gameInput.MoveDown ? -PADDLE_SPEED : 0;

            Set(entity, new VelocityComponent(
                new Vector2(0, moveUpSpeed + moveDownSpeed)));

            if (!_isClient)
                Set(entity, new PlayerActionsComponent());
        }
    }
}
