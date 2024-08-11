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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiptideFNATankClient.Gameplay.GamePhases;
using RiptideFNATankClient.Networking;
using RiptideFNATankCommon.Networking;
using System;
using Wombat.Engine;
using Wombat.Engine.Logging;

namespace RiptideFNATankClient;

static partial class Log
{
    [LoggerMessage(Message = "Client running too slow, speed up >>>>> - desired ({desiredDifference}) vs actual ({actualDifference})")]
    public static partial void ClientRunningSlow(this ILogger logger, LogLevel logLevel, int desiredDifference, uint actualDifference);

    [LoggerMessage(Message = "Client running too fast, slow down <<<<< - desired ({desiredDifference}) vs actual ({actualDifference})")]
    public static partial void ClientRunningFast(this ILogger logger, LogLevel logLevel, int desiredDifference, uint actualDifference);

    [LoggerMessage(Message = "Starting game for {title}.")]
    public static partial void StartClient(this ILogger logger, LogLevel logLevel, string title);
}

/// <summary>
/// Very simple multiplayer implementation of the game, Tank using the Riptide framework, MoonTools.ECS and Quake3 style client / server multiplayer
/// </summary>
/// <remarks>
/// Based on
/// </remarks>
public class ClientGame : BaseGame
{
    public static string Name = "Player";

    public readonly GamePhaseManager GamePhaseManager;

    readonly PlayerProfile _playerProfile;

    //------------------------------------------------------------------------------------------------------------------------------------------------------ 
    //------------------------------------------------------------------------------------------------------------------------------------------------------ 
    //------------------------------------------------------------------------------------------------------------------------------------------------------ 
    //Multiplayer
    readonly NetworkGameManager _networkGameManager;

    public ClientGame()
    {
        Window.Title = "Riptide FNA Tank - CLIENT";

        Logger.CreateLogger("client");
        Logger.Log.StartClient(LogLevel.Information, Window.Title);

        // This can fluctuate on the client if the client needs to speed up and send more inputs to the server.
        TargetElapsedTime = NetworkSettings.PhsyicsTimeSpan;

        _playerProfile = PlayerProfile.LoadOrCreate(LocalApplicationDataPath);

        _networkGameManager = new NetworkGameManager("127.0.0.1", NetworkSettings.PORT);
        _networkGameManager.LocalClientConnected += () => GamePhaseManager.ChangePhase<PlayGamePhase>();

        GamePhaseManager = new GamePhaseManager();
        GamePhaseManager.Add(new MainMenuPhase(_networkGameManager));
        GamePhaseManager.Add(new PlayGamePhase(_networkGameManager));

        // Show the main menu, hide the in-game menu when player quits the match
        GamePhaseManager.Get<PlayGamePhase>().ExitedMatch += () => GamePhaseManager.ChangePhase<MainMenuPhase>();
    }

    protected override void Initialize()
    {
        base.Initialize();

        _networkGameManager.Start();

        GamePhaseManager.Initialise();
        GamePhaseManager.ChangePhase<MainMenuPhase>();
    }

    public void ChangeSimulationRate(float rate, int desiredDifference, uint actualDifference)
    {
        if (rate > 0)
        {
            TargetElapsedTime = NetworkSettings.SpeedUpTimeSpan;
            Logger.Log.ClientRunningSlow(LogLevel.Warning, desiredDifference, actualDifference);
        }
        else if (rate < 0)
        {
            TargetElapsedTime = NetworkSettings.SlowDownTimeSpan;
            Logger.Log.ClientRunningFast(LogLevel.Warning, desiredDifference, actualDifference);
        }
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        if (KeyboardState.IsKeyDown(Keys.Escape) && PreviousKeyboardState.IsKeyUp(Keys.Escape))
            Exit();

        GamePhaseManager.Update(gameTime);
        _networkGameManager.Update();
    }

    protected override void OnExiting(object sender, EventArgs args)
    {
        base.OnExiting(sender, args);

        _networkGameManager.Disconnect();
    }

    protected override void OnDraw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        GamePhaseManager.Draw();
    }
}
