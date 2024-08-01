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
/// A simple circular buffer.
/// <para>
/// Attempts to set values past the end of the array will mod the index so it loops back round to the start of the array.
/// </para>
/// </summary>
/// <example>
/// Array with 5 elements
///  0  1  2  3  4 
/// [ ][ ][ ][ ][ ]
///                  
/// Update slot at 'index' 6 to x
///  0  1  2  3  4 
/// [ ][x][ ][ ][ ]
/// </example>
/// <typeparam name="T">Type of items stored in the buffer. Can be classes, records, structs.</typeparam>
public class CircularBuffer<T>
{
    readonly uint _bufferSize;
    readonly T[] _buffer;

    public CircularBuffer(uint bufferSize)
    {
        _bufferSize = bufferSize;
        _buffer = new T[_bufferSize];
    }

    public T Get(uint index)
    {
        return _buffer[index % _bufferSize];
    }

    public uint Set(uint index, T item)
    {
        var idx = index % _bufferSize;
        _buffer[idx] = item;
        return idx;
    }

    public void Clear()
    {
        Array.Clear(_buffer);
    }
}