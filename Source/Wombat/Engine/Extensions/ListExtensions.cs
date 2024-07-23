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
/// Extension methods for List<T>.
/// </summary>
public static class ListExtensions
{
    public static void Shuffle<T>(this List<T> source)
    {
        for (int n = source.Count - 1; n > 0; --n)
        {
            var k = RandomHelper.FastRandom.Next(n + 1);
            var temp = source[n];
            source[n] = source[k];
            source[k] = temp;
        }
    }
}
