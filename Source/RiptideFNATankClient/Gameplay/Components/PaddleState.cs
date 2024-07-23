// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;

namespace RiptideFNATank.Gameplay.Components;

public record struct PaddleState
{
    public Vector2 Position;
    public Vector2 Velocity;
    public bool MoveUp;
    public bool MoveDown;
}