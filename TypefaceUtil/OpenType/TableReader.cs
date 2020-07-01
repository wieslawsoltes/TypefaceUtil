using System;
using System.Collections.Generic;
using System.IO;

namespace TypefaceUtil
{
    public class TableReader
    {
        public static uint GetIntTag(string tag)
        {
            return (UInt32)(tag[0]) << 24 | (UInt32)(tag[1]) << 16 | (UInt32)(tag[2]) << 08 | (UInt32)(tag[3]) << 00;
        }

        public static string GetPlatformID(UInt16 platformID)
        {
            return platformID switch
            {
                0 => "Unicode",
                1 => "Macintosh",
                3 => "Windows",
                4 => "Custom",
                _ => "unknown",
            };
        }

        public static string GetEncodingID(UInt16 platformID, UInt16 encodingID)
        {
            switch (platformID)
            {
                case 3:
                    {
                        return encodingID switch
                        {
                            0 => "Symbol",
                            1 => "Unicode BMP",
                            2 => "ShiftJIS",
                            3 => "PRC",
                            4 => "Big5",
                            5 => "Wansung",
                            6 => "Johab",
                            7 => "Reserved",
                            8 => "Reserved",
                            9 => "Reserved",
                            10 => "Unicode full repertoire",
                            _ => "unknown",
                        };
                    }
                default: return "unknown";
            }
        }

        public static string GetFormat(UInt16 format)
        {
            return format switch
            {
                0 => "Format 0: Byte encoding table",
                2 => "Format 2: High-byte mapping through table",
                4 => "Format 4: Segment mapping to delta values",
                6 => "Format 6: Trimmed table mapping",
                8 => "Format 8: mixed 16-bit and 32-bit coverage",
                10 => "Format 10: Trimmed array",
                12 => "Format 12: Segmented coverage",
                13 => "Format 13: Many-to-one range mappings",
                14 => "Format 14: Unicode Variation Sequences",
                _ => "unknown",
            };
        }

