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
using RiptideFNATankCommon.Components;
using Wombat.Engine;
using Wombat.Engine.Collisions;

namespace RiptideFNATankCommon.Systems;

/// <summary>
/// Responsible for performing entity to entity collision calculations.
/// </summary>
public sealed class EntityCollisionSystem : MoonTools.ECS.System
{
    readonly int _screenWidth;

    readonly HashSet<(Entity, Entity)> _potentialPairs = new();
    readonly List<(Entity, Entity, float Time)> _actualColliders = new();

    readonly Filter _filter;

    public EntityCollisionSystem(
        World world,
        int screenWidth) : base(world)
    {
        _screenWidth = screenWidth;

        _filter = FilterBuilder
            .Include<PositionComponent>()
            .Include<ScaleComponent>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        TestCollisions();
        ProcessCollisions();
    }

    private void TestCollisions()
    {
        for (var i = 0; i < _filter.Count; i++)
        {
            for (var j = i + 1; j < _filter.Count; j++)
            {
                var entity1 = _filter.NthEntity(i);
                var entity2 = _filter.NthEntity(j);

                var entityPair = (entity1, entity2);

                if (_potentialPairs.Contains(entityPair))
                    continue; //Already tested this pair of entities so we can skip

                //Flag this pair as being tested
                _potentialPairs.Add(entityPair);

                SweepTestAABBs(entity1, entity2);
            }
        }

        _potentialPairs.Clear();
    }

    void SweepTestAABBs(Entity entity1, Entity entity2)
    {
        GetEntityPhyscis(entity1, out Vector2 velocity1, out BoxF bounds1, out Vector2 centreOfGravity1);
        GetEntityPhyscis(entity2, out Vector2 velocity2, out BoxF bounds2, out Vector2 centreOfGravity2);

        if (SweptAabbAabbCollision.Collide(new Vector2(bounds1.Width, bounds1.Height) * 0.5f, centreOfGravity1, velocity1, new Vector2(bounds2.Width, bounds2.Height) * 0.5f, centreOfGravity2, velocity2, out float time, out float time2))
            _actualColliders.Add((entity1, entity2, time));
    }

    void GetEntityPhyscis(Entity entity, out Vector2 velocity, out BoxF bounds, out Vector2 centreOfGravity)
    {
        ref readonly var position = ref Get<PositionComponent>(entity);
        ref readonly var scale = ref Get<ScaleComponent>(entity);
        velocity = Has<VelocityComponent>(entity) ? Get<VelocityComponent>(entity).Value : Vector2.Zero;

        //...derive a bounding box in world space for the entity
        var halfSize1 = scale.Value / 2;
        bounds = new BoxF(-halfSize1, scale.Value);
        var worldBounds = bounds.Translated(position.Value);

        centreOfGravity = new Vector2(worldBounds.Left + (worldBounds.Right - worldBounds.Left) / 2.0f, worldBounds.Bottom + (worldBounds.Top - worldBounds.Bottom) / 2.0f);
    }

    void ProcessCollisions()
    {
        var centreX = _screenWidth / 2;

        for (var i = 0; i < _actualColliders.Count; i++)
        {
            var (entity1, entity2, collisionTime) = _actualColliders[i];
        }

        _actualColliders.Clear();
    }
}
