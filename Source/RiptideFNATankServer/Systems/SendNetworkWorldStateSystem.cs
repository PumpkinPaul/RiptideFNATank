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
    public static uint _serverSequenceId; //TODO: hide or move

    readonly Filter _filter;

    public SendNetworkWorldStateSystem(
        World world,
        ServerNetworkManager networkGameManager,
        PlayerEntityMapper playerEntityMapper
    ) : base(world)
    {
        _networkGameManager = networkGameManager;
        _playerEntityMapper = playerEntityMapper;

        _filter = FilterBuilder
            .Include<PositionComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in _filter.Entities)
        {
            //Simulate out of order packets
            var serverSequenceId = RandomHelper.FastRandom.PercentageChance(_networkGameManager.OutOfOrderPacketPercentage) 
                ? (uint)RandomHelper.FastRandom.Next(0, (int)_serverSequenceId)
                : _serverSequenceId;

            var clientId = _playerEntityMapper.GetClientIdFromEntity(entity);

            ref readonly var position = ref Get<PositionComponent>(entity);

            var message = Message.Create(MessageSendMode.Unreliable, ServerMessageType.SendWorldState);

            // Header
            message.AddUShort(clientId);
            byte gameId = 0;
            message.AddByte(gameId);

            // Snapshot
            message.AddUInt(serverSequenceId);
            message.AddVector2(position.Value);

            // Send a network packet containing the player's state.
            _networkGameManager.SendMessageToAll(message);
        }

        _serverSequenceId++;
    }
}
