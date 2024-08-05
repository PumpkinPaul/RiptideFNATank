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
using RiptideFNATankCommon.Components;
using RiptideFNATankCommon.Networking;
using System;

namespace RiptideFNATankClient.Gameplay.Systems;

/// <summary>
/// Responsible for checking input devices (mouse, keyboard, gamepad) and converting the button presses, etc into game actions.
/// </summary>
/// <example>
/// Check the state of the 'Q' key and turn it into a 'move up' command if it is pressed.
/// </example>
public sealed class SnapshotLocalPlayerActionsSystem : MoonTools.ECS.System
{
    readonly CircularBuffer<PlayerActionsComponent> _playerActions;

    readonly Filter _filter;

    public SnapshotLocalPlayerActionsSystem(
        World world,
        CircularBuffer<PlayerActionsComponent> playerActions) : base(world)
    {
        _playerActions = playerActions;

        _filter = FilterBuilder
            .Include<PlayerActionsComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        ref readonly var simulationState = ref GetSingleton<SimulationStateComponent>();

        foreach (var entity in _filter.Entities)
        {
            ref readonly var playerActions = ref Get<PlayerActionsComponent>(entity);

            // Cache the action...
            // ...so that we have a store of inputs we can send to the server to protect against packet loss
            // ...a stream of actions that we can use to replay client inputs when reconciling state updates (server disagress with predicted client state)
            _playerActions.Set(simulationState.CurrentClientCommandFrame, playerActions);
        }
    }
}
