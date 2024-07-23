/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Riptide;
using Riptide.Utils;
using RiptideFNATankCommon;
using RiptideFNATankCommon.Networking;

namespace RiptideFNATankServer.Networking;

/// <summary>
/// Responsible for managing a networked game.
/// </summary>
public class NetworkGameManager
{
    static NetworkGameManager Instance;

    public Server Server { get; private set; }

    readonly ushort _port;
    readonly ushort _maxClientCount;

    readonly Dictionary<ushort, Player> _players = [];

    public NetworkGameManager(
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

    void SpawnPlayer(ushort clientId, string name)
    {
        _players[clientId] = new Player(clientId, name);

        SendSpawnPlayer(clientId, name);
    }

    #region Handle client messages 

    [MessageHandler((ushort)ClientMessageType.Name)]
    static void ReceivedName(ushort clientId, Message message)
    {
        var name = message.GetString();

#if DEBUG
        Logger.Info($"Message handler: {nameof(ReceivedName)} from client: {clientId}");
        Logger.Debug("Read the following...");
        Logger.Debug($"{name}");
#endif
        Instance.SpawnPlayer(clientId, name);
    }

    #endregion

    #region Send server messages 

    void SendSpawnPlayer(ushort clientId, string name)
    {
        var message = Message.Create(MessageSendMode.Reliable, ServerMessageType.SpawnPlayer);
        message.AddUShort(clientId);
        message.AddString(name);

        Server.SendToAll(message);
    }

    #endregion
}
