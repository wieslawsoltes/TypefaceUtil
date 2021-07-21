using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using SkiaSharp;
using TypefaceUtil.OpenType;

namespace TypefaceUtil.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string? _inputFile;
        private string? _familyName;
        private ObservableCollection<string>? _fontFamilies;
        private float _fontSize = 32f;
        private TypefaceViewModel? _typeface;

        public string? InputFile
        {
            get => _inputFile;
            set => this.RaiseAndSetIfChanged(ref _inputFile, value);
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

        public TypefaceViewModel? Typeface
        {
            get => _typeface;
            set => this.RaiseAndSetIfChanged(ref _typeface, value);
        }

        public ICommand InputFileCommand { get; }

        public ICommand LoadInputFileCommand { get; }

        public ICommand LoadFamilyNameCommand { get; }

        public MainWindowViewModel()
        {
            SetGetFontFamilies();

            InputFileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dlg = new OpenFileDialog();
                dlg.Filters.Add(new FileDialogFilter { Extensions = new List<string> {"ttf", "otf"}, Name = "Font Files"});
                dlg.Filters.Add(new FileDialogFilter { Extensions = new List<string> {"ttf"}, Name = "TTF Files"});
                dlg.Filters.Add(new FileDialogFilter { Extensions = new List<string> {"otf"}, Name = "OTF Files"});
                dlg.Filters.Add(new FileDialogFilter { Extensions = new List<string> {"*"}, Name = "All Files"});
                var paths = await dlg.ShowAsync((Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
                if (paths is { Length: 1 })
                {
                    InputFile = paths[0];
                }
            });
            
            LoadInputFileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await Task.Run(LoadInputFile);
            });

            LoadFamilyNameCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await Task.Run(LoadFamilyName);
            });
        }

        private void SetGetFontFamilies()
        {
            var fontFamilies = SKFontManager.Default.GetFontFamilies();

            Array.Sort(fontFamilies, StringComparer.InvariantCulture);

            _familyName = "Segoe MDL2 Assets";
            _fontFamilies = new ObservableCollection<string>(fontFamilies);
        }

        private void LoadInputFile()
        {
            var inputFile = InputFile;
            var fontSize = FontSize;
            if (string.IsNullOrEmpty(inputFile))
            {
                return;
            }

            var typefaceViewModel = LoadFromFile(inputFile);
            if (typefaceViewModel?.Typeface is null)
            {
                return;
            }

            Process(typefaceViewModel, fontSize);
            Typeface = typefaceViewModel;
        }

        private void LoadFamilyName()
        {
            var familyName = FamilyName;
            var fontSize = FontSize;
            if (string.IsNullOrEmpty(familyName))
            {
                return;
            }

            var typefaceViewModel = LoadFromFamilyName(familyName);
            if (typefaceViewModel?.Typeface is null)
            {
                return;
            }

            Process(typefaceViewModel, fontSize);
            Typeface = typefaceViewModel;
        }

        private void Process(TypefaceViewModel? typefaceViewModel, float fontSize)
        {                    
            if (typefaceViewModel?.Typeface is null || typefaceViewModel?.CharacterMaps is null || typefaceViewModel?.Glyphs is null)
            {
                return;
            }

            var skFont = typefaceViewModel.Typeface.ToFont(fontSize, 1f, 0f);

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
                        var svgPathData = skPath.ToSvgPathData();

                        var skTranslationMatrix = SKMatrix.CreateTranslation(-skPath.Bounds.Left, -skPath.Bounds.Top);
                        skPath.Transform(skTranslationMatrix);

                        var glyph = new GlyphViewModel()
                        {
                            CharCode = charCode,
                            GlyphIndex = glyphIndex,
                            Path = skPath,
                            Paint = new SKPaint
                            {
                                IsAntialias = true,
                                Color = SKColors.Black,
                                Style = SKPaintStyle.Fill
                            },
                            SvgPathData = svgPathData
                        };

                        typefaceViewModel.Glyphs.Add(glyph);
                    }
                }
            }
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

        private TypefaceViewModel? LoadFromFile(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                using var typeface = SKTypeface.FromFile(path);
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
}
