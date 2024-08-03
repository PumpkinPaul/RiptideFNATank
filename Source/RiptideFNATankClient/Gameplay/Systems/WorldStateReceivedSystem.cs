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
using MoonTools.ECS;
using RiptideFNATankClient.Gameplay.Components;
using RiptideFNATankCommon;
using RiptideFNATankCommon.Components;
using RiptideFNATankCommon.Networking;
using System;

namespace RiptideFNATankClient.Gameplay.Systems;

public readonly record struct ReceivedWorldStateMessage(
    ushort ClientId,
    uint ServerTick,
    uint ClientTick,
    Vector2 Position
);

public record struct SimulationStateComponent(
    uint InitialServerTick,
    uint LastReceivedServerTick,
    uint CurrentClientTick,
    uint ServerProcessedClientInputAtClientTick
);

/// <summary>
/// Handles messages from the network manager indicating new world state has arrived.
/// <para>
/// Will update the "master game" and add a new snapshot into the dtate buffer.
/// </para>
/// </summary>
public class WorldStateReceivedSystem : MoonTools.ECS.System
{
    readonly PlayerEntityMapper _playerEntityMapper;
    readonly CircularBuffer<ServerPlayerState> _serverPlayerStateSnapshots;

    public WorldStateReceivedSystem(
        World world,
        PlayerEntityMapper playerEntityMapper,
        CircularBuffer<ServerPlayerState> serverPlayerStateSnapshots
    ) : base(world)
    {
        _playerEntityMapper = playerEntityMapper;
        _serverPlayerStateSnapshots = serverPlayerStateSnapshots;
    }

    public override void Update(TimeSpan delta)
    {
        // TODO: I think we need to buffer these to avoid jitter.
        var span = ReadMessages<ReceivedWorldStateMessage>();
        if (span.Length > 1)
        {
            Logger.Warning($"Received multiple server messages");
        }

        ref var simulationState = ref GetSingleton<SimulationStateComponent>();

        foreach (var message in span)
        {
            if (IsValidPacket(message.ServerTick, simulationState.LastReceivedServerTick) == false)
                continue;

            //Logger.Info($"Received a valid packet from server for sequence: {message.ServerTick}.");

            var entity = _playerEntityMapper.GetEntityFromClientId(message.ClientId);

            if (entity == PlayerEntityMapper.INVALID_ENTITY)
                continue;

            if (Has<PlayerInputComponent>(entity))
            {
                simulationState.LastReceivedServerTick = message.ServerTick;
                simulationState.ServerProcessedClientInputAtClientTick = message.ClientTick;

                if (simulationState.ServerProcessedClientInputAtClientTick > 0)
                {
                    var serverPlayerState = new ServerPlayerState(message.Position);
                    var idx = _serverPlayerStateSnapshots.Set(simulationState.ServerProcessedClientInputAtClientTick, serverPlayerState);

                    Logger.Info($"Wrote server state snpshot for ServerProcessedClientInputAtClientTick: {simulationState.ServerProcessedClientInputAtClientTick}, resolves to idx: {idx}, CurrentClientTick: {simulationState.CurrentClientTick}, Position: {serverPlayerState.Position}");
                }
            }
            else
            {
                // Remote player
                if (Has<PositionComponent>(entity))
                {
                    ref var position = ref Get<PositionComponent>(entity);
                    position.Value = message.Position;
                }
            }
        }
    }

    /// <summary>
    /// Checks to see if the packet just received is valid.
    /// <para>
    /// Due to the way UDP works, it could be old, duplicated, etc
    /// </para>
    /// </summary>
    /// <param name="justReceivedServerTick">The tick on the server this packet is for.</param>
    /// <param name="lastReceivedServerTick">The tick of the most recent packet received by us (the client).</param>
    /// <returns></returns>
    static bool IsValidPacket(uint justReceivedServerTick, uint lastReceivedServerTick)
    {
        if (justReceivedServerTick < lastReceivedServerTick)
        {
            // Discard packet
            Logger.Warning($"Received an old packet from server for sequence: {justReceivedServerTick}. Client has already received state for sequence: {lastReceivedServerTick}.");
            return false;
        }
        //HACK: Remove this when the server creates the state proper!
        else if (false && justReceivedServerTick == lastReceivedServerTick)
        {
            // Duplicate packet?
            Logger.Warning($"Received a duplicate packet from server for sequence: {justReceivedServerTick}.");
            return false;
        }

        return true;
    }
}
