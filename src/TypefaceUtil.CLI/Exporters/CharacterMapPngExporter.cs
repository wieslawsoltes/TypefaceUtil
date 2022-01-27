using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;

namespace TypefaceUtil;

public static class CharacterMapPngExporter
{
    public static void Save(Dictionary<int, ushort> characterToGlyphMap, SKTypeface typeface, float textSize, int cellSize, int columns, Stream stream)
    {
        var skBackgroundColor = new SKColor(0xFF, 0xFF, 0xFF);
        var skLineColor = new SKColor(0x00, 0x00, 0x00);
        var skTextColor = new SKColor(0x00, 0x00, 0x00);

        var numCharCodes = characterToGlyphMap.Count;

        int rows = (int)Math.Ceiling((double)((double)numCharCodes / (double)columns));
        int width = (columns * cellSize);
        int height = (rows * cellSize);
        var skImageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        var skBitmap = new SKBitmap(skImageInfo);
        using var skCanvas = new SKCanvas(skBitmap);

        skCanvas.Clear(skBackgroundColor);

        using var skLinePaint = new SKPaint
        {
            IsAntialias = false,
            Color = skLineColor,
            StrokeWidth = 1
        };

        for (float x = cellSize; x < (float)width; x += cellSize)
        {
            skCanvas.DrawLine(x, 0f, x, (float)height, skLinePaint);
        }

        for (float y = cellSize; y < (float)height; y += cellSize)
        {
            skCanvas.DrawLine(0f, y, (float)width, y, skLinePaint);
        }

        using var skTextPaint = new SKPaint
        {
            IsAntialias = true,
            Color = skTextColor,
            Typeface = typeface,
            TextEncoding = SKTextEncoding.Utf32,
            TextSize = textSize,
            TextAlign = SKTextAlign.Center,
            LcdRenderText = true,
            SubpixelText = true
        };

        var metrics = skTextPaint.FontMetrics;
        var mAscent = metrics.Ascent;
        var mDescent = metrics.Descent;

        UInt32 charCodeCount = 0;

        foreach (var kvp in characterToGlyphMap)
        {
            var charCode = kvp.Key;
            var utf32 = Char.ConvertFromUtf32((int)charCode);
            int row = (int)Math.Floor((double)((double)charCodeCount / (double)columns));
            int column = (int)((((double)charCodeCount * (double)cellSize) % (double)width) / (double)cellSize);
            float x = (float)(column * cellSize) + (cellSize / 2f);
            float y = (row * cellSize) + ((cellSize / 2.0f) - (mAscent / 2.0f) - mDescent / 2.0f);

            charCodeCount++;

            skCanvas.DrawText(utf32, x, y, skTextPaint);
        }

        using var skImage = SKImage.FromBitmap(skBitmap);
        using var skData = skImage.Encode(SKEncodedImageFormat.Png, 100);
        skData.SaveTo(stream);
    }
}