using System;
using System.IO;
using SkiaSharp;

// https://docs.microsoft.com/en-us/typography/opentype/spec/cmap
// https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6cmap.html
// https://docs.microsoft.com/en-us/dotnet/api/skiasharp.sktypeface?view=skiasharp-1.68.1

// dotnet run -- ../../seguisym.ttf
// dotnet run -- "Segoe UI Symbol"

namespace TypefaceUtil
{
    class Program
    {
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

        static uint GetIntTag(string v)
        {
            return (UInt32)(v[0]) << 24 | (UInt32)(v[1]) << 16 | (UInt32)(v[2]) << 08 | (UInt32)(v[3]) << 00;
        }

        static string GetPlatformID(UInt16 platformID)
        {
            switch (platformID)
            {
                case 0: return "Unicode";
                case 1: return "Macintosh";
                case 3: return "Windows";
                case 4: return "Custom";
                default: return "unknown";
            }
        }

        static string GetEncodingID(UInt16 platformID, UInt16 encodingID)
        {
            switch (platformID)
            {
                case 3:
                {
                    switch (encodingID)
                    {
                        case 0: return "Symbol";
                        case 1: return "Unicode BMP";
                        case 2: return "ShiftJIS";
                        case 3: return "PRC";
                        case 4: return "Big5";
                        case 5: return "Wansung";
                        case 6: return "Johab";
                        case 7: return "Reserved";
                        case 8: return "Reserved";
                        case 9: return "Reserved";
                        case 10: return "Unicode full repertoire";
                        default: return "unknown";
                    }
                }
                default: return "unknown";
            }
        }

        static string GetFormat(UInt16 format)
        {
            switch (format)
            {
                case 0: return "Format 0: Byte encoding table";
                case 2: return "Format 2: High-byte mapping through table";
                case 4: return "Format 4: Segment mapping to delta values";
                case 6: return "Format 6: Trimmed table mapping";
                case 8: return "Format 8: mixed 16-bit and 32-bit coverage";
                case 10: return "Format 10: Trimmed array";
                case 12: return "Format 12: Segmented coverage";
                case 13: return "Format 13: Many-to-one range mappings";
                case 14: return "Format 14: Unicode Variation Sequences";
                default: return "unknown";
            }
        }

        static void ReadCmapTable(SKTypeface typeface)
        {
            Console.WriteLine($"FamilyName: {typeface.FamilyName}");

            var cmap = typeface.GetTableData(GetIntTag("cmap"));
            using var ms = new MemoryStream(cmap);
            using var reader = new BigEndianBinaryReader(ms);

            // 'cmap' Header
            // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#cmap-header
            var version = reader.ReadUInt16();
            var numTables = reader.ReadUInt16();

            Console.WriteLine($"numTables: {numTables}");

            var encodingRecords = new EncodingRecord[numTables];

            // Encoding records and encodings
            // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#encoding-records-and-encodings

            for (UInt16 i = 0; i < numTables; i++)
            {
                encodingRecords[i].platformID = reader.ReadUInt16();
                encodingRecords[i].encodingID = reader.ReadUInt16();
                encodingRecords[i].offset = reader.ReadUInt32();
            }

            for (UInt16 i = 0; i < numTables; i++)
            {
                var platformID = encodingRecords[i].platformID;
                var encodingID = encodingRecords[i].encodingID;
                var offset = encodingRecords[i].offset;

                ms.Position = offset;

                var format = reader.ReadUInt16();

                Console.WriteLine($"---");
                Console.WriteLine($"platformID: {platformID} ({GetPlatformID(platformID)})");
                Console.WriteLine($"encodingID: {encodingID} ({GetEncodingID(platformID, encodingID)})");
                Console.WriteLine($"offset: {offset}");
                Console.WriteLine($"format: {format} ({GetFormat(format)})");

                switch (format)
                {
                    // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#format-0-byte-encoding-table
                    case 0:
                        {
                            // TODO:
                        }
                        break;
                    // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#format-2-high-byte-mapping-through-table
                    case 2:
                        {
                            // TODO:
                        }
                        break;
                    // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#format-4-segment-mapping-to-delta-values
                    case 4:
                        {
                            // TODO:
                        }
                        break;
                    // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#format-6-trimmed-table-mapping
                    case 6:
                        {
                            // TODO:
                        }
                        break;
                    // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#format-8-mixed-16-bit-and-32-bit-coverage
                    case 8:
                        {
                            // TODO:
                        }
                        break;
                    // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#format-10-trimmed-array
                    case 10:
                        {
                            // TODO:
                        }
                        break;
                    // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#format-12-segmented-coverage
                    case 12:
                        {
                            // TODO:
                        }
                        break;
                    // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#format-13-many-to-one-range-mappings
                    case 13:
                        {
                            // TODO:
                        }
                        break;
                    // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#format-14-unicode-variation-sequences
                    case 14:
                        {
                            // TODO:
                        }
                        break;
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                if (File.Exists(args[0]))
                {
                    using var typeface = SKTypeface.FromFile(args[0]);
                    ReadCmapTable(typeface);
                }
                else
                {
                    using var typeface = SKTypeface.FromFamilyName(args[0]);
                    ReadCmapTable(typeface);
                }
            }
        }
    }
}
