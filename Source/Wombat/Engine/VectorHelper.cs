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
using System;

namespace Wombat.Engine;

/// <summary>
/// Helper class for Vectors
/// </summary>
public static class VectorHelper
{
    public static Vector2 Polar(float radians, float length)
    {
        return new Vector2(length * (float)Math.Cos(radians), length * (float)Math.Sin(radians));
    }

    public static void Truncate(ref Vector2 source, float maxLength)
    {
        var length = source.Length();
        if (length > maxLength)
        {
            source.Normalize();
            source *= maxLength;
        }
    }

    public static Vector2 RotateAboutOrigin(Vector2 point, Vector2 origin, float rotation)
    {
        return Vector2.Transform(point - origin, Matrix.CreateRotationZ(rotation)) + origin;
    }

    public static Vector2 RotateAboutOrigin(Vector2 point, Vector2 origin, ref Matrix rotation)
    {
        return Vector2.Transform(point - origin, rotation) + origin;
    }

    public static float GetAngle(Vector2 a, Vector2 b)
    {
        return (float)Math.Atan2(b.Y - a.Y, b.X - a.X);
    }

    public static float GetAngle(Vector2 a)
    {
        return (float)Math.Atan2(a.Y, a.X);
    }

    public static float FixRadianAngle(float angle)
    {
        if (angle < 0)
            angle += MathHelper.TwoPi;
        else if (angle > MathHelper.TwoPi)
            angle -= MathHelper.TwoPi;

        return angle;
    }
}