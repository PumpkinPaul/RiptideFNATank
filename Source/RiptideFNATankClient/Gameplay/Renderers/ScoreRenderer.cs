// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using Wombat.Engine;
using RiptideFNATank.Gameplay.Components;

namespace RiptideFNATank.Gameplay.Renderers;

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