        private static Dictionary<int, ushort> ReadCmapFormat4(BigEndianBinaryReader reader)
        {
            var length = reader.ReadUInt16();
            var language = reader.ReadUInt16();
            var segCountX2 = reader.ReadUInt16(); // 2 × segCount.
            var searchRange = reader.ReadUInt16(); // 2 × (2**floor(log2(segCount)))
            var entrySelector = reader.ReadUInt16(); // log2(searchRange/2)
            var rangeShift = reader.ReadUInt16(); // 2 × segCount - searchRange

            // Console.WriteLine($"length: {length}");
            // Console.WriteLine($"language: {language}");
            // Console.WriteLine($"segCountX2: {segCountX2}");
            // Console.WriteLine($"searchRange: {searchRange}");
            // Console.WriteLine($"entrySelector: {entrySelector}");
            // Console.WriteLine($"rangeShift: {rangeShift}");

            var segCount = segCountX2 / 2;
            // Console.WriteLine($"segCount: {segCount}");

            var endCodes = new UInt16[segCount];
            var startCodes = new UInt16[segCount];
            var idDeltas = new Int16[segCount];
            var idRangeOffsets = new UInt16[segCount];

            for (UInt16 j = 0; j < segCount; j++)
            {
                endCodes[j] = reader.ReadUInt16();
            }

            var reservedPad = reader.ReadUInt16();

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

            // Console.WriteLine($"segments:");
            // Console.WriteLine($"startCode | endCode | idDelta | idRangeOffset");

            for (UInt32 j = 0; j < segCount; j++)
            {
                var startCode = startCodes[j];
                var endCode = endCodes[j];
                var idDelta = idDeltas[j];
                var idRangeOffset = idRangeOffsets[j];
                // Console.WriteLine($"{startCode.ToString().PadRight(9)} | {endCode.ToString().PadRight(7)} | {idDelta.ToString().PadRight(7)} | {idRangeOffset.ToString()}");
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
            // Console.WriteLine($"headerLength: {headerLength}");
            // Console.WriteLine($"glyphIdArrayLength: {glyphIdArrayLength}");

            for (UInt32 j = 0; j < glyphIdArrayLength; j++)
            {
                glyphIdArray[j] = reader.ReadUInt16();
                // Console.WriteLine($"glyphIdArray[{j}]: {glyphIdArray[j]} (position={ms.Position-2})");
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

            // Console.WriteLine($"characterToGlyphMap:");
            // Console.WriteLine($"characterToGlyphMap.Count: {characterToGlyphMap.Count}");
            // Console.WriteLine($"charCode | glyphIndex");  

            //foreach (var kvp in characterToGlyphMap)
            //{
            //    var charCode = kvp.Key;
            //    var glyphIndex = kvp.Value;
            //    Console.WriteLine($"{charCode.ToString().PadRight(8)} | {glyphIndex.ToString()}");
            //}

            return characterToGlyphMap;
        }

        private static Dictionary<int, ushort> ReadCmapFormat12(BigEndianBinaryReader reader)
        {
            var reserved = reader.ReadUInt16();
            var length = reader.ReadUInt32();
            var language = reader.ReadUInt32();
            var numGroups = reader.ReadUInt32();

            // Console.WriteLine($"length: {length}");
            // Console.WriteLine($"language: {language}");
            // Console.WriteLine($"numGroups: {numGroups}");

            var groups = new SequentialMapGroup[numGroups];

            // Console.WriteLine($"groups:");
            // Console.WriteLine($"startCharCode | endCharCode | startGlyphID");

            for (UInt32 j = 0; j < numGroups; j++)
            {
                groups[j].startCharCode = reader.ReadUInt32();
                groups[j].endCharCode = reader.ReadUInt32();
                groups[j].startGlyphID = reader.ReadUInt32();

                // Console.WriteLine($"{groups[j].startCharCode.ToString().PadRight(13)} | {groups[j].endCharCode.ToString().PadRight(11)} | {groups[j].startGlyphID.ToString()}");
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

            // Console.WriteLine($"characterToGlyphMap:");
            // Console.WriteLine($"characterToGlyphMap.Count: {characterToGlyphMap.Count}");
            // Console.WriteLine($"charCode | glyphIndex");  

            //foreach (var kvp in characterToGlyphMap)
            //{
            //    var charCode = kvp.Key;
            //    var glyphIndex = kvp.Value;
            //    Console.WriteLine($"{charCode.ToString().PadRight(8)} | {glyphIndex.ToString()}");
            //}

            return characterToGlyphMap;
        }

        public static List<CharacterMap> ReadCmapTable(byte[] cmap)
        {
            var characterMaps = new List<CharacterMap>();

            using var ms = new MemoryStream(cmap);
            using var reader = new BigEndianBinaryReader(ms);

            // cmap — Character to Glyph Index Mapping Table
            // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap

            // 'cmap' Header
            // https://docs.microsoft.com/en-us/typography/opentype/spec/cmap#cmap-header
            var version = reader.ReadUInt16();
            var numTables = reader.ReadUInt16();

            // Console.WriteLine($"numTables: {numTables}");

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

                // Console.WriteLine($"---");
                // Console.WriteLine($"platformID: {platformID} ({GetPlatformID(platformID)})");
                // Console.WriteLine($"encodingID: {encodingID} ({GetEncodingID(platformID, encodingID)})");
                // Console.WriteLine($"offset: {offset}");
                // Console.WriteLine($"format: {format} ({GetFormat(format)})");

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
                           var characterToGlyphMap = ReadCmapFormat4(reader);

                            var characterMap = new CharacterMap()
                            {
                                Name = "Format_4",
                                CharacterToGlyphMap = characterToGlyphMap
                            };

                            characterMaps.Add(characterMap);
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
                            var characterToGlyphMap = ReadCmapFormat12(reader);

                            var characterMap = new CharacterMap()
                            {
                                Name = "Format_12",
                                CharacterToGlyphMap = characterToGlyphMap
                            };

                            characterMaps.Add(characterMap);
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

            return characterMaps;
        }
    }
}
