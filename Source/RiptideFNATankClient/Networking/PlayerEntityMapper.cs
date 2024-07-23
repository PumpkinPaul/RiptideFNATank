// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using System.Collections.Generic;

namespace RiptideFNATank.RiptideMultiplayer;

/// <summary>
/// Maps local and network players to the entities in the ECS.
/// </summary>
public class PlayerEntityMapper
{
    public const int INVALID_ENTITY = -1;

    readonly Dictionary<PlayerIndex, string> _playerIndexToSessionId = new();
    readonly Dictionary<PlayerIndex, Entity> _playerIndexToEntity = new();
    readonly Dictionary<string, Entity> _sessionIdToEntity = new();
    readonly Dictionary<string, PlayerIndex> _sessionIdToPlayerIndex = new();

    public void AddPlayer(PlayerIndex playerIndex, string sessionId)
    {
        _playerIndexToSessionId[playerIndex] = sessionId;
        _sessionIdToPlayerIndex[sessionId] = playerIndex;
    }

    public void MapEntity(PlayerIndex playerIndex, Entity entity)
    {
        _playerIndexToEntity[playerIndex] = entity;

        var sessionId = _playerIndexToSessionId[playerIndex];
        _sessionIdToEntity[sessionId] = entity;
    }

    public void RemovePlayerBySessionId(string sessionId)
    {
        var playerIndex = _sessionIdToPlayerIndex[sessionId];

        _sessionIdToEntity.Remove(sessionId);
        _sessionIdToPlayerIndex.Remove(sessionId);
        _playerIndexToSessionId.Remove(playerIndex);
        _playerIndexToEntity.Remove(playerIndex);
    }

    public Entity GetEntityFromSessionId(string sessionId)
    {
        return _sessionIdToEntity.TryGetValue(sessionId, out var entity) ? entity : INVALID_ENTITY;
    }
}
