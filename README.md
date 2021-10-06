# TypefaceUtil

[![Build status](https://dev.azure.com/wieslawsoltes/GitHub/_apis/build/status/Sources/TypefaceUtil)](https://dev.azure.com/wieslawsoltes/GitHub/_build/latest?definitionId=87)
![CI](https://github.com/wieslawsoltes/TypefaceUtil/workflows/CI/badge.svg)

[![NuGet](https://img.shields.io/nuget/v/TypefaceUtil.OpenType.svg)](https://www.nuget.org/packages/TypefaceUtil.OpenType)
[![NuGet](https://img.shields.io/nuget/dt/TypefaceUtil.OpenType.svg)](https://www.nuget.org/packages/TypefaceUtil.OpenType)

[![GitHub release](https://img.shields.io/github/release/wieslawsoltes/TypefaceUtil.svg)](https://github.com/wieslawsoltes/TypefaceUtil)
[![Github All Releases](https://img.shields.io/github/downloads/wieslawsoltes/TypefaceUtil/total.svg)](https://github.com/wieslawsoltes/TypefaceUtil)
[![Github Releases](https://img.shields.io/github/downloads/wieslawsoltes/TypefaceUtil/latest/total.svg)](https://github.com/wieslawsoltes/TypefaceUtil)

An OpenType typeface utilities.

## About

TypefaceUtil is a set of OpenType typeface utilities.
Currently supported are `cmap` table format parser for `character to glyph index mapping`, 
generation of character `png` map, `svg` and `xaml` export for glyphs.

![4dsZT5hb3a](https://user-images.githubusercontent.com/2297442/126555807-1acf614d-a44e-40fc-bd60-5b0d6df4a10d.jpg)

![mehzNi5G2T](https://user-images.githubusercontent.com/2297442/126555801-c63a3c0a-1092-451b-9c79-a964f4d7dfe7.jpg)

## Usage

```
TypefaceUtil:
  An OpenType typeface utilities.

Usage:
  TypefaceUtil [options]

Options:
  -f, --inputFiles <inputfiles>              The relative or absolute path to the input files
  -d, --inputDirectory <inputdirectory>      The relative or absolute path to the input directory
  -p, --pattern <pattern>                    The search string to match against the names of files in the input directory [default: *.ttf]
  --fontFamily <fontfamily>                  The input font family
  -o, --outputDirectory <outputdirectory>    The relative or absolute path to the output directory
  --zip                                      Create zip archive from exported files
  --zipFile <zipfile>                        The relative or absolute path to the zip file [default: export.zip]
  --printFontFamilies                        Print available font families
  --printCharacterMaps                       Print character maps info
  --png, --pngExport                         Export text as Png
  --pngTextSize <pngtextsize>                Png text size [default: 20]
  --pngCellSize <pngcellsize>                Png cell size [default: 40]
  --pngColumns <pngcolumns>                  Png number of columns [default: 20]
  --svg, --svgExport                         Export text as Svg
  --svgTextSize <svgtextsize>                Svg text size [default: 16]
  --svgPathFill <svgpathfill>                Svg path fill [default: black]
  --xaml, --xamlExport                       Export text as Xaml
  --xamlTextSize <xamltextsize>              Xaml text size [default: 16]
  --xamlBrush <xamlbrush>                    Xaml brush [default: Black]
  --quiet                                    Set verbosity level to quiet
  --debug                                    Set verbosity level to debug
  --version                                  Show version information
  -?, -h, --help                             Show help and usage information
```

```
TypefaceUtil -h
```
```
TypefaceUtil --pngExport -f segoeui.ttf
TypefaceUtil --pngExport --fontFamily "Segoe UI"
```
```
TypefaceUtil --svgExport -f seguisym.ttf
TypefaceUtil --svgExport --fontFamily "Segoe UI Symbol"
```
```
TypefaceUtil --xamlExport -f calibri.ttf
TypefaceUtil --xamlExport --fontFamily "Calibri"
```

```
TypefaceUtil -d C:\Windows\Fonts --png --svg --xaml -o export
TypefaceUtil -d C:\Windows\Fonts --png --svg --xaml --zip --zipFile "Windows-Fonts-IconPack.zip"
```

## Build

```
dotnet build
```

## References

* https://docs.microsoft.com/en-us/typography/opentype/spec/cmap
* https://docs.microsoft.com/en-us/dotnet/api/skiasharp.sktypeface?view=skiasharp-1.68.1

## License

TypefaceUtil is licensed under the [MIT license](LICENSE.TXT).
