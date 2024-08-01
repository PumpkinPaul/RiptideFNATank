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
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankClient.Networking;
using RiptideFNATankCommon;
using RiptideFNATankCommon.Components;
using RiptideFNATankCommon.Networking;
using System;
using Wombat.Engine;

namespace RiptideFNATankClient.Gameplay.Systems;

/// <summary>
/// Sends the local player's input commands / state to the server
/// </summary>
public sealed class PlayerSendNetworkCommandsSystem : MoonTools.ECS.System
{
    readonly NetworkGameManager _networkGameManager;

    // How often to send the player's state across the network
    public const int UPDATES_PER_SECOND = 60;
    private const float STATE_FREQUENCY = 1.0f / UPDATES_PER_SECOND;
    float _stateSyncTimer;

    readonly Filter _filter;

    public PlayerSendNetworkCommandsSystem(
        World world,
        NetworkGameManager networkGameManager
    ) : base(world)
    {
        _networkGameManager = networkGameManager;

        _filter = FilterBuilder
            .Include<PlayerActionsComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        // Periodically send our state to everyone in the session.
        //if (_stateSyncTimer <= 0)
        {

            ref readonly var simulationState = ref GetSingleton<SimulationStateComponent>();

            foreach (var entity in _filter.Entities)
            {
                if (RandomHelper.FastRandom.PercentageChance(_networkGameManager.DroppedPacketPercentage))
                    continue;

                ref readonly var playerActions = ref Get<PlayerActionsComponent>(entity);

                var message = Message.Create(MessageSendMode.Unreliable, ClientMessageType.SendPlayerCommands);

                // Header
                message.AddUInt(0); //TODO: sequence number of last received server message
                byte gameId = 0;
                message.AddByte(gameId);
                message.AddUInt(simulationState.LastReceivedServerTick);

                // Payload
                message.AddUShort(0); //TODO: Client prediction in milliseconds
                message.AddUInt(simulationState.CurrentClientTick);
                message.AddByte(1); //TODO: number of user commands

                // Add user commands.
                message.AddBool(playerActions.MoveUp);
                message.AddBool(playerActions.MoveDown);

                // Send a network packet containing the player's state.
                _networkGameManager.SendMessage(message);

                Logger.Info($"Sending state to server for simulationState.CurrentClientTick: {simulationState.CurrentClientTick}, playerActions.MoveUp: {playerActions.MoveUp}, position: {playerActions.MoveDown}");
            }

            //_stateSyncTimer = STATE_FREQUENCY;
        }

        _stateSyncTimer -= (float)delta.TotalSeconds;
    }
}
