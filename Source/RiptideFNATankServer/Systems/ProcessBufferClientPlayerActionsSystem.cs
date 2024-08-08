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
using RiptideFNATankCommon;
using RiptideFNATankCommon.Components;
using RiptideFNATankCommon.Networking;
using RiptideFNATankServer.Gameplay;

namespace RiptideFNATankServer.Systems;

/// <summary>
/// 
/// </summary>
public sealed class ProcessBufferClientPlayerActionsSystem : MoonTools.ECS.System
{
    readonly PlayerEntityMapper _playerEntityMapper;
    readonly Dictionary<ushort, CommandsBuffer> _clientPlayerActionsBuffer;

    readonly Filter _filter;

    public ProcessBufferClientPlayerActionsSystem(
        World world,
        PlayerEntityMapper playerEntityMapper,
        Dictionary<ushort, CommandsBuffer> clientPlayerActionsBuffer
    ) : base(world)
    {
        _playerEntityMapper = playerEntityMapper;
        _clientPlayerActionsBuffer = clientPlayerActionsBuffer;

        _filter = FilterBuilder
            .Include<PlayerActionsComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        ref readonly var simulationState = ref GetSingleton<SimulationStateComponent>();

        foreach (var entity in _filter.Entities)
        {
            var clientId = _playerEntityMapper.GetClientIdFromEntity(entity);

            if (clientId == PlayerEntityMapper.INVALID_CLIENT_ID)
                continue;

            if (_clientPlayerActionsBuffer.TryGetValue(clientId, out var playerActionsQueue) == false)
                continue;

            var playerActions = playerActionsQueue.Remove(simulationState.CurrentServerCommandFrame);
            Set(entity, playerActions);

            Logger.Debug($"{nameof(ProcessBufferClientPlayerActionsSystem)}: Applied buffered client commands for command frame :{simulationState.CurrentServerCommandFrame} - {playerActions}");
        }
    }
}
