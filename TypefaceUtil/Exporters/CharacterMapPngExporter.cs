using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;

namespace TypefaceUtil
{
    public static class CharacterMapPngExporter
    {
        public static void Save(Dictionary<int, ushort> characterToGlyphMap, SKTypeface typeface, Stream stream)
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
            skTextPaint.Typeface = typeface;
            skTextPaint.TextEncoding = SKTextEncoding.Utf32;
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

            using var skImage = SKImage.FromBitmap(skBitmap);
            using var skData = skImage.Encode(SKEncodedImageFormat.Png, 100);
            skData.SaveTo(stream);
        }
    }
}
