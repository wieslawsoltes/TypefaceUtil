using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SkiaSharp;

namespace TypefaceUtil.Avalonia.Views;

public class Glyph
{
    public SKPath Path { get; set; }
    
    public SKPaint Paint { get; set; }
    public string PathData { get; set; }
}

public class Font
{
    public string Name { get; set; }

    public List<Glyph> Glyphs { get; set; }
}

public class FontsViewModel : ReactiveObject
{
    public List<Font> Fonts { get; set; }
    
    [Reactive] public Font? SelectedFont { get; set; }
}

public partial class MainWindow : Window
{
    public FontsViewModel FontsViewModel { get; set; }

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        /*
        var result = FontManager.Current.TryMatchCharacter(
            '℄', 
            FontStyle.Normal, 
            FontWeight.Normal, 
            FontStretch.Normal, 
            null, 
            null, 
            out var typeface);

        if (result)
        {
            Console.WriteLine(typeface.FontFamily.Name);
        }
        
       var skTypeface = SKTypeface.FromFamilyName("CascadiaMono");
       var skFont = skTypeface.ToFont();
       //var skPath = skFont.GetGlyphPath('Φ');
       //var pathData = skPath.ToSvgPathData();
       //var skPath = skFont.GetGlyphPath('℄');
        */
        
        FontsViewModel = new ()
        {
            Fonts = new List<Font>()
        };

        var paint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Black,
            StrokeWidth = 1
        };

        foreach (var fontFamily in SKFontManager.Default.FontFamilies.OrderBy(x => x))
        {
            Console.WriteLine(fontFamily);
            
            var font = new Font
            {
                Name = fontFamily,
                Glyphs = new List<Glyph>()
            };

            var skTypeface = SKTypeface.FromFamilyName(fontFamily);
            var skFont = skTypeface.ToFont(24);

            for (ushort i = 0; i < ushort.MaxValue; i++)
            {
                var path = skFont.GetGlyphPath(i);
                if (path is null)
                {
                    continue;
                }
                
                var skTranslationMatrix = SKMatrix.CreateTranslation(-path.Bounds.Left, -path.Bounds.Top);
                path.Transform(skTranslationMatrix);

                var pathData = path.ToSvgPathData();
                
                
                
                font.Glyphs.Add(new Glyph
                {
                    Path = path,
                    Paint = paint,
                    PathData = pathData
                });
            }

            FontsViewModel.Fonts.Add(font);
        }

        FontsViewModel.SelectedFont = FontsViewModel.Fonts.FirstOrDefault();
        
        DataContext = FontsViewModel;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
