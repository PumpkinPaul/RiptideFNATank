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
using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using RiptideFNATankCommon.Components;
using Wombat.Engine;

namespace RiptideFNATankClient.Gameplay.Renderers;

public class ScoreRenderer : Renderer
{
    readonly SpriteBatch _spriteBatch;
    readonly Filter _filter;

    public ScoreRenderer(
        World world,
        SpriteBatch spriteBatch
    ) : base(world)
    {
        _spriteBatch = spriteBatch;

        _filter = FilterBuilder
            .Include<PositionComponent>()
            .Include<ScoreComponent>()
            .Build();
    }

    public void Draw()
    {
        foreach (var entity in _filter.Entities)
        {
            ref readonly var position = ref Get<PositionComponent>(entity);
            _spriteBatch.DrawString(Resources.GameFont, "0", position.Value, Color.Cyan);
        }
    }
}