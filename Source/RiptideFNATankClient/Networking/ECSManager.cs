// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MoonTools.ECS;
using Wombat.Engine;
using Wombat.Engine.Extensions;
using System.Collections.Generic;
using RiptideFNATankClient.Gameplay.Renderers;
using RiptideFNATankClient.Gameplay.Systems;

namespace RiptideFNATankClient.Networking;

/// <summary>
/// Encapsulates management of the ECS
/// </summary>
public class ECSManager
{
    readonly Timekeeper _timekeeper = new();
    readonly NetworkOptions _networkOptions;
    readonly World _world;

    //Systems
    readonly MoonTools.ECS.System[] _systems;

    //Renderers
    readonly SpriteRenderer _spriteRenderer;

    readonly PlayerEntityMapper _playerEntityMapper;
    readonly NetworkGameManager _networkGameManager;
    readonly MultiplayerGameState _gameState;

    readonly Queue<LocalPlayerSpawnMessage> _localPlayerSpawnMessages = new();
    readonly Queue<RemotePlayerSpawnMessage> _remotePlayerSpawnMessages = new();
    readonly Queue<ReceivedRemotePaddleStateMessage> _matchDataVelocityAndPositionMessage = new();
    readonly Queue<MatchDataDirectionAndPositionMessage> _matchDataDirectionAndPositionMessage = new();
    readonly Queue<DestroyEntityMessage> _destroyEntityMessage = new();

    public ECSManager(
        NetworkGameManager networkGameManager,
        PlayerEntityMapper playerEntityMapper,
        MultiplayerGameState gameState)
    {
        var game = BaseGame.Instance;

        _networkGameManager = networkGameManager;
        _playerEntityMapper = playerEntityMapper;
        _gameState = gameState;

        _world = new World();

        _networkOptions = new NetworkOptions
        {
            EnablePrediction = false,
            EnableSmoothing = true
        };

        _systems = new MoonTools.ECS.System[]
        {
            //Spawn the entities into the game world
            new LocalPlayerSpawnSystem(_world),
            new RemotePlayerSpawnSystem(_world, _playerEntityMapper),
            new BallSpawnSystem(_world),
            new ScoreSpawnSystem(_world),

            new PlayerInputSystem(_world),   //Get input from devices and turn into game actions...
            new PlayerActionsSystem(_world), //...then process the actions (e.g. do a jump, fire a gun, etc)

            //Turn directions into velocity!
            new DirectionalSpeedSystem(_world),

            //Collisions processors
            new WorldCollisionSystem(_world, _gameState, new Point(BaseGame.SCREEN_WIDTH, BaseGame.SCREEN_HEIGHT)),
            new EntityCollisionSystem(_world, BaseGame.SCREEN_WIDTH),

            //Move the entities in the world
            new MovementSystem(_world),
            new BounceSystem(_world),
            new AngledBounceSystem(_world),

            //LateUpdate
            //...handle sending data to remote clients
            new GoalScoredLocalSyncSystem(_world, _networkGameManager, _gameState),
            new BallNetworkLocalSyncSystem(_world, _networkGameManager),

            //Phase #1
            //This is UpdateLocal gamer from Riptide.Tank
            new PlayerNetworkSendLocalStateSystem(_world, _networkGameManager, _timekeeper),

            //Phase #2
            //...handle receiving data from remote clients
            new PlayerNetworkRemoteResetSmoothingSystem(_world, _networkOptions),  //Reset the smoothing factor
            new PlayerNetworkRemoteSyncSystem(_world),            //Update the 'simulation' state
            new PlayerNetworkRemoteApplyPredictionSystem(_world, _timekeeper, _networkOptions, game.TargetElapsedTime), //Apply client side predication to the 'simulation' state
            
            //Phase #3
            new PlayerNetworkRemoteUpdateSmoothingSystem(_world),
            new PlayerNetworkRemoteUpdateRemoteSystem(_world, _networkOptions),
            new PlayerNetworkRemoteApplySmoothingSystem(_world, _networkOptions),

            new BallNetworkRemoteSyncSystem(_world),
            new LerpPositionSystem(_world),

            //Remove the dead entities
            new DestroyEntitySystem(_world)
        };

        _spriteRenderer = new SpriteRenderer(_world, BaseGame.Instance.SpriteBatch);

        var color = Color.Cyan;

        _world.Send(new BallSpawnMessage(
            Position: new Vector2(BaseGame.SCREEN_WIDTH, BaseGame.SCREEN_HEIGHT) / 2,
            color
        ));

        _world.Send(new ScoreSpawnMessage(
            PlayerIndex: PlayerIndex.One,
            Position: new Vector2(BaseGame.SCREEN_WIDTH * 0.25f, 21)
        ));

        _world.Send(new ScoreSpawnMessage(
            PlayerIndex: PlayerIndex.Two,
            Position: new Vector2(BaseGame.SCREEN_WIDTH * 0.75f, 21)
        ));
    }

