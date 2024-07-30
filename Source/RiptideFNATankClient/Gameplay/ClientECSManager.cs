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
using Microsoft.Xna.Framework.Input;
using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankClient.Gameplay.Renderers;
using RiptideFNATankClient.Gameplay.Systems;
using RiptideFNATankClient.Networking;
using RiptideFNATankCommon.Components;
using RiptideFNATankCommon.Gameplay;
using RiptideFNATankCommon.Networking;
using RiptideFNATankCommon.Systems;
using System.Collections.Generic;
using Wombat.Engine;
using Wombat.Engine.Extensions;

namespace RiptideFNATankClient.Gameplay;

/// <summary>
/// Encapsulates management of the ECS
/// </summary>
public class ClientECSManager
{
    readonly Timekeeper _timekeeper = new();
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
    readonly PlayerActionsComponent[] _localPlayerActionsSnapshots = new PlayerActionsComponent[1024];
    readonly PlayerState[] _localPlayerStateSnapshots = new PlayerState[1024];

    // Queue for incoming world states.
    readonly Queue<WorldState> _worldStateQueue = new();

    // The last received server world tick.
    // TODO Not public
    public int _lastServerWorldTick = 0;

    // The last tick that the server has acknowledged our input for.
    int _lastAckedInputTick = 0;

    // Queued network messages
    readonly Queue<LocalPlayerSpawnMessage> _localPlayerSpawnMessages = new();
    readonly Queue<RemotePlayerSpawnMessage> _remotePlayerSpawnMessages = new();
    readonly Queue<ReceivedWorldStateMessage> _remoteWorldStateMessages = new();
    readonly Queue<DestroyEntityMessage> _destroyEntityMessage = new();

    // Netcode
    readonly NetworkTimer _networkTimer = new(serverTickRateInFPS: NetworkSettings.SERVER_FPS);
    CircularBuffer<ReceivedWorldStateMessage> _clientStateBuffer;
    CircularBuffer<PlayerActionsComponent> clientInputBuffer;

    public ClientECSManager(
        NetworkGameManager networkGameManager,
        PlayerEntityMapper playerEntityMapper)
    {
        _networkGameManager = networkGameManager;
        _playerEntityMapper = playerEntityMapper;

        _world = new World();

        // Add a single for common simulation state
        // e.g.
        // Current simulation tick
        // Last received server sequence
        // etc
        _world.Set(_world.CreateEntity(), new SimulationStateComponent());

        _networkOptions = new NetworkOptions
        {
            EnablePrediction = false,
            EnableSmoothing = true
        };

        _systems = [
            new WorldStateReceivedSystem(_world, _worldStateQueue,  _playerEntityMapper),

            //Spawn the entities into the game world
            new LocalPlayerSpawnSystem(_world, _playerEntityMapper),
            new RemotePlayerSpawnSystem(_world, _playerEntityMapper),

            new PlayerInputSystem(_world, _localPlayerActionsSnapshots),   //Get input from devices and turn into game actions...            
            
            // ====================================================================================================
            // World Simulation Start

            new PlayerActionsSystem(_world, isClient: true), //...then process the actions (e.g. do a jump, fire a gun, etc)

            //Turn directions into velocity!
            new DirectionalSpeedSystem(_world),

            //Collisions processors
            new WorldCollisionSystem(_world, _worldStateQueue, new Point(BaseGame.SCREEN_WIDTH, BaseGame.SCREEN_HEIGHT)),
            new EntityCollisionSystem(_world, BaseGame.SCREEN_WIDTH),

            //Move the entities in the world
            new MovementSystem(_world),

            //Remove the dead entities
            new DestroyEntitySystem(_world),

            // World Simulation End
            // ====================================================================================================

            //...handle sending player commands to the server
            new PlayerSendNetworkCommandsSystem(_world, _networkGameManager, _timekeeper),

            new LerpPositionSystem(_world),
        ];

        _spriteRenderer = new SpriteRenderer(_world, BaseGame.Instance.SpriteBatch);
    }

    public void SpawnLocalPlayer(ReceivedSpawnPlayerEventArgs e)
    {
        //Queue entity creation in the ECS
        _localPlayerSpawnMessages.Enqueue(new LocalPlayerSpawnMessage(
            ClientId: e.ClientId,
            InitialServerSequenceId: e.InitialServerSequenceId,
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
            ServerSequenceId: e.ServerSequenceId,
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
        _timekeeper.GameTime = gameTime;

        SendAllQueuedMessages();

        foreach (var system in _systems)
            system.Update(BaseGame.Instance.TargetElapsedTime);

        _world.FinishUpdate();
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
