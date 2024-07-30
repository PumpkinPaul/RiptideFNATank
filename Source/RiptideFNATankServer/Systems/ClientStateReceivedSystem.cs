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
using RiptideFNATankCommon.Components;

namespace RiptideFNATankServer.Gameplay.Systems;

public readonly record struct ClientStateReceivedMessage(
    ushort ClientId,
    Entity Entity,
    uint LastReceivedMessageId,
    byte GameId,
    uint LastReceivedSnapshotId,
    ushort ClientPredictionInMilliseconds,
    uint GameFrameNumber,
    byte UserCommandsCount,
    bool MoveUp,
    bool MoveDown
);

/// <summary>
/// 
/// </summary>
public sealed class ClientStateReceivedSystem : MoonTools.ECS.System
{
    Dictionary<ushort, uint> _clientAcks = new();

    public ClientStateReceivedSystem(World world) : base(world)
    {
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var message in ReadMessages<ClientStateReceivedMessage>())
        {
            if (_clientAcks.ContainsKey(message.ClientId) == false)
                _clientAcks[message.ClientId] = new();

            _clientAcks[message.ClientId] = message.LastReceivedSnapshotId;

            ref var playerActions = ref Get<PlayerActionsComponent>(message.Entity);
            Set(message.Entity, new PlayerActionsComponent(message.MoveUp, message.MoveDown));            
        }
    }
}
