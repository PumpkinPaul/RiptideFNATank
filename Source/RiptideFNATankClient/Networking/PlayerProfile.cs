// Copyright Pumpkin Games Ltd. All Rights Reserved.

using Newtonsoft.Json;
using Wombat.Engine.IO;
using System;
using System.IO;

namespace RiptideFNATank.RiptideMultiplayer;

/// <summary>
/// Basic info about a player.
/// </summary>
/// <remarks>
/// This info will be read / write to file system.
/// </remarks>
public class PlayerProfile
{
    public string DeviceIdentifier { get; set; }
    
    public string SessionToken { get; set; }

    [JsonRequired]
    public string FullPath { get; private set; }

    public static PlayerProfile LoadOrCreate(string path)
    {
        PlayerProfile playerProfile = null;

        var fullPath = Path.Combine(path, "playerProfile.json");

        try
        {
            if (File.Exists(fullPath))
            {
                var json = File.ReadAllText(fullPath);
                return JsonConvert.DeserializeObject<PlayerProfile>(json);
            }
        }
        catch(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }

        return playerProfile ?? new PlayerProfile { FullPath = fullPath };
    }

    public void Save()
    {
        try
        {
            FileManager.EnsureFileDirectory(FullPath);

            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(FullPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
