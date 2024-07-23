/*
 _       __                __          __ 
| |     / /___  ____ ___  / /_  ____ _/ /_
| | /| / / __ \/ __ `__ \/ __ \/ __ `/ __/
| |/ |/ / /_/ / / / / / / /_/ / /_/ / /_  
|__/|__/\____/_/ /_/ /_/_.___/\__,_/\__/  

A simple 2D engine - use as the base engine for experiments and POCs.
                                          
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

namespace Wombat.Engine.Threading;

public class ConsoleMainThreadDispatcher
{
    static ConsoleMainThreadDispatcher _instance = null;
    static readonly Queue<Action> ExecutionQueue = new();

    public static void Update()
    {
        lock (ExecutionQueue)
        {
            while (ExecutionQueue.Count > 0)
                ExecutionQueue.Dequeue().Invoke();
        }
    }

    /// <summary>
    /// Locks the queue and adds the Action to it.
    /// </summary>
    /// <param name="action">function that will be executed from the main thread.</param>
    public static void Enqueue(Action action)
    {
        lock (ExecutionQueue)
        {
            ExecutionQueue.Enqueue(action);
        }
    }

    public static ConsoleMainThreadDispatcher Instance => _instance ??= new ConsoleMainThreadDispatcher();
}