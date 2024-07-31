/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Microsoft.Xna.Framework;
using Riptide;
using Riptide.Utils;
using RiptideFNATankCommon;
using RiptideFNATankCommon.Extensions;
using RiptideFNATankCommon.Networking;
using System;
using System.Collections.Generic;

namespace RiptideFNATankClient.Networking;

public record ReceivedSpawnPlayerEventArgs(
    ushort ClientId,
    uint InitialServerTick,
    Vector2 Position
);

public record ReceivedWorldStateEventArgs(
    ushort ClientId,
    uint ServerTick,
    Vector2 Position
);

public record RemovedPlayerEventArgs(
    ushort ClientId
);

/// <summary>
/// Responsible for managing a networked game.
/// </summary>
public class NetworkGameManager
{
    static NetworkGameManager Instance;

    /// <summary>
    /// A value between 0 (no dropped packets) and 100 (all dropped packets)
    /// </summary>
    public int DroppedPacketPercentage { get; set; } = 10;

    /// <summary>
    /// TODO: Simulates lag
    /// </summary>
    public float LagDelayInSeconds { get; set; } = 0;

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
        Client.TimeoutTime = 50000;

        SendPlayerName();
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

    [MessageHandler((ushort)ServerMessageType.SendWorldState)]
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

    void SendPlayerName()
    {
        var message = Message.Create(MessageSendMode.Reliable, ClientMessageType.JoinGame);
        message.AddString($"{Guid.NewGuid()}");
        Client.Send(message);
    }

    #endregion

    /// <summary>
    /// Quits the current match.
    /// </summary>
    public void QuitMatch()
    {
        Logger.Info($"QuitMatch");

        // Ask Riptide to leave the match.
        //await _RiptideConnection.Socket.LeaveMatchAsync(_currentMatch);

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
            var initialServerTick = message.GetUInt();
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

            // Setup the appropriate network data values if this is a remote player.
            if (player.IsLocal)
                SpawnedLocalPlayer?.Invoke(new ReceivedSpawnPlayerEventArgs(clientId, initialServerTick, position));
            else
                SpawnedRemotePlayer?.Invoke(new ReceivedSpawnPlayerEventArgs(clientId, 0, position));

            // Add the player to the players array.
            _players[clientId] = player;
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
        var serverSequeceId = message.GetUInt();
        var position = message.GetVector2();

        ReceivedWorldState?.Invoke(new ReceivedWorldStateEventArgs(clientId, serverSequeceId, position));
    }

    void RemovePlayer(ushort clientId)
    {
        if (!_players.ContainsKey(clientId))
            return;

        _players.Remove(clientId);

        RemovedPlayer?.Invoke(this, new RemovedPlayerEventArgs(clientId));
    }
}
 