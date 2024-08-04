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
using RiptideFNATankCommon;
using RiptideFNATankCommon.Components;

namespace RiptideFNATankServer.Gameplay.Systems;

public readonly record struct ClientPlayerActionsReceivedMessage(
    ushort ClientId,
    Entity Entity,
    byte GameId,
    uint LastReceivedServerCommandFrame,
    uint CurrentClientCommandFrame,
    byte UserCommandsCount,
    uint EffectiveClientCommandFrame,
    bool MoveUp,
    bool MoveDown
);

/// <summary>
/// 
/// </summary>
public sealed class ClientPlayerActionsReceivedSystem : MoonTools.ECS.System
{
    readonly Dictionary<ushort, uint> _clientAcks;

    public ClientPlayerActionsReceivedSystem(
        World world,
        Dictionary<ushort, uint> clientAcks
    ) : base(world)
    {
        _clientAcks = clientAcks;
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var message in ReadMessages<ClientPlayerActionsReceivedMessage>())
        {
            if (_clientAcks.ContainsKey(message.ClientId) == false)
                _clientAcks[message.ClientId] = new();

            var lastReceivedClientCommandFrame = _clientAcks[message.ClientId];

            // Ensure the new state > the last state received
            if (message.CurrentClientCommandFrame < lastReceivedClientCommandFrame)
            {
                // Discard packet
                Logger.Warning($"Received an old packet from client for CurrentClientCommandFrame: {message.CurrentClientCommandFrame}. Client has already received state for CommandFrame: {lastReceivedClientCommandFrame}.");
            }
            else if (message.CurrentClientCommandFrame == lastReceivedClientCommandFrame)
            {
                // Duplicate packet?
                Logger.Warning($"Received a duplicate packet from client for CurrentClientCommandFrame: {message.CurrentClientCommandFrame}.");
            }
            else //else if (newState.sequence > lastState.sequence)
            {
                _clientAcks[message.ClientId] = message.CurrentClientCommandFrame;

                ref var playerActions = ref Get<PlayerActionsComponent>(message.Entity);
                Set(message.Entity, new PlayerActionsComponent(message.MoveUp, message.MoveDown));

                Logger.Info($"Got inputs from client for CurrentClientCommandFrame: {message.CurrentClientCommandFrame}, message.MoveUp: {message.MoveUp}, message.MoveDown: {message.MoveDown}");
            }
        }
    }
}
