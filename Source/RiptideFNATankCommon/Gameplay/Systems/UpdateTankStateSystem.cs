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
using RiptideFNATankCommon.Gameplay.Components;

namespace RiptideFNATankCommon.Gameplay.Systems;

/// <summary>
/// Reads remote match data received messages and applies the new values to the 'simulation state' - e.g. the normal component data
/// </summary>
public abstract class UpdateTankStateSystem : MoonTools.ECS.System
{
    public UpdateTankStateSystem(World world) : base(world)
    {
    }

    protected void UpdateState(
        Entity entity,
        ref TankState paddleState)
    {
        ref readonly var position = ref Get<PositionComponent>(entity);

        var moveUpSpeed = paddleState.MoveUp ? ProcessPlayerCommandsSystem.PADDLE_SPEED : 0;
        var moveDownSpeed = paddleState.MoveDown ? -ProcessPlayerCommandsSystem.PADDLE_SPEED : 0;

        paddleState.Velocity = new Vector2(0, moveUpSpeed + moveDownSpeed);

        paddleState.Position += paddleState.Velocity;
        //paddleState.Velocity *= PlayerActionsSystem.PADDLE_FRICTION;
    }
}
