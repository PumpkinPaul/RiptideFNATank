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
using Microsoft.Xna.Framework;
using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankCommon.Gameplay.Components;
using RiptideFNATankCommon.Gameplay.Systems;
using RiptideFNATankCommon.Networking;
using System;
using Wombat.Engine;
using Wombat.Engine.Logging;

namespace RiptideFNATankClient.Gameplay.Systems;

static partial class Log
{
    [LoggerMessage(Message = "Local client prediction error at command frame: {commandFrame}. Predicted {predictedPosition} vs actual {actualPosition}")]
    public static partial void ClientPredictionError(this ILogger logger, LogLevel logLevel, uint commandFrame, Vector2 predictedPosition, Vector2 actualPosition);
}

/// <summary>
/// Reconciles the local player's predicted state with the actual world state received from the server.
/// </summary>
public sealed class ReconcilePredictedStateSystem : MoonTools.ECS.System
{
    readonly CircularBuffer<PlayerCommandsComponent> _localPlayerActionsSnapshots;
    readonly CircularBuffer<LocalPlayerPredictedState> _localPlayerStateSnapshots;
    readonly CircularBuffer<ServerPlayerState> _serverPlayerStateSnapshots;

    readonly Filter _filter;

    readonly MoonTools.ECS.System[] _systems;

    public ReconcilePredictedStateSystem(
        World world,
        CircularBuffer<PlayerCommandsComponent> localPlayerActionsSnapshots,
        CircularBuffer<LocalPlayerPredictedState> localPlayerStateSnapshots,
        CircularBuffer<ServerPlayerState> serverPlayerStateSnapshots
    ) : base(world)
    {
        _localPlayerActionsSnapshots = localPlayerActionsSnapshots;
        _localPlayerStateSnapshots = localPlayerStateSnapshots;
        _serverPlayerStateSnapshots = serverPlayerStateSnapshots;

        _filter = FilterBuilder
            .Include<PlayerCommandsComponent>()
            .Build();

        _systems = [
            // Process the actions (e.g. do a jump, fire a gun, move forward, etc).
            new ProcessPlayerCommandsSystem(world),

            // Turn directions into velocity!
            new DirectionalSpeedSystem(world),

            // Collisions processors.
            new WorldCollisionSystem(world, new Point(BaseGame.SCREEN_WIDTH, BaseGame.SCREEN_HEIGHT)),
            new EntityCollisionSystem(world),

            // Move the entities in the world.
            new MovementSystem(world)
        ];
    }

    public override void Update(TimeSpan delta)
    {
        ref readonly var simulationState = ref GetSingleton<SimulationStateComponent>();

        foreach (var entity in _filter.Entities)
        {
            var predictedState = _localPlayerStateSnapshots.Get(simulationState.LastReceivedServerCommandFrame);
            var serverState = _serverPlayerStateSnapshots.Get(simulationState.LastReceivedServerCommandFrame);

            if (predictedState.Position != serverState.Position)
            {
                Logger.Log.ClientPredictionError(LogLevel.Error, simulationState.LastReceivedServerCommandFrame, predictedState.Position, serverState.Position);

                // Get the state of the player at the current client command frame. 
                // We will want to lerp the DisplayState FROM that falsely predited position TO the actual position over time.
                ref var position = ref Get<PositionComponent>(entity);
                var displayPosition = position.Value;

                if (Has<DisplayStateComponent>(entity))
                {
                    ref readonly var displayState = ref Get<DisplayStateComponent>(entity);
                    displayPosition = displayState.Position;
                }

                // Reset the client state to the server state as the server is the authority
                position.Value = serverState.Position;

                // Playback the client actions
                for (var commandFrame = simulationState.LastReceivedServerCommandFrame + 1; commandFrame <= simulationState.CurrentClientCommandFrame; commandFrame++)
                {
                    foreach(var system in _systems)
                        system.Update(delta);

                    // This is the local player state snaphot system mostly
                    var localPlayerState = new LocalPlayerPredictedState(
                        position.Value);

                    _localPlayerStateSnapshots.Set(commandFrame /* apart from this bit */, localPlayerState);

                    // We will need to lerp the display position to the actual position over some frames
                    Set(entity, new DisplayStateComponent {
                        Position = displayPosition
                    });

                    Set(entity, new LerpPositionComponent(
                        ToPosition: localPlayerState.Position,
                        FromPosition: displayPosition
                    ));
                }
            }
        }
    }
}
