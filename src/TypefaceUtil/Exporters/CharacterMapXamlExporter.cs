using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;

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

            streamWriter.WriteLine("<Styles xmlns=\"https://github.com/avaloniaui\"");
            streamWriter.WriteLine("        xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");
            streamWriter.WriteLine("    <Style>");
            streamWriter.WriteLine("        <Style.Resources>");

            foreach (var kvp in characterToGlyphMap)
            {
                var charCode = kvp.Key;
                var utf32 = Char.ConvertFromUtf32(charCode);
                float x = 0;
                float y = (mAscent / 2.0f) - mDescent / 2.0f;

                using var outlinePath = skTextPaint.GetTextPath(utf32, x, y);
                using var fillPath = skTextPaint.GetFillPath(outlinePath);

                fillPath.Transform(SKMatrix.MakeTranslation(-fillPath.Bounds.Left, -fillPath.Bounds.Top));

                var svgPathData = fillPath.ToSvgPathData();

                streamWriter.WriteLine($"            <GeometryDrawing x:Key=\"{charCode.ToString("X2").PadLeft(5, '0')}\" Brush=\"{brush}\" Geometry=\"{svgPathData}\"/>"); //  x:Key=\"{key}\"
            }

            streamWriter.WriteLine("        </Style.Resources>");
            streamWriter.WriteLine("    </Style>");
            streamWriter.WriteLine("</Styles>");
        }
    }
}
