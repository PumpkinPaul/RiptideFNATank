/*
 _       __                __          __ 
| |     / /___  ____ ___  / /_  ____ _/ /_
| | /| / / __ \/ __ `__ \/ __ \/ __ `/ __/
| |/ |/ / /_/ / / / / / / /_/ / /_/ / /_  
|__/|__/\____/_/ /_/ /_/_.___/\__,_/\__/  

A simple 2D engine - use as the base engine for experiments and POCs.
                                          
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Microsoft.Xna.Framework;

namespace Wombat.Engine.Geometry;

public readonly record struct AABB2
{
    public readonly Vector2 Centre;
    public readonly Vector2 Extents;

    public AABB2(Vector2 centre, Vector2 extents)
    {
        Centre = centre;
        Extents = extents;
    }

    //returns true if this is overlapping b
    public bool Overlaps(in AABB2 b)
    {
        var T = b.Centre - Centre; //Vector from A to B
        return Math.Abs(T.X) <= Extents.X + b.Extents.X && Math.Abs(T.Y) <= Extents.Y + b.Extents.Y;
    }

    public Vector2 Max => Centre + Extents;

    public float MinX => Centre.X - Extents.X;
    public float MinY => Centre.Y - Extents.Y;
    public float MaxX => Centre.X + Extents.X;
    public float MaxY => Centre.Y + Extents.Y;
}