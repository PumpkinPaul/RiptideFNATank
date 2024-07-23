// Copyright Pumpkin Games Ltd. All Rights Reserved.

namespace RiptideFNATank.Gameplay.Components;

public record struct SmoothingComponent
{
    public float Value;

    public SmoothingComponent(float value) => Value = value;
}