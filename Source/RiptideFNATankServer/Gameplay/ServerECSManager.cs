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
using RiptideFNATankCommon;
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
    uint CurrentServerCommandFrame
);

/// <summary>
/// Encapsulates management of the ECS
/// </summary>
public class ServerECSManager
{
    readonly World _world;

    // Systems
    readonly MoonTools.ECS.System[] _systems;

    // Renderers
    readonly SpriteRenderer _spriteRenderer;
    readonly SpriteBatch _spriteBatch;

    // Mapping between networking and ECS
    readonly PlayerEntityMapper _playerEntityMapper = new();
    readonly ServerNetworkManager _networkGameManager;

    readonly Queue<PlayerSpawnMessage> _queuedPlayerSpawnMessages = new();
    readonly Queue<ClientPlayerActionsReceivedMessage> _queuedClientStateMessages = new();
    readonly Queue<DestroyEntityMessage> _destroyEntityMessage = new();

    // T 
    readonly Dictionary<ushort, uint> _clientAcks = [];
    readonly Dictionary<ushort, PriorityQueue<ClientPlayerActions, uint>> _clientPlayerActions = [];

    public ServerECSManager(
        ServerNetworkManager networkGameManager,
        SpriteBatch spriteBatch
    )
    {
        _networkGameManager = networkGameManager;
        _spriteBatch = spriteBatch;

        _world = new World();

        // Add a singleton for common simulation state.
        _world.Set(_world.CreateEntity(), new SimulationStateComponent());

        _systems = [
            // Spawn the entities into the game world
            new PlayerSpawnSystem(_world, _playerEntityMapper),

            // PlayerActions have been received from client...
            new ClientPlayerActionsReceivedSystem(_world, _clientAcks, _clientPlayerActions),

            // ...process buffered commands and set the action for the correct command frame.
            new ProcessBufferClientPlayerActionsSystem(_world, _playerEntityMapper, _clientPlayerActions),

            // ====================================================================================================
            // World Simulation Start
            // The following systems should be the same between client and server to get a consistent game

            // Process the actions (e.g. do a jump, fire a gun, move forward, etc).
            new PlayerActionsSystem(_world),

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

        // Queue entity to begin lerping to the corrected position.
        _destroyEntityMessage.Enqueue(new DestroyEntityMessage(
            Entity: entity
        ));
    }

    public void Update(GameTime gameTime)
    {
        // How do we feel about this being outside of a system?
        ref var simulationState = ref _world.GetSingleton<SimulationStateComponent>();

        Logger.Success($"Command Frame: {simulationState.CurrentServerCommandFrame}");

        SendAllQueuedECSMessages();

        foreach (var system in _systems)
            system.Update(gameTime.ElapsedGameTime);

        _world.FinishUpdate();

        simulationState.CurrentServerCommandFrame++;
        ServerGame.ServerCommandFrame = simulationState.CurrentServerCommandFrame;
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
        // _spriteBatch.Begin();

        // Draw the world

        // ...all the entities
        _spriteRenderer.Draw();

        _spriteBatch.DrawText(Resources.GameFont, "SERVER", new Vector2(BaseGame.SCREEN_WIDTH * 0.5f, BaseGame.SCREEN_HEIGHT - 48), Color.Black, Alignment.Centre);

        _spriteBatch.End();
    }

    public void SpawnPlayer(ushort clientId, string name, Vector2 position)
    {
        // Queue this new state from a client
        _queuedPlayerSpawnMessages.Enqueue(new PlayerSpawnMessage(
            clientId,
            position,
            Color.Black
        ));
    }

    public void ClientPlayerActionsReceivedHandler(ClientPlayerActionsArgs e)
    {
        var entity = _playerEntityMapper.GetEntityFromClientId(e.ClientId);

        if (entity == PlayerEntityMapper.INVALID_ENTITY)
            return;

        // Header
        var gameId = e.Message.GetByte();
        var lastReceivedServerCommandFrame = e.Message.GetUInt();

        // Payload
        var currentClientCommandFrame = e.Message.GetUInt();

        var commandCount = e.Message.GetByte();
        for (var i = 0; i < commandCount; i++)
        {
            var effectiveClientCommandFrame = e.Message.GetUInt();
            var moveUp = e.Message.GetBool();
            var moveDown = e.Message.GetBool();

            // Queue this new state from a client
            _queuedClientStateMessages.Enqueue(new ClientPlayerActionsReceivedMessage(
                e.ClientId,
                entity,
                gameId,
                lastReceivedServerCommandFrame,
                currentClientCommandFrame,
                1, // TODO: could probably get rid of this
                effectiveClientCommandFrame,
                moveUp,
                moveDown
            ));
        }
    }
}
