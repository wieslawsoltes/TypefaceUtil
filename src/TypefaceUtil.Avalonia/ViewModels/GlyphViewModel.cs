using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using ReactiveUI;
using SkiaSharp;

namespace TypefaceUtil.Avalonia.ViewModels
{
    public class GlyphViewModel : ViewModelBase
    {
        private int _charCode;
        private ushort _glyphIndex;
        private SKPath? _path;
        private SKPaint? _paint;
        private string? _svgPathData;

        public int CharCode
        {
            get => _charCode;
            set => this.RaiseAndSetIfChanged(ref _charCode, value);
        }

        public ushort GlyphIndex
        {
            get => _glyphIndex;
            set => this.RaiseAndSetIfChanged(ref _glyphIndex, value);
        }

        public SKPath? Path
        {
            get => _path;
            set => this.RaiseAndSetIfChanged(ref _path, value);
        }

        public SKPaint? Paint
        {
            get => _paint;
            set => this.RaiseAndSetIfChanged(ref _paint, value);
        }

        public string? SvgPathData
        {
            get => _svgPathData;
            set => this.RaiseAndSetIfChanged(ref _svgPathData, value);
        }

        public ICommand CopyAsCommand { get; }

        public GlyphViewModel()
        {
            CopyAsCommand = ReactiveCommand.CreateFromTask<string>(async (format) =>
            {
                await CopyAs(format);
            });
        }

        private async Task CopyAs(string format)
        {
            var brush = "#000000";
            var indent = "  ";
            var text = format switch
            {
                "XamlStreamGeometry" => $"<StreamGeometry>{SvgPathData}</StreamGeometry>",
                "XamlPathIcon" => $"<PathIcon Width=\"{Path?.Bounds.Width.ToString(CultureInfo.InvariantCulture)}\" Height=\"{Path?.Bounds.Height.ToString(CultureInfo.InvariantCulture)}\" Foreground=\"{brush}\" Data=\"{SvgPathData}\"/>",
                "XamlPath" => $"<Path Width=\"{Path?.Bounds.Width.ToString(CultureInfo.InvariantCulture)}\" Height=\"{Path?.Bounds.Height.ToString(CultureInfo.InvariantCulture)}\" Fill=\"{brush}\" Data=\"{SvgPathData}\"/>",
                "XamlCanvas" => $"<Canvas Width=\"{Path?.Bounds.Width.ToString(CultureInfo.InvariantCulture)}\" Height=\"{Path?.Bounds.Height.ToString(CultureInfo.InvariantCulture)}\">\r\n{indent}<Path Fill=\"{brush}\" Data=\"{SvgPathData}\"/>\r\n</Canvas>",
                "XamlGeometryDrawing" => $"<GeometryDrawing Brush=\"{brush}\" Geometry=\"{SvgPathData}\"/>",
                "XamlDrawingGroup" => $"<DrawingGroup>\r\n{indent}<GeometryDrawing Brush=\"{brush}\" Geometry=\"{SvgPathData}\"/>\r\n</DrawingGroup>",
                "XamlDrawingImage" => $"<DrawingImage>\r\n{indent}<GeometryDrawing Brush=\"{brush}\" Geometry=\"{SvgPathData}\"/>\r\n</DrawingImage>",
                "XamlImage" => $"<Image>\r\n{indent}<DrawingImage>\r\n{indent}{indent}<GeometryDrawing Brush=\"{brush}\" Geometry=\"{SvgPathData}\"/>\r\n</DrawingImage>\r\n</Image>",
                "SvgPathData" => $"{SvgPathData}",
                "SvgPath" => $"<path fill=\"{brush}\" d=\"{SvgPathData}\"/>",
                "Svg" => $"<svg viewBox=\"{Path?.Bounds.Left.ToString(CultureInfo.InvariantCulture)} {Path?.Bounds.Top.ToString(CultureInfo.InvariantCulture)} {Path?.Bounds.Width.ToString(CultureInfo.InvariantCulture)} {Path?.Bounds.Height.ToString(CultureInfo.InvariantCulture)}\" xmlns=\"http://www.w3.org/2000/svg\">\r\n{indent}<path fill=\"{brush}\" d=\"{SvgPathData}\"/>\r\n</svg>",
                _ => default
            };

            if (!string.IsNullOrWhiteSpace(text))
            {
                try
                {
                    await Application.Current.Clipboard.SetTextAsync(text);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
