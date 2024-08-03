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
using RiptideFNATankCommon;
using RiptideFNATankCommon.Components;
using RiptideFNATankCommon.Networking;
using System;

namespace RiptideFNATankClient.Gameplay.Systems;

public readonly record struct LocalPlayerSpawnMessage(
    ushort ClientId,
    uint InitialServerTick,
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
    readonly PlayerEntityMapper _playerEntityMapper;

    public LocalPlayerSpawnSystem(
        World world,
        PlayerEntityMapper playerEntityMapper
    ) : base(world)
    {
        _playerEntityMapper = playerEntityMapper;
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var message in ReadMessages<LocalPlayerSpawnMessage>())
        {
            var entity = CreateEntity();

            _playerEntityMapper.AddPlayer(message.ClientId, entity);

            ref var simulationState = ref GetSingleton<SimulationStateComponent>();
            simulationState.InitialServerTick = message.InitialServerTick;

            Logger.Success($"Player joined server on InitialServerTick: {message.InitialServerTick}. Client tick was {simulationState.CurrentClientTick}");

            // We are force reseting the client world tick here to match the server tick.
            // Note - our ticks are fixed rate ticks and are analogous to the Overwatch 'command frame'

            // According to the "Overwatch Gameplay Architechture and Netcode" GDC talk the client should be ahead of the server
            // by a buffer (1 or 2 command frames "as small as possible") + half RTT
            // https://youtu.be/W3aieHjyNvw?list=PLvpI1FIKFk2ijMG-DT3ZfL7DZgAFtybLG&t=1536
            // Instead of syncing the ticks, we need to fast forward the client some command frames!

            // TODO: dynamically calculate this.
            uint halfRTTInTicks = 2;
            simulationState.CurrentClientTick = message.InitialServerTick + NetworkSettings.COMMAND_BUFFER_SIZE + halfRTTInTicks;

            Logger.Success($"Fixed command buffer size is: {NetworkSettings.COMMAND_BUFFER_SIZE} ticks");
            Logger.Success($"Half RTT esitmate is: {halfRTTInTicks} ticks");
            Logger.Success($"Reseting CurrentClientTick to: {simulationState.CurrentClientTick}");

            Set(entity, new PlayerInputComponent(message.PlayerIndex, message.MoveUpKey, message.MoveDownKey));
            Set(entity, new PositionComponent(message.Position));
            Set(entity, new ScaleComponent(new Vector2(16, 64)));
            Set(entity, new ColorComponent(message.Color));
            Set(entity, new VelocityComponent());
        }
    }
}