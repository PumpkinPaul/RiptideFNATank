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

namespace Wombat.Engine.IO;

public class PacketReader
{
    Stream _stream;

    readonly byte[] _buffer = new byte[16];

    public PacketReader()
    {
    }

    public void SetState(byte[] state)
    {
        //TODO: fix the allocation here
        _stream = new MemoryStream(state);
    }

    //
    // Summary:
    //     Reads a Boolean value from the current stream and advances the current position
    //     of the stream by one byte.
    //
    // Returns:
    //     true if the byte is nonzero; otherwise, false.
    public virtual bool ReadBoolean()
    {
        FillBuffer(1);
        return _buffer[0] != 0;
    }

    //
    // Summary:
    //     Reads the next byte from the current stream and advances the current position
    //     of the stream by one byte.
    //
    // Returns:
    //     The next byte read from the current stream.
    public virtual byte ReadByte()
    {
        if (_stream == null)
            throw new IOException("Stream is not open");

        int num = _stream.ReadByte();
        if (num == -1)
            throw new IOException("End of file");

        return (byte)num;
    }

    //
    // Summary:
    //     Reads a signed byte from this stream and advances the current position of the
    //     stream by one byte.
    //
    // Returns:
    //     A signed byte read from the current stream.
    public virtual sbyte ReadSByte()
    {
        FillBuffer(1);
        return (sbyte)_buffer[0];
    }

    //
    // Summary:
    //     Reads a 2-byte signed integer from the current stream and advances the current
    //     position of the stream by two bytes.
    //
    // Returns:
    //     A 2-byte signed integer read from the current stream.
    public virtual short ReadInt16()
    {
        FillBuffer(2);
        return (short)(_buffer[0] | _buffer[1] << 8);
    }

    //
    // Summary:
    //     Reads a 2-byte unsigned integer from the current stream using little-endian encoding
    //     and advances the position of the stream by two bytes.
    //
    // Returns:
    //     A 2-byte unsigned integer read from this stream.
    public virtual ushort ReadUInt16()
    {
        FillBuffer(2);
        return (ushort)(_buffer[0] | _buffer[1] << 8);
    }

    //
    // Summary:
    //     Reads a 4-byte signed integer from the current stream and advances the current
    //     position of the stream by four bytes.
    //
    // Returns:
    //     A 4-byte signed integer read from the current stream.
    public virtual int ReadInt32()
    {
        FillBuffer(4);
        return _buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24;
    }

    public virtual long ReadInt64()
    {
        FillBuffer(8);
        uint num = (uint)(_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
        uint num2 = (uint)(_buffer[4] | _buffer[5] << 8 | _buffer[6] << 16 | _buffer[7] << 24);
        return (long)((ulong)num2 << 32 | num);
    }

    //
    // Summary:
    //     Reads a 4-byte unsigned integer from the current stream and advances the position
    //     of the stream by four bytes.
    //
    // Returns:
    //     A 4-byte unsigned integer read from this stream.
    public virtual uint ReadUInt32()
    {
        FillBuffer(4);
        return (uint)(_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
    }

    //
    // Summary:
    //     Reads a 4-byte floating point value from the current stream and advances the
    //     current position of the stream by four bytes.
    //
    // Returns:
    //     A 4-byte floating point value read from the current stream.
    public unsafe virtual float ReadSingle()
    {
        FillBuffer(4);
        uint num = (uint)(_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
        return *(float*)&num;
    }

    //
    // Summary:
    //     Fills the internal buffer with the specified number of bytes read from the stream.
    //
    // Parameters:
    //   numBytes:
    //     The number of bytes to be read.
    protected virtual void FillBuffer(int numBytes)
    {
        if (_buffer != null && (numBytes < 0 || numBytes > _buffer.Length))
        {
            throw new ArgumentOutOfRangeException(nameof(numBytes), "FillBuffer");
        }

        int num = 0;
        int num2;
        if (numBytes == 1)
        {
            num2 = _stream.ReadByte();
            if (num2 == -1)
                throw new IOException("End of file");

            _buffer[0] = (byte)num2;
            return;
        }

        do
        {
            num2 = _stream.Read(_buffer, num, numBytes - num);
            if (num2 == 0)
                throw new IOException("End of file");

            num += num2;
        }
        while (num < numBytes);
    }

    public unsafe virtual Vector2 ReadVector2()
    {
        return new Vector2(ReadSingle(), ReadSingle());
    }
}
