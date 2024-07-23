/*
 _       __                __          __ 
| |     / /___  ____ ___  / /_  ____ _/ /_
| | /| / / __ \/ __ `__ \/ __ \/ __ `/ __/
| |/ |/ / /_/ / / / / / / /_/ / /_/ / /_  
|__/|__/\____/_/ /_/ /_/_.___/\__,_/\__/  

A simple 2D engine - use as the base engine for experiments and POCs.
                                          
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using System.ComponentModel;

namespace Wombat.Engine.Extensions;

/// <summary>
/// Extension methods for Enums.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the description for an enum.
    /// </summary>
    /// <param name="type">The type of enum.</param>
    /// <returns>The value of the Description attribute or that value as a string.</returns>
    public static string Description(this Enum type)
    {
        var attributes = type.GetType().GetField(type.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? ((DescriptionAttribute)attributes[0]).Description : type.ToString();
    }
}
