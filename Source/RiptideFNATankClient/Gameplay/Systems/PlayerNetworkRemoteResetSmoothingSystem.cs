// Copyright Pumpkin Games Ltd. All Rights Reserved.

using MoonTools.ECS;
using RiptideFNATank.Gameplay.Components;
using RiptideFNATank.RiptideMultiplayer;
using System;

namespace RiptideFNATank.Gameplay.Systems;

/// <summary>
/// Reads remote match data received messages and applies prediction to the 'simulation state' - e.g. the normal component data
/// </summary>
public sealed class PlayerNetworkRemoteResetSmoothingSystem : UpdatePaddleStateSystem
{
    readonly NetworkOptions _networkOptions;

    public PlayerNetworkRemoteResetSmoothingSystem(
        World world,
        NetworkOptions networkOptions
    ) : base(world)
    {
        _networkOptions = networkOptions;
    }

    public override void Update(TimeSpan delta)
    {
        if (_networkOptions.EnableSmoothing)
        {
            foreach (var message in ReadMessages<ReceivedRemotePaddleStateMessage>())
            {
                // Start a new smoothing interpolation from our current state toward this new state we just received.
                ref readonly var displayState = ref Get<DisplayStateComponent>(message.Entity);

                Set(message.Entity, new PreviousStateComponent
                {
                    PaddleState = displayState.PaddleState
                });

                Set(message.Entity, new SmoothingComponent(1));
            }
        }
        else
        {
            foreach (var message in ReadMessages<ReceivedRemotePaddleStateMessage>())
                Set(message.Entity, new SmoothingComponent(0));
        }
    }
}
