using System;
using System.IO;
using SkiaSharp;

// https://docs.microsoft.com/en-us/typography/opentype/spec/cmap
// https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6cmap.html

namespace TypefaceUtil
{
    class Program
    {
        static uint GetIntTag(string v)
        {
            return (UInt32)(v[0]) << 24 | (UInt32)(v[1]) << 16 | (UInt32)(v[2]) << 08 | (UInt32)(v[3]) << 00;
        }

        class BigEndianBinaryReader : BinaryReader
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

        struct EncodingRecord
        {
            public UInt16 platformID;
            public UInt16 encodingID;
            public UInt32 offset;
        }

        static void Main(string[] args)
        {
            using var tf = SKTypeface.FromFamilyName("Segoe UI Symbol");
            var cmap = tf.GetTableData(GetIntTag("cmap"));
            using var ms = new MemoryStream(cmap);
            using var reader = new BigEndianBinaryReader(ms);

            // index
            var version = reader.ReadUInt16();
            var numTables = reader.ReadUInt16();

            // encoding subtables
            var encodingRecords = new EncodingRecord[numTables];

            for (UInt16 i = 0; i < numTables; i++)
            {
                encodingRecords[i].platformID = reader.ReadUInt16();
                encodingRecords[i].encodingID = reader.ReadUInt16();
                encodingRecords[i].offset = reader.ReadUInt32();
            }

            // TODO:
        }
    }
}
