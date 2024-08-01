/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using MoonTools.ECS;
using Riptide;
using RiptideFNATankCommon;
using RiptideFNATankCommon.Components;
using RiptideFNATankCommon.Extensions;
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

    /// <summary>
    /// An incrementing number so that messages can be ordered / ack'd.
    /// </summary>
    public static uint ServerTick; //TODO: hide or move

    readonly Dictionary<ushort, uint> _clientAcks;

    readonly Filter _filter;

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
        foreach (var entity in _filter.Entities)
        {
            //Simulate out of order packets
            var serverTick = RandomHelper.FastRandom.PercentageChance(_networkGameManager.OutOfOrderPacketPercentage) 
                ? (uint)RandomHelper.FastRandom.Next(0, (int)ServerTick)
                : ServerTick;

            var clientId = _playerEntityMapper.GetClientIdFromEntity(entity);
            _clientAcks.TryGetValue(clientId, out var clientTick);

            ref readonly var position = ref Get<PositionComponent>(entity);

            var message = Message.Create(MessageSendMode.Unreliable, ServerMessageType.SendWorldState);

            // Header
            message.AddUShort(clientId);
            byte gameId = 0;
            message.AddByte(gameId);

            // Snapshot
            message.AddUInt(serverTick);
            message.AddUInt(clientTick);
            message.AddVector2(position.Value);

            // Send a network packet containing the player's state.
            _networkGameManager.SendMessageToAll(message);

            Logger.Info($"Sending state to client for serverTick: {serverTick}, clientTick: {clientTick}, position: {position.Value}");
        }

        ServerTick++;
    }
}
