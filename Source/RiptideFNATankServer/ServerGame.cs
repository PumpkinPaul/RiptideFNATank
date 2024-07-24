
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
using Riptide;
using RiptideFNATankCommon;
using RiptideFNATankServer.Networking;
using Wombat.Engine;

/// <summary>
/// Very simple multiplayer implementation of the server for the game, Tank using the Riptide framework, MoonTools.ECS and Quake3 style client / server multiplayer
/// </summary>
/// <remarks>
/// Based on
/// </remarks>
public class ServerGame : Game
{
    //------------------------------------------------------------------------------------------------------------------------------------------------------ 
    //------------------------------------------------------------------------------------------------------------------------------------------------------ 
    //------------------------------------------------------------------------------------------------------------------------------------------------------ 
    //Multiplayer
    readonly NetworkGameManager _networkGameManager;

    const int PLAYER_OFFSET_X = 32;

    readonly Vector2[] _playerSpawnPoints = [
        new Vector2(PLAYER_OFFSET_X, BaseGame.SCREEN_HEIGHT / 2),
        new Vector2(BaseGame.SCREEN_WIDTH - PLAYER_OFFSET_X, BaseGame.SCREEN_HEIGHT / 2)
    ];

    int _playerSpawnPointsIdx = 0;

    public ServerGame()
    {
        Window.Title = "Riptide FNA Tank - SERVER";

        Logger.Info("==================================================");
        Logger.Info($"{Window.Title}");
        Logger.Info("==================================================");

        _ = new GraphicsDeviceManager(this);

        TargetElapsedTime = TimeSpan.FromMicroseconds(1000.0f / 20);

        _networkGameManager = new(
            port: 17871,
            maxClientCount: 4);

        _networkGameManager.ClientConnected += ClientConnectedHandler;
    }

    protected override void Initialize()
    {
        _networkGameManager.StartServer();
    }

    protected override void Update(GameTime gameTime)
    {
        _networkGameManager.Update();
    }

    protected override void OnExiting(object sender, EventArgs args)
    {
        base.OnExiting(sender, args);

        _networkGameManager.Stop();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Red);
    }

    private void ClientConnectedHandler(NetworkGameManager.ClientConnectedArgs e)
    {
        var name = e.Message.GetString();

#if DEBUG
        Logger.Info($"Message handler: {nameof(ClientConnectedHandler)} from client: {e.ClientId}");
        Logger.Debug("Read the following...");
        Logger.Debug($"{name}");
#endif

        //TODO: probably need some logic here to do map stuff, get spawn points, etc
        var position = _playerSpawnPoints[_playerSpawnPointsIdx];
        _networkGameManager.SpawnPlayer(e.ClientId, name, position);

        PrepareNextPlayer();
    }

    void PrepareNextPlayer()
    {
        //Cycle through the spawn points so that players are located in the correct postions
        _playerSpawnPointsIdx = (_playerSpawnPointsIdx + 1) % _playerSpawnPoints.Length;
    }
}
