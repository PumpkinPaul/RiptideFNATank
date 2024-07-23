// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;

namespace RiptideFNATank.Gameplay.Components;

public record struct LerpPositionComponent
{
    public Vector2 ToPosition;
    public Vector2 FromPosition;
    public float Timer;
}