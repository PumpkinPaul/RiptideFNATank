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

/// <summary>
/// A client / server synchronished timer.
/// <para>
/// The client will run faster than the server (e.g.) 60 FPS while the server runs slower at (e.g.) 20 FPS.
/// </para>
/// <para>
/// This will allow us know whch client ticks relate to whcih server ticks and vice versa
/// </para>
/// </summary>
/// <param name="_serverTickRateInFPS"></param>
public class NetworkTimer(float serverTickRateInFPS)
{
    float _timer;
    public int CurrentTick { get; set; }

    public float MinTimeBetweenTicksInSeconds { get; } = 1.0f / serverTickRateInFPS;

    public void Update(float delta)
    {
        _timer += delta;
    }

    public bool ShouldTick()
    {
        if (_timer < MinTimeBetweenTicksInSeconds)
            return false;

        _timer -= MinTimeBetweenTicksInSeconds;
        CurrentTick++;
        return true;
    }
}