// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using Wombat.Engine;
using Wombat.Engine.Extensions;
using RiptideFNATankClient.Gameplay.Components;

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
            if (Has<DisplayStateComponent>(entity))
            {
                ref readonly var displayState = ref Get<DisplayStateComponent>(entity);
                pos = displayState.PaddleState.Position;
            }

            var halfSize = scale.Value / 2;
            var bounds = new BoxF(-halfSize, scale.Value);
            _spriteBatch.DrawFilledBox(pos, bounds, color.Value);
        }
    }
}