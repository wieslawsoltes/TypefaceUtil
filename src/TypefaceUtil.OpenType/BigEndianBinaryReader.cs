using System;
using System.IO;

namespace TypefaceUtil.OpenType;

internal class BigEndianBinaryReader : BinaryReader
{
    private readonly byte[] _buffer = new byte[8];

    public BigEndianBinaryReader(Stream input)
        : base(input)
    {
    }

    private byte[] ReadBigEndian(int count)
    {
        Read(_buffer, 0, count);
        Array.Reverse(_buffer);
        return _buffer;
    }

    public override short ReadInt16() => BitConverter.ToInt16(ReadBigEndian(2), 8 - 2);

    public override ushort ReadUInt16() => BitConverter.ToUInt16(ReadBigEndian(2), 8 - 2);

    public override uint ReadUInt32() => BitConverter.ToUInt32(ReadBigEndian(4), 8 - 4);

    public override ulong ReadUInt64() => BitConverter.ToUInt64(ReadBigEndian(8), 8 - 8);

    public override double ReadDouble() => BitConverter.ToDouble(ReadBigEndian(8), 8 - 8);

    public override int ReadInt32() => BitConverter.ToInt32(ReadBigEndian(4), 8 - 4);

    public override int PeekChar() => throw new NotImplementedException();

    public override int Read() => throw new NotImplementedException();

    public override int Read(byte[] buffer, int index, int count) => base.Read(buffer, index, count);

    public override int Read(char[] buffer, int index, int count) => throw new NotImplementedException();

    public override bool ReadBoolean() => throw new NotImplementedException();

    public override char ReadChar() => throw new NotImplementedException();

    public override char[] ReadChars(int count) => throw new NotImplementedException();

    public override decimal ReadDecimal() => throw new NotImplementedException();

    public override long ReadInt64() => throw new NotImplementedException();

    public override sbyte ReadSByte() => throw new NotImplementedException();

    public override float ReadSingle() => throw new NotImplementedException();

    public override string ReadString() => throw new NotImplementedException();
}