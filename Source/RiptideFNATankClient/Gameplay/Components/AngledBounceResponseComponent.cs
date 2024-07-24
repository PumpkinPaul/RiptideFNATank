// Copyright Pumpkin Games Ltd. All Rights Reserved.

using MoonTools.ECS;

namespace RiptideFNATankClient.Gameplay.Components;

public readonly record struct AngledBounceResponseComponent(
    Entity BouncedBy
);