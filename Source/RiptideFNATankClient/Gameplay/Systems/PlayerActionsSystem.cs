// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using System;

namespace RiptideFNATankClient.Gameplay.Systems;

/// <summary>
/// Handles player actions (initiate a jump, fire a weapon, move a paddle)
/// </summary>
public sealed class PlayerActionsSystem : MoonTools.ECS.System
{
    public const int PADDLE_SPEED = 5;

    readonly Filter _filter;

    public PlayerActionsSystem(World world) : base(world)
    {
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
        }
    }
}
