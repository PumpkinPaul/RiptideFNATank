// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using System;

namespace RiptideFNATankClient.Gameplay.Systems;

public readonly record struct LocalPlayerSpawnMessage(
    PlayerIndex PlayerIndex,
    Keys MoveUpKey,
    Keys MoveDownKey,
    Vector2 Position,
    Color Color
);

/// <summary>
/// Responsible for spawning local player entities with the correct components.
/// </summary>
public class LocalPlayerSpawnSystem : MoonTools.ECS.System
{
    public LocalPlayerSpawnSystem(World world) : base(world)
    {
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var message in ReadMessages<LocalPlayerSpawnMessage>())
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