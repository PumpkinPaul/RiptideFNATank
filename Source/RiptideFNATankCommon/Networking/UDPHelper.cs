/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Microsoft.Extensions.Logging;
using Wombat.Engine.Logging;

namespace RiptideFNATankCommon.Networking;

static partial class Log
{
    [LoggerMessage(Message = "Received an old packet for command frame: {justReceivedCommandFrame}. Already received a packet for CommandFrame: {lastReceivedCommandFrame}")]
    public static partial void ReceivedOldPacket(this ILogger logger, LogLevel logLevel, uint justReceivedCommandFrame, uint lastReceivedCommandFrame);

    [LoggerMessage(Message = "Received a duplicate packet for command frame: {commandFrame}.")]
    public static partial void ReceivedDuplicatePacket(this ILogger logger, LogLevel logLevel, uint commandFrame);
}

/// <summary>
/// Helper functions for UDP stuff.
/// </summary>
public static class UDPHelper
{
    /// <summary>
    /// Checks to see if the packet just received is valid.
    /// <para>
    /// Due to the way UDP works, it could be old, duplicated, etc
    /// </para>
    /// </summary>
    /// <param name="justReceivedCommandFrame">The CommandFrame this packet is for.</param>
    /// <param name="lastReceivedCommandFrame">The CommandFrame of the most recent packet received by us.</param>
    /// <returns></returns>
    public static bool IsValidPacket(uint justReceivedCommandFrame, uint lastReceivedCommandFrame)
    {
        if (justReceivedCommandFrame < lastReceivedCommandFrame)
        {
            // Discard packet
            Logger.Log.ReceivedOldPacket(LogLevel.Trace, justReceivedCommandFrame, lastReceivedCommandFrame);
            return false;
        }
        //HACK: Remove this when the server creates the state proper!
        else if (false && justReceivedCommandFrame == lastReceivedCommandFrame)
        {
            // Duplicate packet?
            Logger.Log.ReceivedDuplicatePacket(LogLevel.Trace, justReceivedCommandFrame);
            return false;
        }

        return true;
    }
}