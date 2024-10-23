/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankCommon.Gameplay.Components;
using RiptideFNATankCommon.Networking;
using System;
using Wombat.Engine.Logging;

namespace RiptideFNATankClient.Gameplay.Systems;

public readonly record struct LocalPlayerSpawnMessage(
    ushort ClientId,
    uint InitialServerCommandFrame,
    PlayerIndex PlayerIndex,
    Keys MoveUpKey,
    Keys MoveDownKey,
    Vector2 Position,
    Color Color
);

static partial class Log
{
    [LoggerMessage(Message = "Player joined server on InitialServerCommandFrame: {initialServerCommandFrame}.")]
    public static partial void PlayerJoinedEvent(this ILogger logger, LogLevel logLevel, uint initialServerCommandFrame);

    [LoggerMessage(Message = "Command buffer size: {commandBufferSize} command frames, Half RTT esitmate is: {halfRTTInCommandFrame} command frames.")]
    public static partial void PlayerJoinedStats(this ILogger logger, LogLevel logLevel, uint commandBufferSize, uint halfRTTInCommandFrame);

    [LoggerMessage(Message = "Reseting CurrentClientCommandFrame to: {currentClientCommandFrame}.")]
    public static partial void ResettingClientCommandFrame(this ILogger logger, LogLevel logLevel, uint currentClientCommandFrame);
}

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
            Logger.Log.PlayerJoinedEvent(LogLevel.Information, message.InitialServerCommandFrame);

            var entity = CreateEntity();

            _playerEntityMapper.AddPlayer(message.ClientId, entity);

            ref var simulationState = ref GetSingleton<SimulationStateComponent>();

            // According to the "Overwatch Gameplay Architechture and Netcode" GDC talk the client should be ahead of the server
            // by a buffer (1 or 2 command frames "as small as possible") + half RTT
            // https://youtu.be/W3aieHjyNvw?list=PLvpI1FIKFk2ijMG-DT3ZfL7DZgAFtybLG&t=1536
            // Instead of syncing the command frames directly, we need to fast forward the client some command frames!

            // TODO: dynamically calculate this.
            uint halfRTTInCommandFrame = 3;
            simulationState.CurrentClientCommandFrame = message.InitialServerCommandFrame + NetworkSettings.COMMAND_BUFFER_SIZE + halfRTTInCommandFrame;
            simulationState.InitialClientCommandFrame = simulationState.CurrentClientCommandFrame;
            simulationState.ServerReceivedClientCommandFrame = simulationState.CurrentClientCommandFrame - 1;

            Logger.Log.PlayerJoinedStats(LogLevel.Information, NetworkSettings.COMMAND_BUFFER_SIZE, halfRTTInCommandFrame);
            Logger.Log.ResettingClientCommandFrame(LogLevel.Information, simulationState.CurrentClientCommandFrame);

            Set(entity, new PlayerInputComponent(message.PlayerIndex, message.MoveUpKey, message.MoveDownKey));
            Set(entity, new PositionComponent(message.Position));
            Set(entity, new RotationComponent(1.78f));
            Set(entity, new ScaleComponent(new Vector2(16, 64)));
            Set(entity, new ColorComponent(message.Color));
            Set(entity, new VelocityComponent());
        }
    }
}