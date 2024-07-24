/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Microsoft.Xna.Framework;
using Riptide;

namespace RiptideFNATankCommon.Extensions;

/// <summary>
/// Extension methods for the Riptide Message class.
/// </summary>
public static class RiptideMessageExtensions
{
    public static void AddVector2(this Message message, Vector2 value)
    {
        message.AddFloat(value.X);
        message.AddFloat(value.Y);
    }

    public static Vector2 GetVector2(this Message message)
    {
        return new Vector2(
            message.GetFloat(),
            message.GetFloat());
    }
}
