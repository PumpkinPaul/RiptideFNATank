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

public readonly record struct PlayerCommandsReceivedMessage(
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
public sealed class ClientPlayerCommandsReceivedSystem : MoonTools.ECS.System
{
    readonly Dictionary<ushort, uint> _clientAcks;
    readonly Dictionary<ushort, CommandsBuffer> _clientPlayerCommands;

    public ClientPlayerCommandsReceivedSystem(
        World world,
        Dictionary<ushort, uint> clientAcks,
        Dictionary<ushort, CommandsBuffer> clientPlayerCommands
    ) : base(world)
    {
        _clientAcks = clientAcks;
        _clientPlayerCommands = clientPlayerCommands;
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var message in ReadMessages<PlayerCommandsReceivedMessage>())
        {
            Logger.Info($"{nameof(ClientPlayerCommandsReceivedSystem)}: Got inputs from client for command frame: {message.EffectiveClientCommandFrame}, message.MoveUp: {message.MoveUp}, message.MoveDown: {message.MoveDown}");

            CacheLatestCommandFrame(message);
            CacheClientPlayerCommands(message);
        }
    }

    /// <summary>
    /// Saves the 'most recent' command frame sent from the server for a player.
    /// <para>
    /// The data structure will help us determine if out of date / duplicate packets have been received.
    /// </para>
    /// </summary>
    /// <param name="message"></param>
    private void CacheLatestCommandFrame(PlayerCommandsReceivedMessage message)
    {
        if (_clientAcks.ContainsKey(message.ClientId) == false)
            _clientAcks[message.ClientId] = new();

        _clientAcks[message.ClientId] = message.CurrentClientCommandFrame;
    }

    /// <summary>
    /// Saves the player commands from clients. 
    /// <para>
    /// These player commands will be fed into the simulation to move all the players.
    /// </para>
    /// </summary>
    private void CacheClientPlayerCommands(PlayerCommandsReceivedMessage message)
    {
        // Save the player commands from the client in the buffer.
        if (_clientPlayerCommands.ContainsKey(message.ClientId) == false)
            _clientPlayerCommands[message.ClientId] = new();

        var playerCommands = new PlayerCommandsComponent(
            MoveUp: message.MoveUp,
            MoveDown: message.MoveDown);

        _clientPlayerCommands[message.ClientId].Add(message.EffectiveClientCommandFrame, playerCommands);
    }
}
