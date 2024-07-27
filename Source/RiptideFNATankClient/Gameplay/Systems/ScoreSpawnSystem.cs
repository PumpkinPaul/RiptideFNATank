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
using MoonTools.ECS;
using RiptideFNATankCommon.Components;
using System;

namespace RiptideFNATankClient.Gameplay.Systems;

public readonly record struct ScoreSpawnMessage(
    PlayerIndex PlayerIndex,
    Vector2 Position
);

/// <summary>
/// Responsible for spawning Player entities with the correct components.
/// </summary>
public class ScoreSpawnSystem : MoonTools.ECS.System
{
    public ScoreSpawnSystem(World world) : base(world)
    {
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var message in ReadMessages<ScoreSpawnMessage>())
        {
            var entity = CreateEntity();

            Set(entity, new PositionComponent(message.Position));
            Set(entity, new ScoreComponent());
        }
    }
}