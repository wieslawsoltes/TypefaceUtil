using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using ReactiveUI;
using SkiaSharp;

namespace TypefaceUtil.Avalonia.ViewModels;

public class GlyphViewModel : ViewModelBase
{
    private int _charCode;
    private ushort _glyphIndex;
    private SKPath? _path;
    private SKPaint? _paint;
    private string? _color;
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

    public string? Color
    {
        get => _color;
        set => this.RaiseAndSetIfChanged(ref _color, value);
    }

    public string? SvgPathData
    {
        get => _svgPathData;
        set => this.RaiseAndSetIfChanged(ref _svgPathData, value);
    }

    public ICommand CopyAsCommand { get; }

    public ICommand OpenCommand { get; }

    public ICommand CloseCommand { get; }

    public GlyphViewModel(Action<GlyphViewModel> onOpen, Action<GlyphViewModel> onClose)
    {
        CopyAsCommand = ReactiveCommand.CreateFromTask<string>(async (format) =>
        {
            await CopyAs(format, _color ?? "#000000");
        });

        OpenCommand = ReactiveCommand.Create(() => onOpen.Invoke(this));

        CloseCommand = ReactiveCommand.Create(() => onClose.Invoke(this));
    }

    public string? Export(string format, string color, bool addXamlKey)
    {
        if (Path is null || Path.Bounds.IsEmpty)
        {
            return default;
        }

        var indent = "  ";
        var xamlKey = addXamlKey ? $" x:Key=\"{CharCode}\"" : "";
        var text = format switch
        {
            "XamlStreamGeometry" => $"<StreamGeometry{xamlKey}>{SvgPathData}</StreamGeometry>",
            "XamlPathIcon" => $"<PathIcon{xamlKey} Width=\"{Path?.Bounds.Width.ToString(CultureInfo.InvariantCulture)}\" Height=\"{Path?.Bounds.Height.ToString(CultureInfo.InvariantCulture)}\" Foreground=\"{color}\" Data=\"{SvgPathData}\"/>",
            "XamlPath" => $"<Path{xamlKey} Width=\"{Path?.Bounds.Width.ToString(CultureInfo.InvariantCulture)}\" Height=\"{Path?.Bounds.Height.ToString(CultureInfo.InvariantCulture)}\" Fill=\"{color}\" Data=\"{SvgPathData}\"/>",
            "XamlCanvas" => $"<Canvas{xamlKey} Width=\"{Path?.Bounds.Width.ToString(CultureInfo.InvariantCulture)}\" Height=\"{Path?.Bounds.Height.ToString(CultureInfo.InvariantCulture)}\">\r\n{indent}<Path Fill=\"{color}\" Data=\"{SvgPathData}\"/>\r\n</Canvas>",
            "XamlGeometryDrawing" => $"<GeometryDrawing{xamlKey} Brush=\"{color}\" Geometry=\"{SvgPathData}\"/>",
            "XamlDrawingGroup" => $"<DrawingGroup{xamlKey}>\r\n{indent}<GeometryDrawing Brush=\"{color}\" Geometry=\"{SvgPathData}\"/>\r\n</DrawingGroup>",
            "XamlDrawingImage" => $"<DrawingImage{xamlKey}>\r\n{indent}<GeometryDrawing Brush=\"{color}\" Geometry=\"{SvgPathData}\"/>\r\n</DrawingImage>",
            "XamlImage" => $"<Image{xamlKey}>\r\n{indent}<DrawingImage>\r\n{indent}{indent}<GeometryDrawing Brush=\"{color}\" Geometry=\"{SvgPathData}\"/>\r\n</DrawingImage>\r\n</Image>",
            "SvgPathData" => $"{SvgPathData}",
            "SvgPath" => $"<path fill=\"{color}\" d=\"{SvgPathData}\"/>",
            "Svg" => $"<svg viewBox=\"{Path?.Bounds.Left.ToString(CultureInfo.InvariantCulture)} {Path?.Bounds.Top.ToString(CultureInfo.InvariantCulture)} {Path?.Bounds.Width.ToString(CultureInfo.InvariantCulture)} {Path?.Bounds.Height.ToString(CultureInfo.InvariantCulture)}\" xmlns=\"http://www.w3.org/2000/svg\">\r\n{indent}<path fill=\"{color}\" d=\"{SvgPathData}\"/>\r\n</svg>",
            _ => default
        };
        return text;
    }

    public async Task CopyAs(string format, string color)
    {
        var text = Export(format, color, false);
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