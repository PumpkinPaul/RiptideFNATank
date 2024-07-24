// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankClient.Networking;
using System;

namespace RiptideFNATankClient.Gameplay.Systems;

public readonly record struct RemotePlayerSpawnMessage(
    PlayerIndex PlayerIndex,
    Vector2 Position,
    Color Color
);

/// <summary>
/// Spawns remote networked player entities with the correct components.
/// </summary>
public class RemotePlayerSpawnSystem : MoonTools.ECS.System
{
    readonly PlayerEntityMapper _playerEntityMapper;

    public RemotePlayerSpawnSystem(
        World world,
        PlayerEntityMapper playerEntityMapper
    ) : base(world)
    {
        _playerEntityMapper = playerEntityMapper;
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var message in ReadMessages<RemotePlayerSpawnMessage>())
        {
            var entity = CreateEntity();

            _playerEntityMapper.MapEntity(message.PlayerIndex, entity);

            Set(entity, new PositionComponent(message.Position));
            Set(entity, new ScaleComponent(new Vector2(16, 64)));
            Set(entity, new ColorComponent(message.Color));
            Set(entity, new VelocityComponent());

            var paddleState = new PaddleState
            {
                Position = message.Position
            };

            Set(entity, new SimulationStateComponent
            {
                PaddleState = paddleState
            });
            Set(entity, new PreviousStateComponent
            {
                PaddleState = paddleState
            });
            Set(entity, new DisplayStateComponent
            {
                PaddleState = paddleState
            });
        }
    }
}