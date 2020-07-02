using System.Collections.Generic;

namespace TypefaceUtil.OpenType
{
    public class CharacterMap
    {
        public string? Name { get; set; }
        public Dictionary<int, ushort>? CharacterToGlyphMap { get; set; }
    }
}
