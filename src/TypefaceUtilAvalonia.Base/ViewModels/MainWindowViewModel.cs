﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Platform.Storage;
using ReactiveUI;
using SkiaSharp;
using TypefaceUtil.OpenType;

namespace TypefaceUtil.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private bool _isLoading;
    private string? _inputFile;
    private MemoryStream? _inputData;
    private string? _familyName;
    private ObservableCollection<string>? _fontFamilies;
    private float _fontSize;
    private string? _color;
    private double _itemWidth;
    private double _itemHeight;
    private TypefaceViewModel? _typeface;
    private GlyphViewModel? _selectedGlyph;

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public string? InputFile
    {
        get => _inputFile;
        set => this.RaiseAndSetIfChanged(ref _inputFile, value);
    }

    public MemoryStream? InputData
    {
        get => _inputData;
        set => this.RaiseAndSetIfChanged(ref _inputData, value);
    }

    public string? FamilyName
    {
        get => _familyName;
        set => this.RaiseAndSetIfChanged(ref _familyName, value);
    }

    public ObservableCollection<string>? FontFamilies
    {
        get => _fontFamilies;
        set => this.RaiseAndSetIfChanged(ref _fontFamilies, value);
    }

    public float FontSize
    {
        get => _fontSize;
        set => this.RaiseAndSetIfChanged(ref _fontSize, value);
    }

    public string? Color
    {
        get => _color;
        set => this.RaiseAndSetIfChanged(ref _color, value);
    }

    public double ItemWidth
    {
        get => _itemWidth;
        set => this.RaiseAndSetIfChanged(ref _itemWidth, value);
    }

    public double ItemHeight
    {
        get => _itemHeight;
        set => this.RaiseAndSetIfChanged(ref _itemHeight, value);
    }

    public TypefaceViewModel? Typeface
    {
        get => _typeface;
        set => this.RaiseAndSetIfChanged(ref _typeface, value);
    }

    public GlyphViewModel? SelectedGlyph
    {
        get => _selectedGlyph;
        set => this.RaiseAndSetIfChanged(ref _selectedGlyph, value);
    }

    public ICommand InputFileCommand { get; }

    public ICommand LoadInputFileCommand { get; }

    public ICommand LoadFamilyNameCommand { get; }

    public ICommand CloseCommand { get; }

    public ICommand CopyAsCommand { get; }

    public MainWindowViewModel()
    {
        FontFamilies = new ObservableCollection<string>(SetGetFontFamilies());
        FamilyName = "Segoe MDL2 Assets";

        FontSize = 32f;
        Color = "#000000";

        ItemWidth = 96;
        ItemHeight = 96;

        InputFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var storageProvider = StorageService.GetStorageProvider();
            if (storageProvider is null)
            {
                return;
            }

            var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open drawing",
                FileTypeFilter = GetOpenFileTypes(),
                AllowMultiple = false
            });

            var file = result.FirstOrDefault();

            if (file is not null && file.CanOpenRead)
            {
                try
                {
                    InputData?.Dispose();
                    InputFile = "";

                    InputData = await Task.Run(async () =>
                    {
                        await using var stream = await file.OpenReadAsync();
                        var ms = new MemoryStream();
                        await stream.CopyToAsync(ms);
                        ms.Position = 0;
                        return ms;
                    });
                    InputFile = file.Name;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                }
            }
        });

        LoadInputFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            IsLoading = true;
            await Task.Run(LoadInputFile);
            IsLoading = false;
        });

        LoadFamilyNameCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            IsLoading = true;
            await Task.Run(LoadFamilyName);
            IsLoading = false;
        });

        CloseCommand = ReactiveCommand.Create(() =>
        {
            SelectedGlyph = null;
            Typeface = null;
        });

        CopyAsCommand = ReactiveCommand.CreateFromTask<string>(async (format) =>
        {
            if (Typeface?.Glyphs is { })
            {
                try
                {
                    var allText = await Task.Run(() =>
                    {
                        var sb = new StringBuilder();

                        // sb.AppendLine($"<Styles xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");
                        // sb.AppendLine($"  <Style>");
                        // sb.AppendLine($"    <Style.Resources>");

                        foreach (var glyph in Typeface.Glyphs)
                        {
                            var glyphText = glyph.Export(format, glyph.Color ?? "#000000", true);
                            if (!string.IsNullOrWhiteSpace(glyphText))
                            {
                                sb.AppendLine(glyphText);
                            }
                        }

                        // sb.AppendLine($"    </Style.Resources>");
                        // sb.AppendLine($"  </Style>");
                        // sb.AppendLine($"</Styles>");

                        return sb.ToString();
                    });

                    if (!string.IsNullOrWhiteSpace(allText))
                    {
                        if (Application.Current?.Clipboard is { } clipboard)
                        {
                            await clipboard.SetTextAsync(allText);
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
        });
    }

    private List<FilePickerFileType> GetOpenFileTypes()
    {
        // TODO:
        // "ttf", "otf" : "Font Files"
        // "ttf" : "TTF Files"
        // "otf" : "OTF Files"
        return new List<FilePickerFileType>
        {
            StorageService.All
        };
    }

    private string[] SetGetFontFamilies()
    {
        var fontFamilies = SKFontManager.Default.GetFontFamilies();

        Array.Sort(fontFamilies, StringComparer.InvariantCulture);

        return fontFamilies;
    }

    private void LoadInputFile()
    {
        var inputFile = InputFile;
        var inputData = InputData;
        var fontSize = FontSize;
        var color = Color ?? "#000000";
        if (string.IsNullOrEmpty(inputFile))
        {
            return;
        }

        var typefaceViewModel = LoadFromData(inputData);
        if (typefaceViewModel?.Typeface is null)
        {
            return;
        }

        if (typefaceViewModel.Glyphs is { })
        {
            foreach (var glyph in LoadGlyphs(typefaceViewModel, fontSize, color).OrderBy(x => x.GlyphIndex))
            {
                typefaceViewModel.Glyphs.Add(glyph);
            }
        }
            
        Typeface = typefaceViewModel;
    }

    private void LoadFamilyName()
    {
        var familyName = FamilyName;
        var fontSize = FontSize;
        var color = Color ?? "#000000";
        if (string.IsNullOrEmpty(familyName))
        {
            return;
        }

        var typefaceViewModel = LoadFromFamilyName(familyName);
        if (typefaceViewModel?.Typeface is null)
        {
            return;
        }

        if (typefaceViewModel.Glyphs is { })
        {
            foreach (var glyph in LoadGlyphs(typefaceViewModel, fontSize, color).OrderBy(x => x.GlyphIndex))
            {
                typefaceViewModel.Glyphs.Add(glyph);
            }
        }

        Typeface = typefaceViewModel;
    }

    private IEnumerable<GlyphViewModel> LoadGlyphs(TypefaceViewModel? typefaceViewModel, float fontSize, string color)
    {                    
        if (typefaceViewModel?.Typeface is null 
            || typefaceViewModel.CharacterMaps is null
            || typefaceViewModel.Glyphs is null)
        {
            yield break;
        }

        var skFont = typefaceViewModel.Typeface.ToFont(fontSize);

        foreach (var characterMap in typefaceViewModel.CharacterMaps)
        {
            if (characterMap.CharacterToGlyphMap != null)
            {
                var characterToGlyphMap = characterMap.CharacterToGlyphMap;

                foreach (var kvp in characterToGlyphMap)
                {
                    var charCode = kvp.Key;
                    var glyphIndex = kvp.Value;
                    var skPath = skFont.GetGlyphPath(glyphIndex);
                    if (skPath is null)
                    {
                        continue;
                    }

                    var skTranslationMatrix = SKMatrix.CreateTranslation(-skPath.Bounds.Left, -skPath.Bounds.Top);
                    skPath.Transform(skTranslationMatrix);

                    var svgPathData = skPath.ToSvgPathData();

                    var glyph = new GlyphViewModel(OpenGlyphDetail, CloseGlyphDetail)
                    {
                        CharCode = charCode,
                        GlyphIndex = glyphIndex,
                        Path = skPath,
                        Paint = new SKPaint
                        {
                            IsAntialias = true,
                            Color = SKColor.Parse(color),
                            Style = SKPaintStyle.Fill
                        },
                        Color = color,
                        SvgPathData = svgPathData
                    };

                    yield return glyph;
                }
            }
        }
    }

    private void OpenGlyphDetail(GlyphViewModel glyphViewModel)
    {
        SelectedGlyph = glyphViewModel;
    }

    private void CloseGlyphDetail(GlyphViewModel glyphViewModel)
    {
        SelectedGlyph = null;
    }

    private static List<CharacterMap> Read(SKTypeface typeface)
    {
        var cmap = typeface.GetTableData(TableReader.GetIntTag("cmap"));
        var characterMaps = TableReader.ReadCmapTable(cmap, false);
        return characterMaps;
    }

    private TypefaceViewModel? LoadFromFamilyName(string fontFamily)
    {
        if (!string.IsNullOrEmpty(fontFamily))
        {
            using var typeface = SKTypeface.FromFamilyName(fontFamily);
            if (typeface != null)
            {
                var characterMaps = Read(typeface);
                return new TypefaceViewModel()
                {
                    Typeface = typeface, 
                    CharacterMaps = characterMaps,
                    Glyphs = new ObservableCollection<GlyphViewModel>()
                };
            }
        }

        return null;
    }

    private TypefaceViewModel? LoadFromData(Stream? stream)
    {
        if (stream is { })
        {
            using var typeface = SKTypeface.FromStream(stream);
            if (typeface != null)
            {
                var characterMaps = Read(typeface);
                return new TypefaceViewModel()
                {
                    Typeface = typeface, 
                    CharacterMaps = characterMaps,
                    Glyphs = new ObservableCollection<GlyphViewModel>()
                };
            }
        }

        return null;
    }
}
