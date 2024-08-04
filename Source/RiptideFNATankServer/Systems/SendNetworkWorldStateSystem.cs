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
using MoonTools.ECS;
using Riptide;
using RiptideFNATankCommon;
using RiptideFNATankCommon.Components;
using RiptideFNATankCommon.Extensions;
using RiptideFNATankCommon.Gameplay;
using RiptideFNATankCommon.Networking;
using RiptideFNATankServer.Networking;
using Wombat.Engine;

namespace RiptideFNATankServer.Gameplay.Systems;

/// <summary>
/// Sends the world state to clients.
/// </summary>
public sealed class SendNetworkWorldStateSystem : MoonTools.ECS.System
{
    readonly ServerNetworkManager _networkGameManager;
    readonly PlayerEntityMapper _playerEntityMapper;
    readonly Dictionary<ushort, uint> _clientAcks;

    readonly Filter _filter;

    readonly Dictionary<ushort, Vector2> _worldState = [];

    public SendNetworkWorldStateSystem(
        World world,
        ServerNetworkManager networkGameManager,
        PlayerEntityMapper playerEntityMapper,
        Dictionary<ushort, uint> clientAcks
    ) : base(world)
    {
        _networkGameManager = networkGameManager;
        _playerEntityMapper = playerEntityMapper;
        _clientAcks = clientAcks;

        _filter = FilterBuilder
            .Include<PositionComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        ref readonly var simulationState = ref GetSingleton<SimulationStateComponent>();

        // Prepare the world state!
        foreach (var entity in _filter.Entities)
        {
            var clientId = _playerEntityMapper.GetClientIdFromEntity(entity);
            
            ref readonly var position = ref Get<PositionComponent>(entity);
            _worldState[clientId] = position.Value;
        }

        // Send the world State
        foreach(var client in _worldState)
        {
            //Simulate out of order packets
            var serverTick = RandomHelper.FastRandom.PercentageChance(_networkGameManager.OutOfOrderPacketPercentage)
                ? (uint)RandomHelper.FastRandom.Next(0, (int)simulationState.CurrentServerTick)
                : simulationState.CurrentServerTick;

            var clientId = client.Key;
            _clientAcks.TryGetValue(clientId, out var clientTick);

            var message = Message.Create(MessageSendMode.Unreliable, ServerMessageType.WorldState);

            // Header
            message.AddUShort(clientId);
            byte gameId = 0;
            message.AddByte(gameId);

            // Snapshot
            message.AddUInt(serverTick);
            message.AddUInt(clientTick);

            // Number of players
            message.AddByte((byte)_worldState.Count);

            // All players
            // - the 'local' player (when the client gets the message)
            // - al the other remote players
            foreach (var player in _worldState)
            {
                var position = _worldState[player.Key];
                message.AddUShort(player.Key);
                message.AddVector2(position);
            }

            // Send a network packet containing all the world state to a single player
            _networkGameManager.SendMessage(message, clientId);
        }
    }
}
