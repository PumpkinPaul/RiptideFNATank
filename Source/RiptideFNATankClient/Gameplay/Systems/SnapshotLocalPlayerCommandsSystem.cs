/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Microsoft.Extensions.Logging;
using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankCommon.Gameplay.Components;
using RiptideFNATankCommon.Networking;
using System;
using Wombat.Engine.Logging;

namespace RiptideFNATankClient.Gameplay.Systems;

static partial class Log
{
    [LoggerMessage(Message = "Snapshot local commands for command frame: {CurrentClientCommandFrame}, idx: {idx}, {playerCommands}.")]
    public static partial void SnapshotPlayerCommands(this ILogger logger, LogLevel logLevel, uint currentClientCommandFrame, uint idx, PlayerCommandsComponent playerCommands);
}

/// <summary>
/// Responsible for checking input devices (mouse, keyboard, gamepad) and converting the button presses, etc into game actions.
/// </summary>
/// <example>
/// Check the state of the 'Q' key and turn it into a 'move up' command if it is pressed.
/// </example>
public sealed class SnapshotLocalPlayerCommandsSystem : MoonTools.ECS.System
{
    readonly CircularBuffer<PlayerCommandsComponent> _playerActions;

    readonly Filter _filter;

    public SnapshotLocalPlayerCommandsSystem(
        World world,
        CircularBuffer<PlayerCommandsComponent> playerActions) : base(world)
    {
        _playerActions = playerActions;

        _filter = FilterBuilder
            .Include<PlayerCommandsComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        ref readonly var simulationState = ref GetSingleton<SimulationStateComponent>();

        foreach (var entity in _filter.Entities)
        {
            ref readonly var playerActions = ref Get<PlayerCommandsComponent>(entity);

            // Cache the action...
            // ...so that we have a store of inputs we can send to the server to protect against packet loss
            // ...a stream of actions that we can use to replay client inputs when reconciling state updates (server disagress with predicted client state)
            var idx = _playerActions.Set(simulationState.CurrentClientCommandFrame, playerActions);

            Logger.Log.SnapshotPlayerCommands(LogLevel.Information, simulationState.CurrentClientCommandFrame, idx, playerActions);
        }
    }
}
