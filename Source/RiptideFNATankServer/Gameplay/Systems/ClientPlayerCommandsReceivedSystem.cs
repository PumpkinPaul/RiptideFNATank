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
using MoonTools.ECS;
using RiptideFNATankCommon;
using RiptideFNATankCommon.Gameplay.Components;
using RiptideFNATankServer.Gameplay;
using Wombat.Engine.Logging;

namespace RiptideFNATankServer.Gameplay.Systems;

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

static partial class Log
{
    [LoggerMessage(Message = "Received valid commands from client for sequence: {sequence}, moveUp: {moveUp}, moveDown: {moveDown}")]
    public static partial void PlayerCommandsReceived(this ILogger logger, LogLevel logLevel, uint sequence, bool moveUp, bool moveDown);
}

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
            Logger.Log.PlayerCommandsReceived(logLevel: LogLevel.Information, sequence: message.EffectiveClientCommandFrame, message.MoveUp, message.MoveDown);

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
