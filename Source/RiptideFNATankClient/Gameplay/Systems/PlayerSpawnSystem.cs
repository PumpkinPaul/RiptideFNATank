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
using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankCommon.Components;
using System;

namespace RiptideFNATankClient.Gameplay.Systems;

public readonly record struct PlayerSpawnMessage(
    PlayerIndex PlayerIndex,
    Keys MoveUpKey,
    Keys MoveDownKey,
    Vector2 Position,
    Color Color,
    int BounceDirection
);

/// <summary>
/// Responsible for spawning Player entities with the correct components.
/// </summary>
public class PlayerSpawnSystem : MoonTools.ECS.System
{
    public PlayerSpawnSystem(World world) : base(world)
    {
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var message in ReadMessages<PlayerSpawnMessage>())
        {
            var entity = CreateEntity();

            Set(entity, new PlayerInputComponent(message.PlayerIndex, message.MoveUpKey, message.MoveDownKey));
            Set(entity, new PositionComponent(message.Position));
            Set(entity, new ScaleComponent(new Vector2(16, 64)));
            Set(entity, new ColorComponent(message.Color));
            Set(entity, new VelocityComponent());
        }
    }
}