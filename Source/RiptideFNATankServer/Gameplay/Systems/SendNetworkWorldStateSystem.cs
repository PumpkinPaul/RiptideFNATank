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
using Riptide;
using RiptideFNATankCommon.Extensions;
using RiptideFNATankCommon.Gameplay.Components;
using RiptideFNATankCommon.Networking;
using RiptideFNATankServer.Networking;
using Wombat.Engine;
using Wombat.Engine.Logging;

namespace RiptideFNATankServer.Gameplay.Systems;

static partial class Log
{
    [LoggerMessage(Message = "Send state to client for command frame: {commandFrame}, {state}")]
    public static partial void SendState(this ILogger logger, LogLevel logLevel, uint commandFrame, Vector2 state);
}

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
        foreach (var client in _worldState)
        {
            //Simulate out of order packets
            var serverCommandFrame = RandomHelper.FastRandom.PercentageChance(_networkGameManager.OutOfOrderPacketPercentage)
                ? (uint)RandomHelper.FastRandom.Next(0, (int)simulationState.CurrentServerCommandFrame)
                : simulationState.CurrentServerCommandFrame;

            var clientId = client.Key;
            _clientAcks.TryGetValue(clientId, out var clientCommandFrame);

            var message = Message.Create(MessageSendMode.Unreliable, ServerMessageType.WorldState);

            // Header
            message.AddUShort(clientId);
            byte gameId = 0;
            message.AddByte(gameId);

            // Snapshot
            message.AddUInt(serverCommandFrame);
            message.AddUInt(clientCommandFrame);

            // Number of players
            message.AddByte((byte)_worldState.Count);

            // All players
            // - the 'local' player (when the client gets the message)
            // - al the other remote players
            Vector2 p = Vector2.Zero;
            foreach (var player in _worldState)
            {
                var position = _worldState[player.Key];
                message.AddUShort(player.Key);
                message.AddVector2(position);

                p = position;
            }

            // Send a network packet containing all the world state to a single player
            _networkGameManager.SendMessage(message, clientId);

            Logger.Log.SendState(LogLevel.Debug, serverCommandFrame, p);
        }
    }
}
