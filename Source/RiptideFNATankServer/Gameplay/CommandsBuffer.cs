/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using RiptideFNATankCommon;
using RiptideFNATankCommon.Components;

namespace RiptideFNATankServer.Gameplay;

/// <summary>
/// Responsible for storing player actions (commands) from a single client.
/// </summary>
public class CommandsBuffer()
{
    /// <summary>
    /// Comparer for player actions in command buffer.
    /// <para>Sorts by command frame, lower commands will be process first.</para>
    /// </summary>
    class CommandsBufferComparer : IComparer<(uint CommandFrame, PlayerActionsComponent Actions)>
    {
        public int Compare((uint CommandFrame, PlayerActionsComponent Actions) x, (uint CommandFrame, PlayerActionsComponent Actions) y)
        {
            return x.CommandFrame.CompareTo(y.CommandFrame);
        }
    }

    /// <summary>
    /// A unique sorted set of all the command frames received for a player.
    /// </summary>
    readonly SortedSet<(uint CommandFrame, PlayerActionsComponent Commands)> _playerActions = new(new CommandsBufferComparer());

    /// <summary>
    /// Gets the number of commands in the buffer.
    /// </summary>
    public int Count => _playerActions.Count;

    /// <summary>
    /// The player's previous commands will be returned if there are no commands for the current command frame in the buffer.
    /// <para>
    /// Why? It's very likely that the player's input will not change that often (relatively speaking at 60fps)
    /// e.g. if they are moving forward on frame 34 they are probably still moving forward on frame 35.
    /// </para>
    /// </summary>
    PlayerActionsComponent _previousPlayerActions;

    public bool Add(uint effectiveCommandFrame, PlayerActionsComponent playerActions)
    {
        return _playerActions.Add((effectiveCommandFrame, playerActions));
    }

    public PlayerActionsComponent Remove(uint currentCommandFrame)
    {
        // Remove any old command frames
        _playerActions.RemoveWhere(p => p.CommandFrame < currentCommandFrame);

        if (_playerActions.Count == 0)
        {
            Logger.Warning("CommandBuffer is empty");
            return _previousPlayerActions;
        }

        var min = _playerActions.Min;
        var (EffectiveCommandFrame, Commands) = min;

        // Return the previous command if we don't have a command for the current command frame.
        // i.e. We have commands but they are for future frames
        if (EffectiveCommandFrame > currentCommandFrame)
        {
            Logger.Warning("CommandBuffer only has future commands");
            return _previousPlayerActions;
        }

        // Cache the command to return so that it can be returned again for situations when the CommandBuffer doesn't contain enough commands.
        _previousPlayerActions = Commands;
        _playerActions.Remove(min);
        return Commands;
    }
}

