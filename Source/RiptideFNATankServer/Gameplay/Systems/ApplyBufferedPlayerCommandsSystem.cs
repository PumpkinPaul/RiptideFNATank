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
using RiptideFNATankCommon.Gameplay.Components;
using RiptideFNATankCommon.Networking;

namespace RiptideFNATankServer.Gameplay.Systems;

/// <summary>
/// 
/// </summary>
public sealed class ApplyBufferedPlayerCommandsSystem : MoonTools.ECS.System
{
    readonly PlayerEntityMapper _playerEntityMapper;
    readonly Dictionary<ushort, CommandsBuffer> _clientPlayerCommandsBuffer;

    readonly Filter _filter;

    public ApplyBufferedPlayerCommandsSystem(
        World world,
        PlayerEntityMapper playerEntityMapper,
        Dictionary<ushort, CommandsBuffer> clientPlayerCommandsBuffer
    ) : base(world)
    {
        _playerEntityMapper = playerEntityMapper;
        _clientPlayerCommandsBuffer = clientPlayerCommandsBuffer;

        _filter = FilterBuilder
            .Include<PlayerCommandsComponent>()
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

            if (_clientPlayerCommandsBuffer.TryGetValue(clientId, out var playerCommandsBuffer) == false)
                continue;

            // Remove any old command frame commands
            // e.g. if we have commands for frames 3, 4, 5, 6 in the buffer and we are at frame 5 now, remove commands for frames 3 and 5
            playerCommandsBuffer.RemoveOldCommands(simulationState.CurrentServerCommandFrame);

            // Get the commands to apply this frame - will either be what the client sent or the last used commands if the buffer has run dry.
            var playerCommands = playerCommandsBuffer.Get(simulationState.CurrentServerCommandFrame);
            Set(entity, playerCommands);

            Logger.Debug($"{nameof(ApplyBufferedPlayerCommandsSystem)}: Applied buffered player commands for command frame :{simulationState.CurrentServerCommandFrame} - {playerCommands}");
        }
    }
}
