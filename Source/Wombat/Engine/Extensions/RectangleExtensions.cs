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

namespace Wombat.Engine.Extensions;

/// <summary>
/// Extension methods for Int.
/// </summary>
public static class RectangleExtensions
{
    public static Vector2 GetIntersectionDepth(this Rectangle rectA, Rectangle rectB)
    {
        // Calculate half sizes.
        float halfWidthA = rectA.Width / 2.0f;
        float halfHeightA = rectA.Height / 2.0f;
        float halfWidthB = rectB.Width / 2.0f;
        float halfHeightB = rectB.Height / 2.0f;

        // Calculate centers.
        Vector2 centerA = new(rectA.Left + halfWidthA, rectA.Top + halfHeightA);
        Vector2 centerB = new(rectB.Left + halfWidthB, rectB.Top + halfHeightB);

        // Calculate current and minimum-non-intersecting distances between centers.
        float distanceX = centerA.X - centerB.X;
        float distanceY = centerA.Y - centerB.Y;
        float minDistanceX = halfWidthA + halfWidthB;
        float minDistanceY = halfHeightA + halfHeightB;

        // If we are not intersecting at all, return (0, 0).
        if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
            return Vector2.Zero;

        // Calculate and return intersection depths.
        float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
        float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
        return new Vector2(depthX, depthY);
    }
}
