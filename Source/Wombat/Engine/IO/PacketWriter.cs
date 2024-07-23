/*
 _       __                __          __ 
| |     / /___  ____ ___  / /_  ____ _/ /_
| | /| / / __ \/ __ `__ \/ __ \/ __ `/ __/
| |/ |/ / /_/ / / / / / / /_/ / /_/ / /_  
|__/|__/\____/_/ /_/ /_/_.___/\__,_/\__/  

A simple 2D engine - use as the base engine for experiments and POCs.
                                          
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace Wombat.Engine.IO;

public class PacketWriter
{
    private readonly MemoryStream _outputStream;
    private readonly byte[] _buffer; // temp space for writing to.

    public PacketWriter(MemoryStream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        if (!stream.CanWrite)
            throw new ArgumentException("The stream is not writable");

        _outputStream = stream;
        _buffer = new byte[16];
    }

    //TODO: make this readonly memory or span!
    public byte[] GetBuffer() => _outputStream.GetBuffer();

    public void Reset() => _outputStream.Position = 0;

    // Writes a boolean to this stream. A single byte is written to the stream
    // with the value 0 representing false or the value 1 representing true.
    // 
    public virtual void Write(bool value)
    {
        _buffer[0] = (byte)(value ? 1 : 0);
        _outputStream.Write(_buffer, 0, 1);
    }

    // Writes a byte to this stream. The current position of the stream is
    // advanced by one.
    // 
    public virtual void Write(byte value)
    {
        _outputStream.WriteByte(value);
    }

    // Writes a signed byte to this stream. The current position of the stream 
    // is advanced by one.
    public virtual void Write(sbyte value)
    {
        _outputStream.WriteByte((byte)value);
    }

    public virtual void Write(short value)
    {
        _buffer[0] = (byte)value;
        _buffer[1] = (byte)(value >> 8);
        _outputStream.Write(_buffer, 0, 2);
    }

    // Writes a two-byte unsigned integer to this stream. The current position
    // of the stream is advanced by two.
    public virtual void Write(ushort value)
    {
        _buffer[0] = (byte)value;
        _buffer[1] = (byte)(value >> 8);
        _outputStream.Write(_buffer, 0, 2);
    }

    // Writes a four-byte signed integer to this stream. The current position
    // of the stream is advanced by four.
    public virtual void Write(int value)
    {
        _buffer[0] = (byte)value;
        _buffer[1] = (byte)(value >> 8);
        _buffer[2] = (byte)(value >> 16);
        _buffer[3] = (byte)(value >> 24);
        _outputStream.Write(_buffer, 0, 4);
    }

    // Writes a four-byte unsigned integer to this stream. The current position
    // of the stream is advanced by four.
    public virtual void Write(uint value)
    {
        _buffer[0] = (byte)value;
        _buffer[1] = (byte)(value >> 8);
        _buffer[2] = (byte)(value >> 16);
        _buffer[3] = (byte)(value >> 24);
        _outputStream.Write(_buffer, 0, 4);
    }

    // Writes an eight-byte signed integer to this stream. The current position
    // of the stream is advanced by eight.
    public virtual void Write(long value)
    {
        _buffer[0] = (byte)value;
        _buffer[1] = (byte)(value >> 8);
        _buffer[2] = (byte)(value >> 16);
        _buffer[3] = (byte)(value >> 24);
        _buffer[4] = (byte)(value >> 32);
        _buffer[5] = (byte)(value >> 40);
        _buffer[6] = (byte)(value >> 48);
        _buffer[7] = (byte)(value >> 56);
        _outputStream.Write(_buffer, 0, 8);
    }

    // Writes a float to this stream. The current position of the stream is
    // advanced by four.
    public unsafe virtual void Write(float value)
    {
        uint TmpValue = *(uint*)&value;
        _buffer[0] = (byte)TmpValue;
        _buffer[1] = (byte)(TmpValue >> 8);
        _buffer[2] = (byte)(TmpValue >> 16);
        _buffer[3] = (byte)(TmpValue >> 24);
        _outputStream.Write(_buffer, 0, 4);
    }

    public unsafe virtual void Write(Vector2 value)
    {
        Write(value.X);
        Write(value.Y);
    }
}
