using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;
using SkiaSharp;
using TypefaceUtil.OpenType;

namespace TypefaceUtil.Avalonia.ViewModels;

public partial class TypefaceViewModel : ViewModelBase
{
    [Reactive]
    public partial SKTypeface? Typeface { get; set; }
    
    [Reactive]
    public partial List<CharacterMap>? CharacterMaps { get; set; }
    
    [Reactive]
    public partial ObservableCollection<GlyphViewModel>? Glyphs { get; set; }
}
