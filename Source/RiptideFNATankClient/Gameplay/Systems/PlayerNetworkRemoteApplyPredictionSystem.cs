// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using RiptideFNATank.Gameplay.Components;
using RiptideFNATank.RiptideMultiplayer;
using System;
using System.Collections.Generic;

namespace RiptideFNATank.Gameplay.Systems;

/// <summary>
/// Reads remote match data received messages and applies prediction to the 'simulation state' - e.g. the normal component data
/// </summary>
public sealed class PlayerNetworkRemoteApplyPredictionSystem : UpdatePaddleStateSystem
{
    readonly Timekeeper _timekeeper;
    readonly NetworkOptions _networkOptions;
    readonly TimeSpan _oneFrame;

    readonly Dictionary<Entity, RollingAverage> _clockDeltas = new();

    public PlayerNetworkRemoteApplyPredictionSystem(
        World world,
        Timekeeper timekeeper,
        NetworkOptions networkOptions,
        TimeSpan oneFrame
        ) : base(world)
    {
        _timekeeper = timekeeper;
        _networkOptions = networkOptions;
        _oneFrame = oneFrame;
    }

    public override void Update(TimeSpan delta)
    {
        if (_networkOptions.EnablePrediction == false)
            return;

        foreach (var message in ReadMessages<ReceivedRemotePaddleStateMessage>())
        {
            //Estimate how long this packet took to arrive.
            //TODO! figure out how to do latency simulation using Riptide.
            var latency = TimeSpan.FromSeconds(1 / 20.0f);

            if (_clockDeltas.TryGetValue(message.Entity, out var clockDelta) == false)
            {
                clockDelta = new RollingAverage();
                _clockDeltas[message.Entity] = clockDelta;
            }

            // Work out the difference between our current local time
            // and the remote time at which this packet was sent.
            float localTime = (float)_timekeeper.GameTime.TotalGameTime.TotalSeconds;

            float timeDelta = localTime - message.TotalSeconds;

            // Maintain a rolling average of time deltas from the last 100 packets.
            clockDelta.AddValue(timeDelta);

            // The caller passed in an estimate of the average network latency, which
            // is provided by the XNA Framework networking layer. But not all packets
            // will take exactly that average amount of time to arrive! To handle
            // varying latencies per packet, we include the send time as part of our
            // packet data. By comparing this with a rolling average of the last 100
            // send times, we can detect packets that are later or earlier than usual,
            // even without having synchronized clocks between the two machines. We
            // then adjust our average latency estimate by this per-packet deviation.

            float timeDeviation = timeDelta - clockDelta.AverageValue;

            latency += TimeSpan.FromSeconds(timeDeviation);

            // Apply prediction by updating our simulation state however
            // many times is necessary to catch up to the current time.
            ref var simulationState = ref GetMutable<SimulationStateComponent>(message.Entity);
            while (latency >= _oneFrame)
            {
                UpdateState(message.Entity, ref simulationState.PaddleState);

                latency -= _oneFrame;
            }
        }
    }
}
