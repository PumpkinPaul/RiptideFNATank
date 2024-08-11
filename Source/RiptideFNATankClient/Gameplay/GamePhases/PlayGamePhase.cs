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
using Microsoft.Xna.Framework.Input;
using RiptideFNATankClient.Networking;
using RiptideFNATankCommon.Networking;
using System;
using Wombat.Engine;
using Wombat.Engine.Logging;

namespace RiptideFNATankClient.Gameplay.GamePhases;

static partial class Log
{
    [LoggerMessage(Message = "Client has quit the match.")]
    public static partial void QuitMatch(this ILogger logger, LogLevel logLevel);
}

/// <summary>
/// Playing the game phase
/// </summary>
/// <remarks>
/// Tanks are moving, turrets are rotating - all the bits that make up the gameplay part of the game.
/// </remarks>
public class PlayGamePhase : GamePhase
{
    //Multiplayer networking
    readonly NetworkGameManager _networkGameManager;

    //ECS
    ClientECSManager _ecsManager;

    //Mapping between networking and ECS
    readonly PlayerEntityMapper _playerEntityMapper = new();

    //------------------------------------------------------------------------------------------------------------------------------------------------------ 
    //------------------------------------------------------------------------------------------------------------------------------------------------------ 
    //------------------------------------------------------------------------------------------------------------------------------------------------------ 
    //Gameplay
    public event Action ExitedMatch;

    public PlayGamePhase(
        NetworkGameManager networkGameManager
    )
    {
        _networkGameManager = networkGameManager;
    }

    public override void Initialise()
    {
        base.Initialise();

        _ecsManager = new ClientECSManager(_networkGameManager, _playerEntityMapper);

        _networkGameManager.SpawnedLocalPlayer += _ecsManager.SpawnLocalPlayer;
        _networkGameManager.SpawnedRemotePlayer += _ecsManager.SpawnRemotePlayer;
        _networkGameManager.ReceivedWorldState += _ecsManager.ReceivedWorldState;
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        base.OnUpdate(gameTime);

        if (BaseGame.Instance.KeyboardState.IsKeyDown(Keys.Space) && BaseGame.Instance.PreviousKeyboardState.IsKeyUp(Keys.Space))
            QuitMatch();

        // Toggle prediction on or off?
        if (BaseGame.Instance.IsPressed(Keys.X, Buttons.X))
            _ecsManager.TogglePrediction();

        // Toggle smoothing on or off?
        if (BaseGame.Instance.IsPressed(Keys.Z, Buttons.Y))
            _ecsManager.ToggleSmoothing();

        _ecsManager.Update(gameTime);
    }

    protected override void OnDraw()
    {
        base.OnDraw();

        _ecsManager.Draw();
    }

    /// <summary>
    /// Quits the current match.
    /// </summary>
    public void QuitMatch()
    {
        Logger.Log.QuitMatch(LogLevel.Information);

        _networkGameManager.QuitMatch();

        ExitedMatch?.Invoke();
    }
}
