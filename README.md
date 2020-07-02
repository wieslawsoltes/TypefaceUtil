# TypefaceUtil

An OpenType typeface utilities.

## About

TypefaceUtil is a set of OpenType typeface utilities.
Currently supported are `cmap` table format parser for `character to glyph index mapping`, 
generation of character `png` map, `svg` and `xaml` export for glyphs.

## Usage

```
TypefaceUtil:
  An OpenType typeface utilities.

Usage:
  TypefaceUtil [options]

Options:
  -f, --inputFiles <inputfiles>              The relative or absolute path to the input files
  -d, --inputDirectory <inputdirectory>      The relative or absolute path to the input directory
  -p, --pattern <pattern>                    The search string to match against the names of files in the input directory
                                             [default: *.ttf]
  -ff, --fontFamily <fontfamily>             The input font family
  -o, --outputDirectory <outputdirectory>    The relative or absolute path to the output directory
  -png, --pngExport                          Export text as Png
  --pngTextSize <pngtextsize>                Png text size [default: 20]
  --pngCellSize <pngcellsize>                Png cell size [default: 40]
  --pngColumns <pngcolumns>                  Png number of columns [default: 20]
  -svg, --svgExport                          Export text as Svg
  --svgTextSize <svgtextsize>                Svg text size [default: 16]
  --svgPathFill <svgpathfill>                Svg path fill [default: black]
  -xaml, --xamlExport                        Export text as Xaml
  --xamlTextSize <xamltextsize>              Xaml text size [default: 16]
  --xamlBrush <xamlbrush>                    Xaml brush [default: Black]
  --quiet                                    Set verbosity level to quiet
  --version                                  Show version information
  -?, -h, --help                             Show help and usage information
```

```
TypefaceUtil -h
```
```
TypefaceUtil --pngExport -f segoeui.ttf
TypefaceUtil --pngExport -ff "Segoe UI"
```
```
TypefaceUtil --svgExport -f seguisym.ttf
TypefaceUtil --svgExport -ff "Segoe UI Symbol"
```
```
TypefaceUtil --xamlExport -f calibri.ttf
TypefaceUtil --xamlExport -ff "Calibri"
```

## Testing

```
dotnet run -c Release -- --help
```

```
dotnet run -c Release -- --pngExport --svgExport --xamlExport -f ../../../segoeui.ttf
dotnet run -c Release -- --pngExport --svgExport --xamlExport -f ../../../seguisym.ttf
dotnet run -c Release -- --pngExport --svgExport --xamlExport -f ../../../calibri.ttf
```

```
dotnet run -c Release -- --pngExport --svgExport --xamlExport -f ../../../segoeui.ttf --pngColumns 20 --pngTextSize 50 --pngCellSize 70
```

```
dotnet run -c Release -- --pngExport --svgExport --xamlExport -ff "Segoe UI"
dotnet run -c Release -- --pngExport --svgExport --xamlExport -ff "Segoe UI Symbol"
dotnet run -c Release -- --pngExport --svgExport --xamlExport -ff "Calibri"
```

## References

* https://docs.microsoft.com/en-us/typography/opentype/spec/cmap
* https://docs.microsoft.com/en-us/dotnet/api/skiasharp.sktypeface?view=skiasharp-1.68.1

## License

TypefaceUtil is licensed under the [MIT license](LICENSE.TXT).
