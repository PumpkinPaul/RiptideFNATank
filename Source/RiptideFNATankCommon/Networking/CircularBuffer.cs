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

public class CircularBuffer<T>
{
    readonly int _bufferSize;
    T[] _buffer;

    public CircularBuffer(int bufferSize)
    {
        _bufferSize = bufferSize;
        _buffer = new T[_bufferSize];
    }

    public void Add(T item, int index)
    {
        _buffer[index % _bufferSize] = item;
    }

    public T Get(int index)
    {
        return _buffer[index % _bufferSize];
    }

    public void Clear()
    {
        //TODO: garbage
        _buffer = new T[_bufferSize];
    }
}