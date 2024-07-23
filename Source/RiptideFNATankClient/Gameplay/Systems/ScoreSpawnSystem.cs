// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using RiptideFNATank.Gameplay.Components;
using System;

namespace RiptideFNATank.Gameplay.Systems;

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