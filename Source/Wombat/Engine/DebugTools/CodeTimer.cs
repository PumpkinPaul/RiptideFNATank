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

namespace Wombat.Engine.DebugTools;

public readonly record struct CodeTimer : IDisposable
{
    private readonly string _name;

    public CodeTimer(string name, Color color)
    {
        _name = name;

        DebugSystem.TimeRuler.BeginMark(name, color);
    }

    public void Dispose()
    {
        DebugSystem.TimeRuler.EndMark(_name);
    }
}