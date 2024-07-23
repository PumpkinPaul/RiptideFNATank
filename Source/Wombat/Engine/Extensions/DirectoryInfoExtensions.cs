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
/// Extension methods for Enums.
/// </summary>
public static class DirectoryInfoExtensions
{
    /// <summary>
    /// Gets the parent of a directory a number of levels 'up'.
    /// </summary>
    /// <returns>The directoty info object.</returns>
    public static DirectoryInfo GetParent(this DirectoryInfo directoryInfo, uint levels)
    {
        if (directoryInfo == null) throw
            new ArgumentNullException(nameof(directoryInfo));

        uint count = 0;
        var di = directoryInfo;

        while (count < levels && di != null)
        {
            di = di.Parent;

            count++;
        }

        return di;
    }
}
