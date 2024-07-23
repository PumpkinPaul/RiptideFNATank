// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using MoonTools.ECS;
using RiptideFNATank.Gameplay.Components;

namespace RiptideFNATank.Gameplay.Systems;

/// <summary>
/// Reads remote match data received messages and applies the new values to the 'simulation state' - e.g. the normal component data
/// </summary>
public abstract class UpdatePaddleStateSystem : MoonTools.ECS.System
{
    public UpdatePaddleStateSystem(World world) : base(world)
    {
    }

    protected void UpdateState(
        Entity entity,
        ref PaddleState paddleState)
    {
        ref readonly var position = ref Get<PositionComponent>(entity);

        var moveUpSpeed   = paddleState.MoveUp   ?  PlayerActionsSystem.PADDLE_SPEED : 0;
        var moveDownSpeed = paddleState.MoveDown ? -PlayerActionsSystem.PADDLE_SPEED : 0;

        paddleState.Velocity = new Vector2(0, moveUpSpeed + moveDownSpeed);
        
        paddleState.Position += paddleState.Velocity;
        //paddleState.Velocity *= PlayerActionsSystem.PADDLE_FRICTION;
    }
}
