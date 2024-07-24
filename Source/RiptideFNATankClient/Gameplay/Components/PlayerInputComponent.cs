// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RiptideFNATankClient.Gameplay.Components;

public readonly record struct PlayerInputComponent(
    PlayerIndex PlayerIndex,
    Keys MoveUpKey,
    Keys MoveDownKey
);