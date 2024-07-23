
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
using RiptideFNATankServer.Networking;

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
}
