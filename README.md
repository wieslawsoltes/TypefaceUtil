# TypefaceUtil

An OpenType typeface utilities.

## About

TypefaceUtil is a set of OpenType typeface utilities.
Currently supported are `cmap` table format parser for `character to glyph index mapping`, 
generation of character `png` map, `svg` and `xaml` export for glyphs.

## Usage

```
dotnet run -c Release -- ../../segoeui.ttf
dotnet run -c Release -- ../../seguisym.ttf
dotnet run -c Release -- ../../calibri.ttf
```

```
dotnet run -c Release -- "Segoe UI"
dotnet run -c Release -- "Segoe UI Symbol"
dotnet run -c Release -- "Calibri"
```

## References

* https://docs.microsoft.com/en-us/typography/opentype/spec/cmap
* https://docs.microsoft.com/en-us/dotnet/api/skiasharp.sktypeface?view=skiasharp-1.68.1

## License

TypefaceUtil is licensed under the [MIT license](LICENSE.TXT).
