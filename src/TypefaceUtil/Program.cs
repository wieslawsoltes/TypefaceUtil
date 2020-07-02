﻿using System.IO;
using SkiaSharp;
using TypefaceUtil.OpenType;

namespace TypefaceUtil
{
    partial class Program
    {
        static void Read(SKTypeface typeface)
        {
            var cmap = typeface.GetTableData(TableReader.GetIntTag("cmap"));
            var characterMaps = TableReader.ReadCmapTable(cmap);

            foreach (var characterMap in characterMaps)
            {
                if (characterMap.CharacterToGlyphMap != null)
                {
                    {
                        using var stream = File.OpenWrite($"charmap_({typeface.FamilyName})_{characterMap.Name}.png");
                        CharacterMapPngExporter.Save(characterMap.CharacterToGlyphMap, typeface, textSize: 20, size: 40, columns: 20, stream);
                    }
    
                    {
                        using var streamWriter = File.CreateText($"charmap_({typeface.FamilyName})_{characterMap.Name}.svg.txt");
                        CharacterMapSvgExporter.Save(characterMap.CharacterToGlyphMap, typeface, 16f, "black", streamWriter);
                    }
    
                    {
                        using var streamWriter = File.CreateText($"charmap_({typeface.FamilyName})_{characterMap.Name}.xaml.txt");
                        CharacterMapXamlExporter.Save(characterMap.CharacterToGlyphMap, typeface, 16f, "Black", streamWriter);
                    }
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
                    Read(typeface);
                }
                else
                {
                    using var typeface = SKTypeface.FromFamilyName(args[0]);
                    Read(typeface);
                }
            }
        }
    }
}