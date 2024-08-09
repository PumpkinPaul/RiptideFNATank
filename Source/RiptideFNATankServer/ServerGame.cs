/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

namespace RiptideFNATankServer;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiptideFNATankCommon;
using RiptideFNATankCommon.Networking;
using RiptideFNATankServer.Gameplay;
using RiptideFNATankServer.Networking;
using Wombat.Engine;

/// <summary>
/// Very simple multiplayer implementation of the server for the game, Tank using the Riptide framework, MoonTools.ECS and Quake3 style client / server multiplayer
/// </summary>
/// <remarks>
/// Based on
/// </remarks>
public class ServerGame : BaseGame
{
    // Multiplayer
    ServerNetworkManager _networkGameManager;

    // ECS
    ServerECSManager _ecsManager;

    // Maps
    const int PLAYER_OFFSET_X = 32;

    readonly Vector2[] _playerSpawnPoints = [
        new Vector2(PLAYER_OFFSET_X, SCREEN_HEIGHT / 2),
        new Vector2(SCREEN_WIDTH - PLAYER_OFFSET_X, SCREEN_HEIGHT / 2)
    ];

    int _playerSpawnPointsIdx = 0;

    //HACK
    public static uint ServerCommandFrame;

    public ServerGame()
    {
        Window.Title = "Riptide FNA Tank - SERVER";

        Logger.Info("==================================================");
        Logger.Info($"{Window.Title}");
        Logger.Info("==================================================");

        TargetElapsedTime = NetworkSettings.PhsyicsTimeSpan;
    }

    protected override void Initialize()
    {
        base.Initialize();

        _networkGameManager = new(
            port: NetworkSettings.PORT,
            maxClientCount: NetworkSettings.MAX_PLAYERS);

        _ecsManager = new ServerECSManager(_networkGameManager, SpriteBatch);

        _networkGameManager.ClientConnected += ClientConnectedHandler;
        _networkGameManager.ReceivedPlayerCommands += _ecsManager.PlayerCommandsReceivedHandler;

        _networkGameManager.StartServer();
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        _networkGameManager.Update();
        _ecsManager.Update(gameTime);
    }

    protected override void OnExiting(object sender, EventArgs args)
    {
        base.OnExiting(sender, args);

        _networkGameManager.Stop();
    }

    protected override void OnDraw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.DarkGray);
        _ecsManager.Draw();
    }

    private void ClientConnectedHandler(ServerNetworkManager.ClientConnectedArgs e)
    {
        var name = e.Message.GetString();
        var position = _playerSpawnPoints[_playerSpawnPointsIdx];
        _networkGameManager.SpawnPlayer(e.ClientId, name, position, ServerCommandFrame);

        _ecsManager.SpawnPlayer(e.ClientId, name, position);

        PrepareNextPlayer();
    }

    void PrepareNextPlayer()
    {
        // Cycle through the spawn points so that players are located in the correct postions
        _playerSpawnPointsIdx = (_playerSpawnPointsIdx + 1) % _playerSpawnPoints.Length;
    }
}
