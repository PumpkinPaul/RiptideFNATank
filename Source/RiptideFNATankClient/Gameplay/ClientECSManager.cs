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
using RiptideFNATankClient.Gameplay.Renderers;
using RiptideFNATankClient.Gameplay.Systems;
using RiptideFNATankClient.Networking;
using RiptideFNATankCommon.Networking;
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

    readonly PlayerEntityMapper _playerEntityMapper;
    readonly NetworkGameManager _networkGameManager;
    readonly MultiplayerGameState _gameState;

    readonly Queue<LocalPlayerSpawnMessage> _localPlayerSpawnMessages = new();
    readonly Queue<RemotePlayerSpawnMessage> _remotePlayerSpawnMessages = new();
    readonly Queue<DestroyEntityMessage> _destroyEntityMessage = new();

    public ClientECSManager(
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

        _systems = [
            //Spawn the entities into the game world
            new LocalPlayerSpawnSystem(_world),
            new RemotePlayerSpawnSystem(_world, _playerEntityMapper),
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

            //...handle sending data to the server
            new PlayerSendNetworkStateSystem(_world, _networkGameManager, _timekeeper),

            new LerpPositionSystem(_world),

            //Remove the dead entities
            new DestroyEntitySystem(_world)
        ];

        _spriteRenderer = new SpriteRenderer(_world, BaseGame.Instance.SpriteBatch);

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

    public void SpawnRemotePlayer(ushort clientId, Vector2 position)
    {
        //Queue entity creation in the ECS
        _remotePlayerSpawnMessages.Enqueue(new RemotePlayerSpawnMessage(
            ClientId: clientId,
            Position: position,
            Color.Blue
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
