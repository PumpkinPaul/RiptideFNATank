/*
 _       __                __          __ 
| |     / /___  ____ ___  / /_  ____ _/ /_
| | /| / / __ \/ __ `__ \/ __ \/ __ `/ __/
| |/ |/ / /_/ / / / / / / /_/ / /_/ / /_  
|__/|__/\____/_/ /_/ /_/_.___/\__,_/\__/  

A simple 2D engine - use as the base engine for experiments and POCs.
                                          
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using System.Collections.Concurrent;

namespace Wombat.Engine.Threading;

public sealed class ConsoleSynchronizationContext : SynchronizationContext
{
    private static readonly ConcurrentQueue<Message> Queue;

    static ConsoleSynchronizationContext() => Queue = new ConcurrentQueue<Message>();

    private static void Enqueue(SendOrPostCallback callback, object state) => Queue.Enqueue(new Message(callback, state));

    public static void Update()
    {
        if (Queue.IsEmpty)
            return;

        if (!Queue.TryDequeue(out Message message))
            return;

        message.Callback(message.State);
    }

    public override SynchronizationContext CreateCopy() => new ConsoleSynchronizationContext();

    public override void Post(SendOrPostCallback d, object state) => Enqueue(d, state);

    public override void Send(SendOrPostCallback d, object state) => Enqueue(d, state);

    private sealed class Message
    {
        public Message(SendOrPostCallback callback, object state)
        {
            Callback = callback;
            State = state;
        }

        public SendOrPostCallback Callback { get; }

        public object State { get; }
    }
}