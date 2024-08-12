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
using Microsoft.Xna.Framework.Input;
using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankClient.Gameplay.Renderers;
using RiptideFNATankClient.Gameplay.Systems;
using RiptideFNATankClient.Networking;
using RiptideFNATankCommon.Gameplay.Components;
using RiptideFNATankCommon.Gameplay.Systems;
using RiptideFNATankCommon.Networking;
using System.Collections.Generic;
using Wombat.Engine;
using Wombat.Engine.Extensions;
using Wombat.Engine.Logging;

namespace RiptideFNATankClient.Gameplay;

static partial class Log
{
    [LoggerMessage(Message = "CommandBuffer is empty.")]
    public static partial void CommandBufferIsEmpty(this ILogger logger, LogLevel logLevel);

    [LoggerMessage(Message = "CommandBuffer only has future commands.")]
    public static partial void CommandBufferOnlyHasFutureCommands(this ILogger logger, LogLevel logLevel);
}

/// <summary>
/// Encapsulates management of the ECS
/// </summary>
public class ClientECSManager
{
    readonly NetworkOptions _networkOptions;
    readonly World _world;

    //Systems
    readonly MoonTools.ECS.System[] _systems;

    //Renderers
    readonly SpriteRenderer _spriteRenderer;

    // DI
    readonly PlayerEntityMapper _playerEntityMapper;
    readonly NetworkGameManager _networkGameManager;

    // Snapshot buffers for input and state used for prediction & replay.
    
    // A buffer of client inputs
    readonly CircularBuffer<PlayerCommandsComponent> _localPlayerActionsSnapshots = new(1024);

    // Predicted client state - client writes this after simulation so that it can be verified with the server state.
    readonly CircularBuffer<LocalPlayerPredictedState> _localPlayerStateSnapshots = new (1024);

    // Authoritative state received from the server 
    readonly CircularBuffer<ServerPlayerState> _serverPlayerStateSnapshots = new(1024);

    // Queued network messages
    readonly Queue<LocalPlayerSpawnMessage> _localPlayerSpawnMessages = new();
    readonly Queue<RemotePlayerSpawnMessage> _remotePlayerSpawnMessages = new();
    readonly Queue<ReceivedWorldStateMessage> _remoteWorldStateMessages = new();
    readonly Queue<DestroyEntityMessage> _destroyEntityMessage = new();
    
    public ClientECSManager(
        NetworkGameManager networkGameManager,
        PlayerEntityMapper playerEntityMapper)
    {
        _networkGameManager = networkGameManager;
        _playerEntityMapper = playerEntityMapper;

        _networkOptions = new NetworkOptions
        {
            EnablePrediction = false,
            EnableSmoothing = true
        };

        _world = new World();

        // Add a singleton (I think) for common simulation state.
        // e.g.
        // Current command frame
        // Last received server command frame
        // etc
        _world.Set(_world.CreateEntity(), new SimulationStateComponent());

        _systems = [
            new WorldStateReceivedSystem(_world, _playerEntityMapper, _serverPlayerStateSnapshots),
            new ReconcilePredictedStateSystem(_world, _localPlayerActionsSnapshots, _localPlayerStateSnapshots, _serverPlayerStateSnapshots),

            // Spawn the entities into the game world.
            new LocalPlayerSpawnSystem(_world, _playerEntityMapper),
            new RemotePlayerSpawnSystem(_world, _playerEntityMapper),

            // Get input from devices and turn into game actions.
            new PlayerInputSystem(_world),
            new SnapshotLocalPlayerCommandsSystem(_world, _localPlayerActionsSnapshots),
            
            // ====================================================================================================
            // World Simulation Start
            // The following systems should be the same between client and server to get a consistent game.

            // Process the actions (e.g. do a jump, fire a gun, move forward, etc).
            new ProcessPlayerCommandsSystem(_world),

            // Turn directions into velocity!
            new DirectionalSpeedSystem(_world),

            // Collisions processors.
            new WorldCollisionSystem(_world, new Point(BaseGame.SCREEN_WIDTH, BaseGame.SCREEN_HEIGHT)),
            new EntityCollisionSystem(_world),

            // Move the entities in the world.
            new MovementSystem(_world),

            // Remove the dead entities.
            new DestroyEntitySystem(_world),

            // World Simulation End
            // ====================================================================================================

            // Cache the player state (for the local player) - used for server state reconciliation.
            new SnapshotLocalPlayerPredictedStateSystem(_world, _localPlayerStateSnapshots),

            // Send player game actions to the server - do this after writing the predication snapshot!
            new PlayerSendNetworkCommandsSystem(_world, _localPlayerActionsSnapshots, _networkGameManager),

            new LerpPositionSystem(_world),
        ];

        _spriteRenderer = new SpriteRenderer(_world, BaseGame.Instance.SpriteBatch);
    }

