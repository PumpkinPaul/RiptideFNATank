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
using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using RiptideFNATankCommon.Gameplay;
using RiptideFNATankCommon.Networking;
using RiptideFNATankCommon.Systems;
using RiptideFNATankServer.Gameplay.Renderers;
using RiptideFNATankServer.Gameplay.Systems;
using RiptideFNATankServer.Networking;
using Wombat.Engine;
using Wombat.Engine.Extensions;
using static RiptideFNATankServer.Networking.ServerNetworkManager;

namespace RiptideFNATankServer.Gameplay;

public record struct SimulationStateComponent(
    uint CurrentServerTick
);

/// <summary>
/// Encapsulates management of the ECS
/// </summary>
public class ServerECSManager
{
    readonly World _world;

    //Systems
    readonly MoonTools.ECS.System[] _systems;

    //Renderers
    readonly SpriteRenderer _spriteRenderer;
    SpriteBatch _spriteBatch;

    readonly PlayerEntityMapper _playerEntityMapper;
    readonly ServerNetworkManager _networkGameManager;
    readonly WorldState _gameState;

    readonly Queue<PlayerSpawnMessage> _queuedPlayerSpawnMessages = new();
    readonly Queue<ClientStateReceivedMessage> _queuedClientStateMessages = new();
    readonly Queue<DestroyEntityMessage> _destroyEntityMessage = new();

    // This is possibly temp while I try to figure this stuff out
    // Master gamestate
    // Player snapshots
    // Dummy gamestate
    readonly Dictionary<ushort, uint> _clientAcks = [];

    public ServerECSManager(
        ServerNetworkManager networkGameManager,
        PlayerEntityMapper playerEntityMapper,
        SpriteBatch spriteBatch)
    {
        _networkGameManager = networkGameManager;
        _playerEntityMapper = playerEntityMapper;
        _gameState = new WorldState();
        _spriteBatch = spriteBatch;

        _world = new World();

        // Add a singleton (I think) for common simulation state.
        // e.g.
        // Current simulation tick
        // Last received server tick
        // etc
        _world.Set(_world.CreateEntity(), new SimulationStateComponent());

        _systems = [
            // State from client
            // TODO: how to update the master state!
            new ClientStateReceivedSystem(_world, _clientAcks),

            //Spawn the entities into the game world
            new PlayerSpawnSystem(_world, _playerEntityMapper),

            // ====================================================================================================
            // World Simulation Start
            // The following systems should be the same between client and server to get a consistent game

            new PlayerActionsSystem(_world, isClient: false), //...then process the actions (e.g. do a jump, fire a gun, etc)

            // Turn directions into velocity!
            new DirectionalSpeedSystem(_world),

            // Collisions processors
            new WorldCollisionSystem(_world, new Point(BaseGame.SCREEN_WIDTH, BaseGame.SCREEN_HEIGHT)),
            new EntityCollisionSystem(_world),

            // Move the entities in the world
            new MovementSystem(_world),

            // Remove the dead entities
            new DestroyEntitySystem(_world),

            // World Simulation End
            // ====================================================================================================

            // Handle sending server world state data to remote clients
            new SendNetworkWorldStateSystem(_world, _networkGameManager, _playerEntityMapper, _clientAcks),
        ];

        _spriteRenderer = new SpriteRenderer(_world, spriteBatch);
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
        SendAllQueuedECSMessages();

        foreach (var system in _systems)
            system.Update(gameTime.ElapsedGameTime);

        // How do we feel about this being outside of a system?
        ref var simulationState = ref _world.GetSingleton<SimulationStateComponent>();
        simulationState.CurrentServerTick++;

        ServerGame.ServerTick = simulationState.CurrentServerTick++;

        _world.FinishUpdate();
    }

    private void SendAllQueuedECSMessages()
    {
        SendMessages(_queuedPlayerSpawnMessages);
        SendMessages(_queuedClientStateMessages);
        SendMessages(_destroyEntityMessage);
    }

    private void SendMessages<T>(Queue<T> messages) where T : unmanaged
    {
        while (messages.Count > 0)
            _world.Send(messages.Dequeue());
    }

    public void Draw()
    {
        _spriteBatch.BeginTextRendering();
        //_spriteBatch.Begin();

        //Draw the world

        //...all the entities
        _spriteRenderer.Draw();

        _spriteBatch.DrawText(Resources.GameFont, "SERVER", new Vector2(BaseGame.SCREEN_WIDTH * 0.5f, BaseGame.SCREEN_HEIGHT - 48), Color.Black, Alignment.Centre);

        _spriteBatch.End();
    }

    public void SpawnPlayer(ushort clientId, string name, Vector2 position)
    {
        // TODO: Possibly need a producer / consumer here 

        // Queue this new state from a client
        _queuedPlayerSpawnMessages.Enqueue(new PlayerSpawnMessage(
            clientId,
            position,
            Color.Black
        ));
    }

    public void ClientStateReceivedHandler(ClientStateArgs e)
    {
        var entity = _playerEntityMapper.GetEntityFromClientId(e.ClientId);

        if (entity == PlayerEntityMapper.INVALID_ENTITY)
            return;

        // Header
        var lastReceivedMessageId = e.Message.GetUInt();
        var gameId = e.Message.GetByte();
        var lastReceivedServerTick = e.Message.GetUInt();

        // Payload
        var clientPredictionInMilliseconds = e.Message.GetUShort();
        var currentClientTick = e.Message.GetUInt();
        var userCommandsCount = e.Message.GetByte();
        var moveUp = e.Message.GetBool();
        var moveDown = e.Message.GetBool();

        // TODO: Possibly need a producer / consumer here 

        // Queue this new state from a client
        _queuedClientStateMessages.Enqueue(new ClientStateReceivedMessage(
            e.ClientId,
            entity,
            lastReceivedMessageId,
            gameId,
            lastReceivedServerTick,
            clientPredictionInMilliseconds,
            currentClientTick,
            userCommandsCount,
            moveUp,
            moveDown
        ));
    }
}
