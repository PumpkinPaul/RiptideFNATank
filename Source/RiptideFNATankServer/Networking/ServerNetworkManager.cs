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

namespace RiptideFNATankServer.Networking;

/// <summary>
/// Responsible for managing a networked game on the server.
/// </summary>
public class ServerNetworkManager
{
    static ServerNetworkManager Instance;

    /// <summary>
    /// A value between 0 (no out of order packets) and 100 (all packets out of order)
    /// </summary>
    public int OutOfOrderPacketPercentage { get; set; } = 10;

    public readonly record struct ClientConnectedArgs(
        ushort ClientId,
        Message Message);

    public event Action<ClientConnectedArgs> ClientConnected;

    public readonly record struct ClientStateArgs(
        ushort ClientId,
        Message Message);

    public event Action<ClientStateArgs> ReceivedClientState;

    public Server Server { get; private set; }

    readonly ushort _port;
    readonly ushort _maxClientCount;

    readonly Dictionary<ushort, Player> _players = [];

    public ServerNetworkManager(
        ushort port,
        ushort maxClientCount)
    {
        Instance = this;

        _port = port;
        _maxClientCount = maxClientCount;

        RiptideLogger.Initialize(Logger.Debug, Logger.Info, Logger.Warning, Logger.Error, includeTimestamps: false);
    }

    public void StartServer()
    {
        Server = new Server();
        Server.Start(_port, _maxClientCount);
        Server.TimeoutTime = 50000;
        Server.ClientDisconnected += ServerClientDisconnected;
    }

    private void ServerClientDisconnected(object sender, ServerDisconnectedEventArgs e)
    {
        Logger.Debug($"{nameof(ServerClientDisconnected)} from client: {e.Client.Id}");
        _players.Remove(e.Client.Id);
    }

    public void Update()
    {
        Server.Update();
    }

    public void Stop()
    {
        Server.Stop();
    }

    public void SpawnPlayer(ushort clientId, string name, Vector2 position, uint initialServerSequenceId)
    {
        _players[clientId] = new Player(clientId, name);

        SendSpawnPlayer(clientId, position, initialServerSequenceId);
    }

    #region Handle client messages 

    [MessageHandler((ushort)ClientMessageType.JoinGame)]
    static void ReceivedClientNameHandler(ushort clientId, Message message)
    {
        Instance.ClientConnected?.Invoke(new ClientConnectedArgs(clientId, message));
    }

    [MessageHandler((ushort)ClientMessageType.SendPlayerCommands)]
    static void ReceivedClientStateHandler(ushort clientId, Message message)
    {
        Instance.ReceivedClientState?.Invoke(new ClientStateArgs(clientId, message));
    }

    #endregion

    #region Send server messages to client

    public void SendMessageToAll(Message message)
    {
        Server.SendToAll(message);
    }

    void SendSpawnPlayer(ushort clientId, Vector2 position, uint initialServerSequenceId)
    {
        // A player has joined - send a message so that the client can spawn them
        // Late joiners will need details of all the current players

        var message = Message.Create(MessageSendMode.Reliable, ServerMessageType.SpawnPlayer);
        message.AddByte((byte)_players.Count);
        foreach (var player in _players.Values)
        {
            message.AddUShort(player.ClientId);
            message.AddString(player.Name);

            message.AddUInt(initialServerSequenceId);

            if (clientId == player.ClientId)
                message.AddVector2(position);
            else
                message.AddVector2(Vector2.Zero);
        }

        Server.SendToAll(message);
    }

    #endregion
}
