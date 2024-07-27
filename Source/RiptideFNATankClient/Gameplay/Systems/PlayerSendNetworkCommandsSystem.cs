/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using MoonTools.ECS;
using Riptide;
using RiptideFNATankClient.Networking;
using RiptideFNATankCommon.Components;
using RiptideFNATankCommon.Extensions;
using RiptideFNATankCommon.Networking;
using System;

namespace RiptideFNATankClient.Gameplay.Systems;

/// <summary>
/// Sends the local player's input commands / state to the server
/// </summary>
public sealed class PlayerSendNetworkCommandsSystem : MoonTools.ECS.System
{
    readonly NetworkGameManager _networkGameManager;
    readonly Timekeeper _timekeeper;

    // How often to send the player's state across the network
    public const int UPDATES_PER_SECOND = 60;
    private const float STATE_FREQUENCY = 1.0f / UPDATES_PER_SECOND;
    float _stateSyncTimer;

    /// <summary>
    /// An incrementing number so that messages can be ordered / ack'd.
    /// </summary>
    uint _sequenceId;

    readonly Filter _filter;

    public PlayerSendNetworkCommandsSystem(
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
        // Periodically send our state to everyone in the session.
        //if (_stateSyncTimer <= 0)
        {
            foreach (var entity in _filter.Entities)
            {
                ref readonly var position = ref Get<PositionComponent>(entity);
                ref readonly var velocity = ref Get<VelocityComponent>(entity);
                ref readonly var playerActions = ref Get<PlayerActionsComponent>(entity);

                var message = Message.Create(MessageSendMode.Unreliable, ClientMessageType.SendPlayerCommands);
                message.AddUInt(_sequenceId);

                // Add input commands.
                message.AddVector2(position.Value);

                // Also add our current inputs. These can be used to more accurately
                // predict how the player is likely to move in the future.
                message.AddBool(playerActions.MoveUp);
                message.AddBool(playerActions.MoveDown);

                // Send a network packet containing the player's state.
                _networkGameManager.SendMessage(message);
            }

            //_stateSyncTimer = STATE_FREQUENCY;
            _sequenceId++;
        }

        _stateSyncTimer -= (float)delta.TotalSeconds;
    }
}
