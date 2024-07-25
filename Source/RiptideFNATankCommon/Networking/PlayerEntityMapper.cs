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

namespace RiptideFNATankCommon.Networking;

/// <summary>
/// Maps local and network players to the entities in the ECS.
/// </summary>
public class PlayerEntityMapper
{
    public const int INVALID_ENTITY = -1;

    readonly Dictionary<PlayerIndex, ushort> _playerIndexToClientId = [];
    readonly Dictionary<PlayerIndex, Entity> _playerIndexToEntity = [];
    readonly Dictionary<ushort, Entity> _clientIdToEntity = [];
    readonly Dictionary<ushort, PlayerIndex> _clientToPlayerIndex = [];

    public void AddPlayer(PlayerIndex playerIndex, ushort clientId)
    {
        _playerIndexToClientId[playerIndex] = clientId;
        _clientToPlayerIndex[clientId] = playerIndex;
    }

    public void AddPlayer(ushort clientId, Entity entity)
    {
        _clientIdToEntity[clientId] = entity;
    }

    public void RemovePlayerByClientId(ushort clientId)
    {
        var playerIndex = _clientToPlayerIndex[clientId];

        _clientIdToEntity.Remove(clientId);
        _clientToPlayerIndex.Remove(clientId);
        _playerIndexToClientId.Remove(playerIndex);
        _playerIndexToEntity.Remove(playerIndex);
    }

    public Entity GetEntityFromClientId(ushort clientId)
    {
        return _clientIdToEntity.TryGetValue(clientId, out var entity) ? entity : INVALID_ENTITY;
    }
}