    public void SpawnLocalPlayer(ReceivedSpawnPlayerEventArgs e)
    {
        //Queue entity creation in the ECS
        _localPlayerSpawnMessages.Enqueue(new LocalPlayerSpawnMessage(
            ClientId: e.ClientId,
            InitialServerCommandFrame: e.InitialServerCommandFrame,
            PlayerIndex: PlayerIndex.One,
            MoveUpKey: Keys.Q,
            MoveDownKey: Keys.A,
            Position: e.Position,
            Color.Red
        ));
    }

    public void SpawnRemotePlayer(ReceivedSpawnPlayerEventArgs e)
    {
        //Queue entity creation in the ECS
        _remotePlayerSpawnMessages.Enqueue(new RemotePlayerSpawnMessage(
            ClientId: e.ClientId,
            Position: e.Position,
            Color.Blue
        ));
    }

    public void ReceivedWorldState(ReceivedWorldStateEventArgs e)
    {
        // Server snapshot received
        _remoteWorldStateMessages.Enqueue(new ReceivedWorldStateMessage(
            ClientId: e.ClientId,
            ServerCommandFrame: e.ServerCommandFrame,
            ServerReceivedClientCommandFrame: e.ClientCommandFrame,
            Position: e.Position
        ));
    }

    public void DestroyEntity(ushort clientId)
    {
        var entity = _playerEntityMapper.GetEntityFromClientId(clientId);

        if (entity == PlayerEntityMapper.INVALID_ENTITY)
            return;

        _playerEntityMapper.RemovePlayerByClientId(clientId);

        //Queue entity to begin lerping to the corrected position.
        _destroyEntityMessage.Enqueue(new DestroyEntityMessage(
            Entity: entity
        ));
    }

    public void Update(GameTime gameTime)
    {
        // How do we feel about this being outside of a system?
        ref var simulationState = ref _world.GetSingleton<SimulationStateComponent>();

        using (Logger.Log.BeginScope(("Frame", simulationState.CurrentClientCommandFrame)))
        {
            SendAllQueuedMessages();

            // Always use the physics tick even when catching up?
            foreach (var system in _systems)
                system.Update(NetworkSettings.PhsyicsTimeSpan);

            _world.FinishUpdate();

            simulationState.CurrentClientCommandFrame++;
        }
    }

    private void SendAllQueuedMessages()
    {
        SendMessages(_localPlayerSpawnMessages);
        SendMessages(_remotePlayerSpawnMessages);
        SendMessages(_remoteWorldStateMessages);
        SendMessages(_destroyEntityMessage);
    }

    private void SendMessages<T>(Queue<T> messages) where T : unmanaged
    {
        while (messages.Count > 0)
            _world.Send(messages.Dequeue());
    }

    public void TogglePrediction()
    {
        _networkOptions.EnablePrediction = !_networkOptions.EnablePrediction;
    }

    public void ToggleSmoothing()
    {
        _networkOptions.EnableSmoothing = !_networkOptions.EnableSmoothing;
    }

    public void Draw()
    {
        var spriteBatch = BaseGame.Instance.SpriteBatch;

        spriteBatch.BeginTextRendering();

        //Draw the world...
        _spriteRenderer.Draw();

        //..."HUD"
        //spriteBatch.DrawText(Resources.GameFont, _gameState.Player1Score.ToString(), new Vector2(BaseGame.SCREEN_WIDTH * 0.25f, BaseGame.SCREEN_HEIGHT - 48), Color.Red, Alignment.Centre);
        //spriteBatch.DrawText(Resources.GameFont, _gameState.Player2Score.ToString(), new Vector2(BaseGame.SCREEN_WIDTH * 0.75f, BaseGame.SCREEN_HEIGHT - 48), Color.Blue, Alignment.Centre);

        spriteBatch.End();
    }
}
