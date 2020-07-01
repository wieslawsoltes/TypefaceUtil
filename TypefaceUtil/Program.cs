using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;

// https://docs.microsoft.com/en-us/typography/opentype/spec/cmap
// https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6cmap.html
// https://docs.microsoft.com/en-us/dotnet/api/skiasharp.sktypeface?view=skiasharp-1.68.1
// https://opentype.js.org/glyph-inspector.html
// https://opentype.js.org/font-inspector.html
// https://fontdrop.info/
// https://github.com/opentypejs/opentype.js/blob/master/src/tables/cmap.js
// https://github.com/LayoutFarm/Typography/blob/master/Typography.OpenFont/Tables/Cmap.cs
// https://github.com/LayoutFarm/Typography/blob/master/Typography.OpenFont/Tables/CharacterMap.cs

// dotnet run -- ../../segoeui.ttf > segui.txt
// dotnet run -- ../../seguisym.ttf > seguisym.txt
// dotnet run -- "Segoe UI"
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

        struct SequentialMapGroup
        {
            public UInt32 startCharCode;
            public UInt32 endCharCode;
            public UInt32 startGlyphID;
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
                            var length = reader.ReadUInt16();
                            var language = reader.ReadUInt16();
                            var segCountX2 = reader.ReadUInt16(); // 2 × segCount.
                            var searchRange = reader.ReadUInt16(); // 2 × (2**floor(log2(segCount)))
                            var entrySelector = reader.ReadUInt16(); // log2(searchRange/2)
                            var rangeShift = reader.ReadUInt16(); // 2 × segCount - searchRange

                            Console.WriteLine($"length: {length}");
                            Console.WriteLine($"language: {language}");
                            Console.WriteLine($"segCountX2: {segCountX2}");
                            Console.WriteLine($"searchRange: {searchRange}");
                            Console.WriteLine($"entrySelector: {entrySelector}");
                            Console.WriteLine($"rangeShift: {rangeShift}");

                            var segCount = segCountX2 / 2;
                            Console.WriteLine($"segCount: {segCount}");

                            var endCodes = new UInt16[segCount];
                            var startCodes = new UInt16[segCount];
                            var idDeltas = new Int16[segCount];
                            var idRangeOffsets = new UInt16[segCount];

                            for (UInt16 j = 0; j < segCount; j++)
                            {
                                endCodes[j] = reader.ReadUInt16();
                            }

                            var	reservedPad = reader.ReadUInt16();

                            for (UInt16 j = 0; j < segCount; j++)
                            {
                                startCodes[j] = reader.ReadUInt16();
                            }

                            for (UInt16 j = 0; j < segCount; j++)
                            {
                                idDeltas[j] = reader.ReadInt16();
                            }

                            for (UInt16 j = 0; j < segCount; j++)
                            {
                                idRangeOffsets[j] = reader.ReadUInt16();
                            }

                            Console.WriteLine($"segments:");
                            Console.WriteLine($"startCode | endCode | idDelta | idRangeOffset");
                            for (UInt32 j = 0; j < segCount; j++)
                            {
                                var startCode = startCodes[j];
                                var endCode = endCodes[j];
                                var idDelta = idDeltas[j];
                                var idRangeOffset = idRangeOffsets[j];
                                Console.WriteLine($"{startCode.ToString().PadRight(9)} | {endCode.ToString().PadRight(7)} | {idDelta.ToString().PadRight(7)} | {idRangeOffset.ToString()}");
                            }

                            // header:
                            // format 2 bytes
                            // length 2 bytes
                            // language 2 bytes
                            // segCountX2 2 bytes
                            // searchRange 2 bytes
                            // entrySelector 2 bytes
                            // rangeShift 2 bytes
                            // endCodes segCount*2 bytes
                            // reservedPad 2 bytes
                            // startCodes segCount*2 bytes
                            // idDeltas segCount*2 bytes
                            // idRangeOffsets segCount*2 bytes
                            var headerLength = (8 * 2) + (4 * segCount * 2);
                            var glyphIdArrayLength = (length - headerLength) / 2; // length - header
                            var glyphIdArray = new UInt16[glyphIdArrayLength];
                            Console.WriteLine($"headerLength: {headerLength}");
                            Console.WriteLine($"glyphIdArrayLength: {glyphIdArrayLength}");

                            for (UInt32 j = 0; j < glyphIdArrayLength; j++)
                            {
                                glyphIdArray[j] = reader.ReadUInt16();
                                Console.WriteLine($"glyphIdArray[{j}]: {glyphIdArray[j]} (position={ms.Position-2})");
                            }

                            // mapping of a Unicode code point to a glyph index
                            var characterToGlyphMap = new Dictionary<int, ushort>();

                            for (UInt32 j = 0; j < segCount; j++)
                            {
                                var startCode = startCodes[j];
                                var endCode = endCodes[j];
                                var idDelta = idDeltas[j];
                                var idRangeOffset = idRangeOffsets[j];

                                for (int c = startCode; c <= endCode; c += 1)
                                {
                                    UInt16 charCode = (UInt16)c;
                                    if (charCode == 0xFFFF)
                                    {
                                        continue;
                                    }

                                    if (idRangeOffset != 0)
                                    {
                                        int glyphIndexOffset = (idRangeOffset / 2) + (charCode - startCode) - idRangeOffsets.Length + (int)j;

                                        // Console.WriteLine($"glyphIndexOffset={glyphIndexOffset} (idRangeOffset/2)={idRangeOffset/2}, (charCode - startCode)={(charCode - startCode)}, idRangeOffsets.Length={idRangeOffsets.Length}, j={j}");

                                        UInt16 glyphIndex = glyphIdArray[glyphIndexOffset];

                                        if (glyphIndex != 0)
                                        {
                                            glyphIndex = (UInt16)((glyphIndex + idDelta) % 0xFFFF);
                                        }

                                        // Console.WriteLine($"charCode={charCode} : glyphIdArray[{glyphIndexOffset}]={glyphIndex} => {charCode} - {startCode} (idRangeOffset={idRangeOffset}, idDelta={idDelta}, j={j})");

                                        characterToGlyphMap[(int)charCode] = (ushort)glyphIndex;
                                    }
                                    else
                                    {
                                        UInt16 glyphIndex = (UInt16)((charCode + idDelta) % 0xFFFF);
                                        characterToGlyphMap[(int)charCode] = (ushort)glyphIndex;
                                    }
                                }
                            }

                            Console.WriteLine($"characterToGlyphMap:");
                            Console.WriteLine($"characterToGlyphMap.Count: {characterToGlyphMap.Count}");
                            Console.WriteLine($"charCode | glyphIndex");  

                            foreach (var kvp in characterToGlyphMap)
                            {
                                var charCode = kvp.Key;
                                var glyphIndex = kvp.Value;
                                Console.WriteLine($"{charCode.ToString().PadRight(8)} | {glyphIndex.ToString()}");
                            }

                            SaveCharMapPng(characterToGlyphMap, typeface, $"charmap_Format_4_({typeface.FamilyName}).png");

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
                            var reserved = reader.ReadUInt16();
                            var length = reader.ReadUInt32();
                            var language = reader.ReadUInt32();
                            var numGroups = reader.ReadUInt32();

                            Console.WriteLine($"length: {length}");
                            Console.WriteLine($"language: {language}");
                            Console.WriteLine($"numGroups: {numGroups}");

                            var groups = new SequentialMapGroup[numGroups];

                            Console.WriteLine($"groups:");
                            Console.WriteLine($"startCharCode | endCharCode | startGlyphID");

                            for (UInt32 j = 0; j < numGroups; j++)
                            {
                                groups[j].startCharCode = reader.ReadUInt32();
                                groups[j].endCharCode = reader.ReadUInt32();
                                groups[j].startGlyphID = reader.ReadUInt32();

                                Console.WriteLine($"{groups[j].startCharCode.ToString().PadRight(13)} | {groups[j].endCharCode.ToString().PadRight(11)} | {groups[j].startGlyphID.ToString()}");
                            }

                            // mapping of a Unicode code point to a glyph index
                            var characterToGlyphMap = new Dictionary<int, ushort>();

                            for (UInt32 j = 0; j < numGroups; j++)
                            {
                                var startCharCode = groups[j].startCharCode;
                                var endCharCode = groups[j].endCharCode;
                                var startGlyphID = groups[j].startGlyphID;

                                for (UInt32 charCode = groups[j].startCharCode; charCode < groups[j].endCharCode; charCode++)
                                {
                                    characterToGlyphMap[(int)charCode] = (ushort)startGlyphID;
                                    startGlyphID++;
                                }
                            }

                            Console.WriteLine($"characterToGlyphMap:");
                            Console.WriteLine($"characterToGlyphMap.Count: {characterToGlyphMap.Count}");
                            Console.WriteLine($"charCode | glyphIndex");  

                            foreach (var kvp in characterToGlyphMap)
                            {
                                var charCode = kvp.Key;
                                var glyphIndex = kvp.Value;
                                Console.WriteLine($"{charCode.ToString().PadRight(8)} | {glyphIndex.ToString()}");
                            }

                            SaveCharMapPng(characterToGlyphMap, typeface, $"charmap_Format_12_({typeface.FamilyName}).png");
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

        static void SaveCharMapPng(Dictionary<int, ushort> characterToGlyphMap, SKTypeface typeface, string path)
        {
            var numCharCodes = characterToGlyphMap.Count;

            float textSize = 20;
            int size = 40;
            int columns = 20;
            int rows = (int)Math.Ceiling((double)((double)numCharCodes / (double)columns));
            int width = (columns * size);
            int height = (rows * size);
            var skImageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            var skBitmap = new SKBitmap(skImageInfo);
            using var skCanvas = new SKCanvas(skBitmap);

            // Console.WriteLine($"{columns}x{rows} ({width} {height})");

            skCanvas.Clear(new SKColor(0xFF, 0xFF, 0xFF));

            using var skLinePaint = new SKPaint();
            skLinePaint.IsAntialias = false;
            skLinePaint.Color = new SKColor(0x00, 0x00, 0x00);
            skLinePaint.StrokeWidth = 1;

            for (float x = size; x < (float)width; x += size)
            {
                skCanvas.DrawLine(x, 0f, x, (float)height, skLinePaint);
            }

            for (float y = size; y < (float)height; y += size)
            {
                skCanvas.DrawLine(0f, y, (float)width, y, skLinePaint);
            }

            using var skTextPaint = new SKPaint();
            skTextPaint.IsAntialias = true;
            skTextPaint.Color = new SKColor(0x00, 0x00, 0x00);
            skTextPaint.Typeface  = typeface;
            skTextPaint.TextEncoding  = SKTextEncoding.Utf32;
            skTextPaint.TextSize = textSize;
            skTextPaint.TextAlign = SKTextAlign.Center;
            skTextPaint.LcdRenderText = true;
            skTextPaint.SubpixelText = true;

            var metrics = skTextPaint.FontMetrics;
            var mAscent = metrics.Ascent;
            var mDescent = metrics.Descent;

            UInt32 charCodeCount = 0;

            foreach (var kvp in characterToGlyphMap)
            {
                var charCode = kvp.Key;
                var utf32 = Char.ConvertFromUtf32((int)charCode);
                int row = (int)Math.Floor((double)((double)charCodeCount / (double)columns));
                int column = (int)((((double)charCodeCount * (double)size) % (double)width) / (double)size);
                float x = (float)(column * size) + (size / 2f);
                float y = (row * size) + ((size / 2.0f) - (mAscent / 2.0f) - mDescent / 2.0f);

                charCodeCount++;
                // Console.WriteLine($"{charCodeCount} {row}x{column} ({x} {y})");
                skCanvas.DrawText(utf32, x, y, skTextPaint);
            }

            using var stream = File.OpenWrite(path);
            using var skImage = SKImage.FromBitmap(skBitmap);
            using var skData = skImage.Encode(SKEncodedImageFormat.Png, 100);
            skData.SaveTo(stream);
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
