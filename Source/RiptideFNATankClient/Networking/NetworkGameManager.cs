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
using Riptide;
using Riptide.Utils;
using RiptideFNATankClient.Gameplay;
using RiptideFNATankCommon;
using RiptideFNATankCommon.Extensions;
using RiptideFNATankCommon.Networking;
using System;
using System.Collections.Generic;
using Wombat.Engine.Logging;

namespace RiptideFNATankClient.Networking;

public record ReceivedSpawnPlayerEventArgs(
    ushort ClientId,
    uint InitialServerCommandFrame,
    Vector2 Position
);

public record ReceivedWorldStateEventArgs(
    ushort ClientId,
    uint ServerCommandFrame,
    uint ClientCommandFrame,
    Vector2 Position
);

public record RemovedPlayerEventArgs(
    ushort ClientId
);

static partial class Log
{
    [LoggerMessage(Message = "Client has quit match.")]
    public static partial void QuitMatch(this ILogger logger, LogLevel logLevel);
}

/// <summary>
/// Responsible for managing a networked game.
/// </summary>
public class NetworkGameManager
{
    static NetworkGameManager Instance;

    /// <summary>
    /// A value between 0 (no dropped packets) and 100 (all dropped packets)
    /// </summary>
    public int DroppedPacketPercentage { get; set; } = 0;

    /// <summary>
    /// TODO: Simulates lag
    /// </summary>
    public float LagDelayInSeconds { get; set; } = 10;

    public event Action LocalClientConnected;
    public event Action ConnectionFailed;
    public event Action Disconnected;

    public event Action<ReceivedSpawnPlayerEventArgs> SpawnedLocalPlayer;
    public event Action<ReceivedSpawnPlayerEventArgs> SpawnedRemotePlayer;
    public event Action<ReceivedWorldStateEventArgs> ReceivedWorldState;
    public event EventHandler<RemovedPlayerEventArgs> RemovedPlayer;

    readonly Dictionary<ushort, Player> _players = [];

    public Client Client { get; private set; }

    readonly string _ip;
    readonly ushort _port;

    public NetworkGameManager(
        string ip,
        ushort port)
    {
        Instance = this;

        _port = port;
        _ip = ip;

        RiptideLogger.Initialize(Logger.Debug, Logger.Info, Logger.Warning, Logger.Error, includeTimestamps: false);
    }

    public void Start()
    {
        Client = new Client();
        Client.Connected += LocalClientConnectedHandler;
        Client.Disconnected += LocalClientDisconnectedHandler;
        Client.ConnectionFailed += LocalClientConnectionFailedHandler;
    }

    void LocalClientConnectedHandler(object sender, EventArgs e)
    {
        // TODO: remove this!
        Client.TimeoutTime = 50000;

        SendPlayerJoined();
        LocalClientConnected?.Invoke();
    }

    void LocalClientConnectionFailedHandler(object sender, EventArgs e)
    {
        ConnectionFailed?.Invoke();
    }

    void LocalClientDisconnectedHandler(object sender, EventArgs e)
    {
        Disconnected?.Invoke();
    }

    public void Update()
    {
        Client.Update();
    }

    public void Connect()
    {
        Client.Connect($"{_ip}:{_port}");
    }

    public void Disconnect()
    {
        Client.Disconnect();
    }

    #region Handle server messages 

    [MessageHandler((ushort)ServerMessageType.SpawnPlayer)]
    static void ReceivedSpawnPlayerFromServer(Message message)
    {
        Instance.SpawnPlayer(message);
    }

    [MessageHandler((ushort)ServerMessageType.WorldState)]
    static void ReceivedWorldStateFromServer(Message message)
    {
        Instance.ReceivedNewWorldState(message);
    }

    #endregion

    #region Send messages from client to server

    public void SendMessage(Message message)
    {
        Client.Send(message);
    }

    void SendPlayerJoined()
    {
        var message = Message.Create(MessageSendMode.Reliable, ClientMessageType.JoinGame);
        message.AddString(ClientGame.Name);
        Client.Send(message);
    }

    #endregion

    /// <summary>
    /// Quits the current match.
    /// </summary>
    public void QuitMatch()
    {
        Logger.Log.QuitMatch(LogLevel.Information);

        // Destroy all existing player.
        foreach (var player in _players.Values)
            player.Destroy();

        // Clear the players array.
        _players.Clear();
    }

    void SpawnPlayer(Message message)
    {
        var playerCount = message.GetByte();

        for (var i = 0; i < playerCount; i++)
        {
            var clientId = message.GetUShort();
            var name = message.GetString();
            var initialServerCommandFrame = message.GetUInt();
            var position = message.GetVector2();

            // If the player has already been spawned, return early.
            if (_players.ContainsKey(clientId))
                continue;

            // Set a variable to check if the player is the local player or not.
            var isLocal = Client.Id == clientId;

            var player = new Player
            {
                Name = name,
                IsLocal = Client.Id == clientId
            };

            // Add the player to the players array.
            _players[clientId] = player;

            // Setup the appropriate network data values if this is a remote player.
            if (player.IsLocal)
                SpawnedLocalPlayer?.Invoke(new ReceivedSpawnPlayerEventArgs(clientId, initialServerCommandFrame, position));
            else
                SpawnedRemotePlayer?.Invoke(new ReceivedSpawnPlayerEventArgs(clientId, 0, position));
        }
    }

    void ReceivedNewWorldState(Message message)
    {
        // Header
        var clientId = message.GetUShort();
        var gameId = message.GetByte();

        // If the player has been removed (quit / disconnected) then NAR.
        if (_players.ContainsKey(clientId) == false)
            return;

        // Snapshot
        var serverCommandFrame = message.GetUInt();
        var clientCommandFrame = message.GetUInt();

        // Number of players
        var playerCount = message.GetByte();

        // All players
        // - the 'local' player (this client)
        // - all the other remote players
        for (var i = 0; i < playerCount; i++)
        {
            var playerId = message.GetUShort();
            var position = message.GetVector2();

            // TODO: handle remote player disconnections here.
            ReceivedWorldState?.Invoke(new ReceivedWorldStateEventArgs(playerId, serverCommandFrame, clientCommandFrame, position));
        }
    }

    void RemovePlayer(ushort clientId)
    {
        if (!_players.ContainsKey(clientId))
            return;

        _players.Remove(clientId);

        RemovedPlayer?.Invoke(this, new RemovedPlayerEventArgs(clientId));
    }
}
 