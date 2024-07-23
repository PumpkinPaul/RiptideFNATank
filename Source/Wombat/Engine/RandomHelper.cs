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

namespace Wombat.Engine;

/// <summary>
/// Helper class to generate random numbers.
/// </summary>
public static class RandomHelper
{
    public static float RandomAngularVariance(float varianceInDegrees)
    {
        var radians = MathHelper.ToRadians(varianceInDegrees);
        return FastRandom.NextFloat(radians) - radians * 0.5f;
    }

    public static FastRandom DeterministicRandom { get; set; } = new FastRandom();
    public static FastRandom FastRandom { get; set; } = new FastRandom();
}
