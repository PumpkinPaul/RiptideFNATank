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

namespace RiptideFNATankServer.Gameplay.Systems;

public readonly record struct ClientStateReceivedMessage(
    Entity Entity,
    uint SequenceId,
    Vector2 Position,
    bool MoveUp,
    bool MoveDown
);

/// <summary>
/// 
/// </summary>
public sealed class ClientStateReceivedSystem : MoonTools.ECS.System
{
    public ClientStateReceivedSystem(World world) : base(world)
    {
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var message in ReadMessages<ClientStateReceivedMessage>())
        {
            //ref var position = ref GetMutable<PositionComponent>(message.Entity);
            //position.Value = message.Position;

            ref var playerActions = ref GetMutable<PlayerActionsComponent>(message.Entity);
            Set(message.Entity, new PlayerActionsComponent(message.MoveUp, message.MoveDown));            
        }
    }
}
