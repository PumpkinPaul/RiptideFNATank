// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework.Input;
using MoonTools.ECS;
using RiptideFNATank.Gameplay.Components;
using System;

namespace RiptideFNATank.Gameplay.Systems;

/// <summary>
/// Responsible for checking input devices, converting button presses, etc into game actions.
/// </summary>
/// <example>
/// Check the state of the 'Q' key and turn it into a 'move up' command if it is pressed.
/// </example>
public sealed class PlayerInputSystem : MoonTools.ECS.System
{
    readonly Filter _filter;

    public PlayerInputSystem(World world) : base(world)
    {
        _filter = FilterBuilder
            .Include<PlayerInputComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        var keyBoardState = Keyboard.GetState();

        foreach (var entity in _filter.Entities)
        {
            ref readonly var playerInput = ref Get<PlayerInputComponent>(entity);

            var moveUp = keyBoardState.IsKeyDown(playerInput.MoveUpKey);
            var moveDown = keyBoardState.IsKeyDown(playerInput.MoveDownKey);

            Set(entity, new PlayerActionsComponent(
                moveUp,
                moveDown));            
        }
    }
}
