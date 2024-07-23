/*
 _       __                __          __ 
| |     / /___  ____ ___  / /_  ____ _/ /_
| | /| / / __ \/ __ `__ \/ __ \/ __ `/ __/
| |/ |/ / /_/ / / / / / / /_/ / /_/ / /_  
|__/|__/\____/_/ /_/ /_/_.___/\__,_/\__/  

A simple 2D engine - use as the base engine for experiments and POCs.
                                          
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Newtonsoft.Json;

namespace Wombat.Engine.DebugTools;

public class DebugSettings
{
    [JsonIgnore]
    public static string FullPath => Path.Combine(BaseGame.LocalApplicationDataPath, "DebugSettings.json");

    public static DebugSettings Create()
    {
        DebugSettings settings = null;
        try
        {
            if (File.Exists(FullPath))
            {
                var json = File.ReadAllText(FullPath);
                return JsonConvert.DeserializeObject<DebugSettings>(json);
            }
        }
        catch
        {

        }

        return settings ?? new DebugSettings();

    }

    public bool ShowEntityPositions;
    public bool ShowEntityCollisionBounds;
    public bool ShowEntityCollisionRadius;

    public bool ShowGcCounter; 
    public bool ShowFpsCounter;
    public bool ShowTimeRuler;
    public bool ShowPlots;

    public void Save()
    {
        try
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(FullPath, json);
        }
        catch
        {
        }
    }
}