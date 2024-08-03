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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Wombat.Engine;
using RiptideFNATankCommon;
using System;
using RiptideFNATankClient.Gameplay.GamePhases;
using RiptideFNATankClient.Networking;
using RiptideFNATankCommon.Networking;

namespace RiptideFNATankClient;

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

        Logger.Info("==================================================");
        Logger.Info($"{Window.Title}");
        Logger.Info("==================================================");

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
