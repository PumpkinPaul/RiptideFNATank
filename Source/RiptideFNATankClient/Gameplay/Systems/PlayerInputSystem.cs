/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Microsoft.Xna.Framework.Input;
using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankCommon.Components;
using System;

namespace RiptideFNATankClient.Gameplay.Systems;

/// <summary>
/// Responsible for checking input devices, converting button presses, etc into game actions.
/// </summary>
/// <example>
/// Check the state of the 'Q' key and turn it into a 'move up' command if it is pressed.
/// </example>
public sealed class PlayerInputSystem : MoonTools.ECS.System
{
    readonly PlayerActionsComponent[] _playerActions;

    readonly Filter _filter;

    public PlayerInputSystem(
        World world,
        PlayerActionsComponent[] playerActions) : base(world)
    {
        _playerActions = playerActions;

        _filter = FilterBuilder
            .Include<PlayerInputComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        var keyBoardState = Keyboard.GetState();

        foreach (var entity in _filter.Entities)
        {
            ref var playerInput = ref Get<PlayerInputComponent>(entity);

            var moveUp = keyBoardState.IsKeyDown(playerInput.MoveUpKey);
            var moveDown = keyBoardState.IsKeyDown(playerInput.MoveDownKey);

            var playerActionsThisFrame = new PlayerActionsComponent(
                moveUp,
                moveDown);

            Set(entity, playerActionsThisFrame);

            // Add the player action into the buffer too.
            _playerActions[0] = playerActionsThisFrame;
        }
    }
}
