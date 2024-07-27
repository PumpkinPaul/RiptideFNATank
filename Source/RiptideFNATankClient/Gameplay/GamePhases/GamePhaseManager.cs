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
using Wombat.Engine.Collections;
using System;

namespace RiptideFNATankClient.Gameplay.GamePhases;

/// <summary>
/// Manages the game's distinct phases - Splash, Intro, MainMenu, Play, etc.
/// </summary>
public class GamePhaseManager
{
    public GamePhase ActivePhase { get; private set; }

    private readonly SimpleHashList<Type, GamePhase> _gamePhases = new();

    public void Add(GamePhase gamePhase)
    {
        if (gamePhase == null)
            throw new ArgumentNullException(nameof(gamePhase));

        var key = gamePhase.GetType();

        _gamePhases[key] = gamePhase;
    }

    public void Initialise()
    {
        foreach (var phase in _gamePhases)
            phase.Initialise();
    }

    public void ChangePhase<T>() where T : GamePhase
    {
        ActivePhase?.Destroy();

        var newPhase = Get<T>();

        ActivePhase = newPhase;
        ActivePhase.Create();
    }

    public T Get<T>() where T : GamePhase => (T)_gamePhases[typeof(T)];

    public void Update(GameTime gameTime) => ActivePhase?.Update(gameTime);

    public void Draw() => ActivePhase?.Draw();
}

