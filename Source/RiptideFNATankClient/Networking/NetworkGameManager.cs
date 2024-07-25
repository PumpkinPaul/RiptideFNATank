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
using RiptideFNATankClient.Gameplay.Players;
using RiptideFNATankCommon;
using RiptideFNATankCommon.Extensions;
using RiptideFNATankCommon.Networking;
using System;
using System.Collections.Generic;

namespace RiptideFNATankClient.Networking;

public record SpawnedPlayerEventArgs(
    ushort ClientId,
    Vector2 Position
);

public record ReceivedRemotePaddleStateEventArgs(
    float TotalSeconds,
    Vector2 Position,
    Vector2 Velocity,
    bool MoveUp,
    bool MoveDown,
    string SessionId
);

public record ReceivedRemoteBallStateEventArgs(
    float Direction,
    Vector2 Position
);

public record ReceivedRemoteScoreEventArgs(
    int Player1Score,
    int Player2Score
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

    public event Action LocalClientConnected;
    public event Action ConnectionFailed;
    public event Action Disconnected;

    public event Action<SpawnedPlayerEventArgs> SpawnedLocalPlayer;
    public event Action<SpawnedPlayerEventArgs> SpawnedRemotePlayer;
    public event EventHandler<ReceivedRemotePaddleStateEventArgs> ReceivedRemotePaddleState;
    public event EventHandler<ReceivedRemoteBallStateEventArgs> ReceivedRemoteBallState;
    public event EventHandler<ReceivedRemoteScoreEventArgs> ReceivedRemoteScore;
    public event EventHandler<RemovedPlayerEventArgs> RemovedPlayer;

    readonly Dictionary<ushort, Player> _players = [];
    Player _localPlayer;

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
    static void ReceivedSpawnPlayer(Message message)
    {
        var clientId = message.GetUShort();
        var name = message.GetString();
        var position = message.GetVector2();

#if DEBUG
        Logger.Info($"Message handler: {nameof(ReceivedSpawnPlayer)} from client: {clientId}");
        Logger.Debug("Read the following...");
        Logger.Debug($"{name}");
#endif
        Instance.SpawnPlayer(clientId, name, position);
    }

    #endregion

    #region Send client messages 

    public void SendMessage(Message message)
    {
        Client.Send(message);
    }

    void SendPlayerName()
    {
        var message = Message.Create(MessageSendMode.Reliable, ClientMessageType.Name);
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

    void SpawnPlayer(ushort clientId, string name, Vector2 position)
    {
#if DEBUG
        Logger.Info($"{nameof(SpawnPlayer)}");
        Logger.Debug($"{clientId}");
        Logger.Debug($"{name}");
        Logger.Debug($"{position}");
#endif

        // If the player has already been spawned, return early.
        if (_players.ContainsKey(clientId))
        {
            return;
        }

        // Set a variable to check if the player is the local player or not.
        var isLocal = Client.Id == clientId;

        Player player;

        // Setup the appropriate network data values if this is a remote player.
        if (isLocal)
        {
            player = new LocalPlayer();
            _localPlayer = player;

            SpawnedLocalPlayer?.Invoke(new SpawnedPlayerEventArgs(clientId, position));
        }
        else
        {
            player = new NetworkPlayer
            {
                NetworkData = new RemotePlayerNetworkData
                {

                }
            };

            SpawnedRemotePlayer?.Invoke(new SpawnedPlayerEventArgs(clientId, position));
        }

        // Add the player to the players array.
        _players[clientId] = player;
    }

    void RemovePlayer(ushort clientId)
    {
        if (!_players.ContainsKey(clientId))
            return;

        _players.Remove(clientId);

        RemovedPlayer?.Invoke(this, new RemovedPlayerEventArgs(clientId));
    }

    /// <summary>
    /// Updates the player's velocity and position based on incoming state data.
    /// </summary>
    /// <param name="state">The incoming state byte array.</param>
    private void UpdateRemotePaddleStateFromPacket(byte[] state, NetworkPlayer networkPlayer)
    {
        //TODO: fix the allocation here
        //_packetReader.SetState(state);

        //var totalSeconds = _packetReader.ReadSingle();

        //var position = _packetReader.ReadVector2();
        //var velocity = _packetReader.ReadVector2();

        //var moveUp = _packetReader.ReadBoolean();
        //var moveDown = _packetReader.ReadBoolean();

        //ReceivedRemotePaddleState?.Invoke(
        //    this,
        //    new ReceivedRemotePaddleStateEventArgs(
        //        totalSeconds,
        //        position,
        //        velocity,
        //        moveUp,
        //        moveDown,
        //        networkPlayer.NetworkData.User.SessionId));
    }

    /// <summary>
    /// Updates the ball's direction and position based on incoming state data.
    /// </summary>
    /// <param name="state">The incoming state byte array.</param>
    private void UpdateDirectionAndPositionFromState(byte[] state)
    {
        //_packetReader.SetState(state);

        //var direction = _packetReader.ReadSingle();
        //var position = _packetReader.ReadVector2();

        //ReceivedRemoteBallState?.Invoke(
        //    this,
        //    new ReceivedRemoteBallStateEventArgs(direction, position));
    }

    /// <summary>
    /// Updates the score based on incoming state data.
    /// </summary>
    /// <param name="state">The incoming state byte array.</param>
    private void UpdateScoreFromState(byte[] state)
    {
        //_packetReader.SetState(state);

        //var player1Score = _packetReader.ReadInt32();
        //var player2Score = _packetReader.ReadInt32();

        //ReceivedRemoteScore?.Invoke(
        //    this,
        //    new ReceivedRemoteScoreEventArgs(player1Score, player2Score));
    }

    /// <summary>
    /// Sends a match state message across the network.
    /// </summary>
    /// <param name="opCode">The operation code.</param>
    /// <param name="state">The stringified JSON state data.</param>
    public void SendMatchState(long opCode, string state)
    {
        //_RiptideConnection.Socket.SendMatchStateAsync(_currentMatch.Id, opCode, state);
    }

    /// <summary>
    /// Sends a match state message across the network.
    /// </summary>
    /// <param name="opCode">The operation code.</param>
    /// <param name="state">The stringified JSON state data.</param>
    public void SendMatchState(long opCode, byte[] state)
    {
        //_RiptideConnection.Socket.SendMatchStateAsync(_currentMatch.Id, opCode, state);
    }
}
