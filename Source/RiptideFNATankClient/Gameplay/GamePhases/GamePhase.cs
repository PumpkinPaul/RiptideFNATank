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

namespace RiptideFNATankClient.Gameplay.GamePhases;

/// <summary>
/// Base class for all the different game phases (e.g. main menu, playing the game, game over, etc).
/// </summary>
public abstract class GamePhase
{
    /// <summary>The number of ticks since the state was created.</summary>
    protected int ElapsedTicks;

    public virtual void Initialise() { }

    public void Create()
    {
        ElapsedTicks = 0;

        OnCreate();
    }

    public virtual bool SupportsPause => false;

    public void Update(GameTime gameTime)
    {
        ElapsedTicks++;

        OnUpdate(gameTime);
    }

    public void Draw() => OnDraw();

    public void Destroy() => OnDestroy();

    protected virtual void OnCreate() { }
    protected virtual void OnUpdate(GameTime gameTime) { }
    protected virtual void OnDraw() { }
    protected virtual void OnDestroy() { }
}
