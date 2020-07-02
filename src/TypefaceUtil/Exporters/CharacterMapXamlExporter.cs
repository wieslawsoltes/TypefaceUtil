using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;
using TypefaceUtil.OpenType;

namespace TypefaceUtil
{
    public static class CharacterMapXamlExporter
    {
        public static void Save(Dictionary<int, ushort> characterToGlyphMap, SKTypeface typeface, float textSize, string brush, StreamWriter streamWriter)
        {
            var skColor = new SKColor(0x00, 0x00, 0x00);

            using var skTextPaint = new SKPaint
            {
                IsAntialias = true,
                Color = skColor,
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

            foreach (var kvp in characterToGlyphMap)
            {
                var charCode = kvp.Key;
                var utf32 = Char.ConvertFromUtf32((int)charCode);
                float x = 0;
                float y = (mAscent / 2.0f) - mDescent / 2.0f;

                using var outlinePath = skTextPaint.GetTextPath(utf32, x, y);
                using var fillPath = skTextPaint.GetFillPath(outlinePath);

                fillPath.Transform(SKMatrix.MakeTranslation(-fillPath.Bounds.Left, -fillPath.Bounds.Top));

                var svgPathData = fillPath.ToSvgPathData();

                streamWriter.WriteLine($"[{utf32}]");
                streamWriter.WriteLine($"<GeometryDrawing Brush=\"{brush}\" Geometry=\"{svgPathData}\"/>"); //  x:Key=\"{key}\"
            }
        }
    }
}