    public void SpawnLocalPlayer(Vector2 position)
    {
        //Queue entity creation in the ECS
        _localPlayerSpawnMessages.Enqueue(new LocalPlayerSpawnMessage(
            PlayerIndex: PlayerIndex.One,
            MoveUpKey: Keys.Q,
            MoveDownKey: Keys.A,
            Position: position,
            Color.Red
        ));
    }

    public void SpawnRemotePlayer(Vector2 position)
    {
        //Queue entity creation in the ECS
        _remotePlayerSpawnMessages.Enqueue(new RemotePlayerSpawnMessage(
            PlayerIndex: PlayerIndex.Two,
            Position: position,
            Color.Blue
        ));
    }

    public void ReceivedRemotePaddleState(ReceivedRemotePaddleStateEventArgs e, ushort clientId)
    {
        var entity = _playerEntityMapper.GetEntityFromClientId(clientId);

        if (entity == PlayerEntityMapper.INVALID_ENTITY)
            return;

        //Queue entity to begin lerping to the corrected position.
        _matchDataVelocityAndPositionMessage.Enqueue(new ReceivedRemotePaddleStateMessage(
            entity,
            e.TotalSeconds,
            e.Position,
            e.Velocity,
            e.MoveUp,
            e.MoveDown
        ));
    }

    public void ReceivedRemoteBallState(float direction, Vector2 position)
    {
        _matchDataDirectionAndPositionMessage.Enqueue(new MatchDataDirectionAndPositionMessage(
            direction,
            position
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
        SendMessages(_matchDataVelocityAndPositionMessage);
        SendMessages(_matchDataDirectionAndPositionMessage);
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

        //Draw the world

        //...all the entities
        _spriteRenderer.Draw();

        //...play area
        spriteBatch.DrawLine(new Vector2(BaseGame.SCREEN_WIDTH / 2, 0), new Vector2(BaseGame.SCREEN_WIDTH / 2, BaseGame.SCREEN_HEIGHT), Color.Cyan);

        //..."HUD"
        spriteBatch.DrawText(Resources.GameFont, _gameState.Player1Score.ToString(), new Vector2(BaseGame.SCREEN_WIDTH * 0.25f, BaseGame.SCREEN_HEIGHT - 48), Color.Cyan, Alignment.Centre);
        spriteBatch.DrawText(Resources.GameFont, _gameState.Player2Score.ToString(), new Vector2(BaseGame.SCREEN_WIDTH * 0.75f, BaseGame.SCREEN_HEIGHT - 48), Color.Cyan, Alignment.Centre);

        //...help text
        spriteBatch.DrawText(Resources.SmallFont, "Z Smoothing", new Vector2(BaseGame.SCREEN_WIDTH * 0.25f, BaseGame.SCREEN_HEIGHT - 92), _networkOptions.EnableSmoothing ? Color.Cyan : Color.Gray, Alignment.Centre);
        spriteBatch.DrawText(Resources.SmallFont, "X Prediction", new Vector2(BaseGame.SCREEN_WIDTH * 0.75f, BaseGame.SCREEN_HEIGHT - 92), _networkOptions.EnablePrediction ? Color.Cyan : Color.Gray, Alignment.Centre);
        spriteBatch.End();
    }
}
