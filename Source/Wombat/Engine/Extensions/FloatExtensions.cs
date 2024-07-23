/*
 _       __                __          __ 
| |     / /___  ____ ___  / /_  ____ _/ /_
| | /| / / __ \/ __ `__ \/ __ \/ __ `/ __/
| |/ |/ / /_/ / / / / / / /_/ / /_/ / /_  
|__/|__/\____/_/ /_/ /_/_.___/\__,_/\__/  

A simple 2D engine - use as the base engine for experiments and POCs.
                                          
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

namespace Wombat.Engine.Extensions;

/// <summary>
/// Extension methods for Float.
/// </summary>
public static class FloatExtensions
{
    public static bool FloatEquals(this float source, float value, float epsilon = 0.0001f)
    {
        return source + epsilon >= value && source - epsilon <= value;
    }
}
