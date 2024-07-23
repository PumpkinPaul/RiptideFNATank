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
using Microsoft.Xna.Framework.Input;
using Wombat.Engine;
using RiptideFNATank.RiptideMultiplayer;
using RiptideFNATankCommon;
using System;

namespace RiptideFNATank.Gameplay.GamePhases;

/// <summary>
/// Playing the game phase
/// </summary>
/// <remarks>
/// Tanks are moving, turrets are rotating - all the bits that make up the gameplay part of the game.
/// </remarks>
public class PlayGamePhase : GamePhase
{
    MultiplayerGameState _gameState;

    //Multiplayer networking
    readonly NetworkGameManager _networkGameManager;

    //ECS
    ECSManager _ecsManager;

    //Mapping between networking and ECS
    readonly PlayerEntityMapper _playerEntityMapper = new();

    //------------------------------------------------------------------------------------------------------------------------------------------------------ 
    //------------------------------------------------------------------------------------------------------------------------------------------------------ 
    //------------------------------------------------------------------------------------------------------------------------------------------------------ 
    //Gameplay
    public event EventHandler ExitedMatch;

    const int PLAYER_OFFSET_X = 32;

    readonly Vector2[] _playerSpawnPoints = new[] {
        new Vector2(PLAYER_OFFSET_X, BaseGame.SCREEN_HEIGHT / 2),
        new Vector2(BaseGame.SCREEN_WIDTH - PLAYER_OFFSET_X, BaseGame.SCREEN_HEIGHT / 2)
    };

    public PlayGamePhase(
        NetworkGameManager networkGameManager)
    {
        _networkGameManager = networkGameManager;
    }

    public override void Initialise()
    {
        base.Initialise();

        _gameState = new MultiplayerGameState();

        _ecsManager = new ECSManager(_networkGameManager, _playerEntityMapper, _gameState);
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
        Logger.Info($"PlayGamePhase.QuitMatch");

        _networkGameManager.QuitMatch();

        ExitedMatch?.Invoke(this, EventArgs.Empty);
    }
}
