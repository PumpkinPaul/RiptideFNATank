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
using RiptideFNATankServer.Gameplay;

namespace RiptideFNATankServer.Systems;

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
    readonly Dictionary<ushort, CommandsBuffer> _clientPlayerActions;

    public ClientPlayerActionsReceivedSystem(
        World world,
        Dictionary<ushort, uint> clientAcks,
        Dictionary<ushort, CommandsBuffer> clientPlayerActions
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
        if (_clientAcks.ContainsKey(message.ClientId) == false)
            _clientAcks[message.ClientId] = new();

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

        var playerActions = new PlayerActionsComponent(
            MoveUp: message.MoveUp,
            MoveDown: message.MoveDown);

        _clientPlayerActions[message.ClientId].Add(message.EffectiveClientCommandFrame, playerActions);
    }
}
