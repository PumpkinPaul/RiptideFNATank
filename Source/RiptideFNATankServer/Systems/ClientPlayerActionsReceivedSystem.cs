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
    readonly Dictionary<ushort, PriorityQueue<ClientPlayerActions, uint>> _clientPlayerActions;

    public ClientPlayerActionsReceivedSystem(
        World world,
        Dictionary<ushort, uint> clientAcks,
        Dictionary<ushort, PriorityQueue<ClientPlayerActions, uint>> clientPlayerActions
    ) : base(world)
    {
        _clientAcks = clientAcks;
        _clientPlayerActions = clientPlayerActions;
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var message in ReadMessages<ClientPlayerActionsReceivedMessage>())
        {
            Logger.Info($"{nameof(ClientPlayerActionsReceivedSystem)}: Got inputs from client for command frame: {message.EffectiveClientCommandFrame}, message.MoveUp: {message.MoveUp}, message.MoveDown: {message.MoveDown}");

            if (_clientAcks.ContainsKey(message.ClientId) == false)
                _clientAcks[message.ClientId] = new();

            CacheLatestCommandFrame(message);
            CacheClientPlayerActions(message);
        }
    }

    /// <summary>
    /// Saves the 'most recent' command frame sent from the server for a player.
    /// <para>
    /// The data structure will help us determine if out of date / duplicate packets have been received.
    /// </para>
    /// </summary>
    /// <param name="message"></param>
    private void CacheLatestCommandFrame(ClientPlayerActionsReceivedMessage message)
    {
        _clientAcks[message.ClientId] = message.CurrentClientCommandFrame;
    }

    /// <summary>
    /// Saves the player actions from clients. 
    /// <para>
    /// These player actions will be fed into the simulation to move all the players.
    /// </para>
    /// </summary>
    private void CacheClientPlayerActions(ClientPlayerActionsReceivedMessage message)
    {
        // Save the player actions from the client in the buffer.
        if (_clientPlayerActions.ContainsKey(message.ClientId) == false)
            _clientPlayerActions[message.ClientId] = new();

        // Ensure the effective command frame isn't already added - UDP could send us duplicate packets.
        // TODO: probably better not to even create the message - have another data type that tracks that info?
        foreach (var (_, Priority) in _clientPlayerActions[message.ClientId].UnorderedItems)
        {
            if (Priority == message.EffectiveClientCommandFrame)
            {
                Logger.Debug($"Server already recevied client input for command frame: {message.EffectiveClientCommandFrame}");
                return;
            }
        }

        _clientPlayerActions[message.ClientId].Enqueue(
            new ClientPlayerActions
            {
                ClientCommandFrame = message.EffectiveClientCommandFrame,
                MoveUp = message.MoveUp,
                MoveDown = message.MoveDown
            },
            message.EffectiveClientCommandFrame
        );
    }
}
