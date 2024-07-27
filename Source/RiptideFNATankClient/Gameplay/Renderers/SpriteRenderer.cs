/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using RiptideFNATankCommon.Components;
using Wombat.Engine;
using Wombat.Engine.Extensions;

namespace RiptideFNATankClient.Gameplay.Renderers;

public class SpriteRenderer : Renderer
{
    readonly SpriteBatch _spriteBatch;
    readonly Filter _filter;

    public SpriteRenderer(
        World world,
        SpriteBatch spriteBatch
    ) : base(world)
    {
        _spriteBatch = spriteBatch;

        _filter = FilterBuilder
            .Include<PositionComponent>()
            .Include<ScaleComponent>()
            .Include<ColorComponent>()
            .Build();
    }

    public void Draw()
    {
        foreach (var entity in _filter.Entities)
        {
            ref readonly var position = ref Get<PositionComponent>(entity);
            ref readonly var scale = ref Get<ScaleComponent>(entity);
            ref readonly var color = ref Get<ColorComponent>(entity);

            var pos = position.Value;
            //if (Has<DisplayStateComponent>(entity))
            {
                //ref readonly var displayState = ref Get<DisplayStateComponent>(entity);
                //pos = displayState.PaddleState.Position;
            }

            var halfSize = scale.Value / 2;
            var bounds = new BoxF(-halfSize, scale.Value);
            _spriteBatch.DrawFilledBox(pos, bounds, color.Value);
        }
    }
}