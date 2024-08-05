/*
 _       __                __          __ 
| |     / /___  ____ ___  / /_  ____ _/ /_
| | /| / / __ \/ __ `__ \/ __ \/ __ `/ __/
| |/ |/ / /_/ / / / / / / /_/ / /_/ / /_  
|__/|__/\____/_/ /_/ /_/_.___/\__,_/\__/  

A simple 2D engine - use as the base engine for experiments and POCs.
                                          
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

namespace Wombat.Engine.Collections;

/// <summary>
/// A maximum comparer.
/// <para>
/// E.g. Use in a PriorityQueue to order by hightest value (instead of the default which is the lowest value).
/// </para>
/// </summary>
public class UIntMaxComparer : IComparer<uint>
{
    public int Compare(uint x, uint y) => y.CompareTo(x);
}