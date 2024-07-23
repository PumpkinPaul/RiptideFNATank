// Copyright Pumpkin Games Ltd. All Rights Reserved.

namespace RiptideFNATank.Gameplay.Components;

public readonly record struct CausesBounceComponent(
    //A 'mask' that will determince if an entity can cause bouncing
    //If the sign of this mask matches the sign of the velocity then a bounce can occur
    ////This stops slightly embedded objects causing a juddering
    int BounceDirection
);