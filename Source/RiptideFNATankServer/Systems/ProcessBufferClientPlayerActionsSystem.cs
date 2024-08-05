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
using RiptideFNATankCommon.Components;
using RiptideFNATankCommon.Networking;

namespace RiptideFNATankServer.Gameplay.Systems;

/// <summary>
/// 
/// </summary>
public sealed class ProcessBufferClientPlayerActionsSystem : MoonTools.ECS.System
{
    readonly PlayerEntityMapper _playerEntityMapper;
    readonly Dictionary<ushort, PriorityQueue<ClientPlayerActions, uint>> _clientPlayerActionsBuffer;

    readonly Filter _filter;

    public ProcessBufferClientPlayerActionsSystem(
        World world,
        PlayerEntityMapper playerEntityMapper,
        Dictionary<ushort, PriorityQueue<ClientPlayerActions, uint>> clientPlayerActionsBuffer
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

            // Look in the buffer for a player command for the current command frame.
            // If there isn't one then the previous command will be used (the component will already contain the values used last command frame)

            if (playerActionsQueue.Count == 0)
            {
                // TODO: we probably need to stamp the outgoing state with a 'no input increase client sim tick rate' flag
                continue;
            }

            // Remove any old stale input that is for command frames in the past.
            ClientPlayerActions bufferedPlayerActions;
            uint bufferCommandFramePriority;
            while (playerActionsQueue.TryPeek(out bufferedPlayerActions, out bufferCommandFramePriority) && bufferCommandFramePriority < simulationState.CurrentServerCommandFrame)
            {
                playerActionsQueue.Dequeue();
            }
 
            // Skip if the remaining actions are for future command frames
            if (bufferCommandFramePriority > simulationState.CurrentServerCommandFrame)
                continue;

            // The buffered actions are for this command frame - apply them to the entity state
            Set(entity, new PlayerActionsComponent(bufferedPlayerActions.MoveUp, bufferedPlayerActions.MoveDown));
        }
    }
}
