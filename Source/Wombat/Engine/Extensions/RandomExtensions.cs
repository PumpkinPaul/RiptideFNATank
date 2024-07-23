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

public static class RandomExtensions
{
    public static float NextFloat(this Random random)
    {
        return (float)random.NextDouble();
    }

    public static float NextFloat(this Random random, float minimum, float maximum)
    {
        return (float)random.NextDouble() * (maximum - minimum) + minimum;
    }
}
