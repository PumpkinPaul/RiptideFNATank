/*
 _       __                __          __ 
| |     / /___  ____ ___  / /_  ____ _/ /_
| | /| / / __ \/ __ `__ \/ __ \/ __ `/ __/
| |/ |/ / /_/ / / / / / / /_/ / /_/ / /_  
|__/|__/\____/_/ /_/ /_/_.___/\__,_/\__/  

A simple 2D engine - use as the base engine for experiments and POCs.
                                          
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

namespace Wombat.Engine;

//Indicates the location of a collision in relation to the entity / actor / game object's bound box

//        TOP
//   -------------
// L |           | R
// E |           | I
// F |           | G
// T |           | H
//   ------------- T
//       Bottom

[Flags]
public enum CollisionEdge
{
    None = 0,
    Top = 1,
    Left = 2,
    Bottom = 4,
    Right = 8,
}