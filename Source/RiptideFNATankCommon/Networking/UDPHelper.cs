/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Wombat.Engine.Logging;

namespace RiptideFNATankCommon.Networking;

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
            Logger.Warning($"Received an old packet for CommandFrame: {justReceivedCommandFrame}. Already received a packet for CommandFrame: {lastReceivedCommandFrame}.");
            return false;
        }
        //HACK: Remove this when the server creates the state proper!
        else if (false && justReceivedCommandFrame == lastReceivedCommandFrame)
        {
            // Duplicate packet?
            Logger.Warning($"Received a duplicate packet for CommandFrame: {justReceivedCommandFrame}.");
            return false;
        }

        return true;
    }
}