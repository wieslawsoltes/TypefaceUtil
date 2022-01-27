using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;
using SkiaSharp;
using TypefaceUtil.OpenType;

namespace TypefaceUtil.Avalonia.ViewModels;

public class TypefaceViewModel : ViewModelBase
{
    private SKTypeface? _typeface;
    private List<CharacterMap>? _characterMaps;
    private ObservableCollection<GlyphViewModel>? _glyphs;

    public SKTypeface? Typeface
    {
        get => _typeface;
        set => this.RaiseAndSetIfChanged(ref _typeface, value);
    }

    public List<CharacterMap>? CharacterMaps
    {
        get => _characterMaps;
        set => this.RaiseAndSetIfChanged(ref _characterMaps, value);
    }

    public ObservableCollection<GlyphViewModel>? Glyphs
    {
        get => _glyphs;
        set => this.RaiseAndSetIfChanged(ref _glyphs, value);
    }
}