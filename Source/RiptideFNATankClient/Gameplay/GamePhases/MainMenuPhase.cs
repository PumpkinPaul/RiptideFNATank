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
using Wombat.Engine.Extensions;
using RiptideFNATank.RiptideMultiplayer;

namespace RiptideFNATank.Gameplay.GamePhases;

/// <summary>
/// Main Menu Processing
/// </summary>
public class MainMenuPhase : GamePhase
{
    enum Phase
    {
        Ready,
        Connected
    }

    Phase _phase = Phase.Ready;

    readonly NetworkGameManager _networkGameManager;

    public MainMenuPhase(
        NetworkGameManager networkGameManager)
    {
        _networkGameManager = networkGameManager;
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        _phase = Phase.Ready;
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        base.OnUpdate(gameTime);

        if (BaseGame.Instance.KeyboardState.IsKeyDown(Keys.Space) && BaseGame.Instance.PreviousKeyboardState.IsKeyUp(Keys.Space))
        {
            if (_phase == Phase.Ready)
            {
                _phase = Phase.Connected;
                _networkGameManager.Connect();
            }
            else
            {
                _phase = Phase.Ready;
                _networkGameManager.Disconnect();
            }
        }
    }

    protected override void OnDraw()
    {
        base.OnDraw();

        var spriteBatch = BaseGame.Instance.SpriteBatch;

        var centreX = BaseGame.SCREEN_WIDTH * 0.5f;

        //Draw the UI
        spriteBatch.BeginTextRendering();

        spriteBatch.DrawText(Resources.GameFont, "Tank", new Vector2(centreX, BaseGame.SCREEN_HEIGHT * 0.65f), Color.White, Alignment.Centre);

        switch (_phase)
        {
            case Phase.Ready:
                spriteBatch.DrawText(Resources.SmallFont, "Press SPACE to play!", new Vector2(centreX, 220), Color.White, Alignment.Centre);
                break;

            case Phase.Connected:
                spriteBatch.DrawText(Resources.SmallFont, "Searching for match", new Vector2(centreX, 220), Color.White, Alignment.Centre);
                spriteBatch.DrawText(Resources.SmallFont, "Press SPACE to cancel", new Vector2(centreX, 180), Color.White, Alignment.Centre);
                break;
        }

        spriteBatch.End();
    }
}
