// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using Wombat.Engine.IO;
using RiptideFNATank.Gameplay.Components;
using RiptideFNATank.RiptideMultiplayer;
using System;
using System.IO;

namespace RiptideFNATank.Gameplay.Systems;

/// <summary>
/// Syncs the local player's state across the network by sending frequent network packets containing relevent 
/// information such as velocity, position and game actions (jump, shoot, crouch, etc).
/// </summary>
public sealed class PlayerNetworkSendLocalStateSystem : MoonTools.ECS.System
{
    readonly NetworkGameManager _networkGameManager;
    readonly Timekeeper _timekeeper;

    // How often to send the player's velocity and position across the network, in seconds.
    public const int UPDATES_PER_SECOND = 10;
    readonly float StateFrequency = 1.0f / UPDATES_PER_SECOND;
    float _stateSyncTimer;

    //Packet writer to writer all paddle state required each tick
    readonly PacketWriter _packetWriter = new(new MemoryStream(28));

    readonly Filter _filter;

    public PlayerNetworkSendLocalStateSystem(
        World world,
        NetworkGameManager networkGameManager,
        Timekeeper timekeeper
    ) : base(world)
    {
        _networkGameManager = networkGameManager;
        _timekeeper = timekeeper;

        _filter = FilterBuilder
            .Include<PositionComponent>()
            .Include<VelocityComponent>()
            .Include<PlayerActionsComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in _filter.Entities)
        {
            ref readonly var position = ref Get<PositionComponent>(entity);
            ref readonly var velocity = ref Get<VelocityComponent>(entity);
            ref readonly var playerActions = ref Get<PlayerActionsComponent>(entity);

            // Periodically send our state to everyone in the session.
            if (_stateSyncTimer <= 0)
            {
                _packetWriter.Reset();
                _packetWriter.Write((float)_timekeeper.GameTime.TotalGameTime.TotalSeconds);

                // Send the current state of the paddle.
                _packetWriter.Write(position.Value);
                _packetWriter.Write(velocity.Value);

                // Also send our current inputs. These can be used to more accurately
                // predict how the paddle is likely to move in the future.
                _packetWriter.Write(playerActions.MoveUp);
                _packetWriter.Write(playerActions.MoveDown);

                // Send a network packet containing the player's state.
                _networkGameManager.SendMatchState(
                    OpCodes.PADDLE_PACKET,
                    _packetWriter.GetBuffer());

                _stateSyncTimer = StateFrequency;
            }

            _stateSyncTimer -= (float)delta.TotalSeconds;
        }
    }
}
