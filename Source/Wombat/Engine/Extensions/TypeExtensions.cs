/*
 _       __                __          __ 
| |     / /___  ____ ___  / /_  ____ _/ /_
| | /| / / __ \/ __ `__ \/ __ \/ __ `/ __/
| |/ |/ / /_/ / / / / / / /_/ / /_/ / /_  
|__/|__/\____/_/ /_/ /_/_.___/\__,_/\__/  

A simple 2D engine - use as the base engine for experiments and POCs.
                                          
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using System.Reflection;

namespace Wombat.Engine.Extensions;

public static class TypeExtensions
{
    public static IEnumerable<FieldInfo> GetAllFields(this Type t)
    {
        if (t == null)
        {
            return [];
        }

        const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic |
                                   BindingFlags.Static | BindingFlags.Instance |
                                   BindingFlags.DeclaredOnly;

        return t.GetFields(FLAGS).Concat(t.BaseType.GetAllFields());
    }

    public static IEnumerable<PropertyInfo> GetAllProperties(this Type t)
    {
        if (t == null)
        {
            return [];
        }

        const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic |
                                   BindingFlags.Static | BindingFlags.Instance |
                                   BindingFlags.DeclaredOnly;

        return t.GetProperties(FLAGS).Concat(t.BaseType.GetAllProperties());
    }

    public static bool Implements<T>(this Type source) where T : class
    {
        return typeof(T).IsAssignableFrom(source) && source.IsClass;
    }
}