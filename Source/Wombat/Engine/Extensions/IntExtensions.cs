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
/// Extension methods for Int.
/// </summary>
public static class IntExtensions
{
    internal static int Mod(this int a, int n)
    {
        if (n == 0)
            throw new ArgumentOutOfRangeException(nameof(n), "(a mod 0) is undefined.");

        //puts a in the [-n+1, n-1] range using the remainder operator
        var remainder = a % n;

        //if the remainder is less than zero, add n to put it in the [0, n-1] range if n is positive
        //if the remainder is greater than zero, add n to put it in the [n-1, 0] range if n is negative
        if (n > 0 && remainder < 0 || n < 0 && remainder > 0)
            return remainder + n;

        return remainder;
    }
}
