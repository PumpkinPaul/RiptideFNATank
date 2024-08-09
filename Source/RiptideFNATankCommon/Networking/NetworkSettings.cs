/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

namespace RiptideFNATankCommon.Networking;


public static class NetworkSettings
{
    public const float SIMULATION_NORMAL_FPS = 60.0f;
    public const float SIMULATION_CATCH_UP_FPS = 66.0f;
    public const float SIMULATION_SLOW_DOWN_FPS = 56.0f;

    public static TimeSpan PhsyicsTimeSpan { get; } = TimeSpan.FromSeconds(1.0f / SIMULATION_NORMAL_FPS);
    public static TimeSpan SpeedUpTimeSpan { get; } = TimeSpan.FromSeconds(1.0f / SIMULATION_CATCH_UP_FPS);
    public static TimeSpan SlowDownTimeSpan { get; } = TimeSpan.FromSeconds(1.0f / SIMULATION_SLOW_DOWN_FPS);

    public const int BUFFER_SIZE = 1024;

    public const ushort PORT = 17871;
    public const ushort MAX_PLAYERS = 4;

    public const byte COMMAND_BUFFER_SIZE = 2;
}
