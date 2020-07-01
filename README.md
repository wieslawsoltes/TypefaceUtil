# TypefaceUtil

### Run

```
dotnet run -c Release -- ../../segoeui.ttf
dotnet run -c Release -- ../../seguisym.ttf
dotnet run -c Release -- ../../calibri.ttf
dotnet run -c Release -- ../../fa-brands-400.ttf
dotnet run -c Release -- ../../fa-regular-400.ttf
dotnet run -c Release -- ../../fa-solid-900.ttf
```

```
dotnet run -c Release -- ../../segoeui.ttf > segui.txt
dotnet run -c Release -- ../../seguisym.ttf > seguisym.txt
dotnet run -c Release -- ../../calibri.ttf > calibri.txt
```

```
dotnet run -c Release -- "Segoe UI"
dotnet run -c Release -- "Segoe UI Symbol"
dotnet run -c Release -- "Calibri"
```

### References

* https://docs.microsoft.com/en-us/typography/opentype/spec/cmap
* https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6cmap.html
* https://docs.microsoft.com/en-us/dotnet/api/skiasharp.sktypeface?view=skiasharp-1.68.1
* https://opentype.js.org/glyph-inspector.html
* https://opentype.js.org/font-inspector.html
* https://fontdrop.info/
* https://github.com/opentypejs/opentype.js/blob/master/src/tables/cmap.js
* https://github.com/LayoutFarm/Typography/blob/master/Typography.OpenFont/Tables/Cmap.cs
* https://github.com/LayoutFarm/Typography/blob/master/Typography.OpenFont/Tables/CharacterMap.cs
* https://fontawesome.com/how-to-use/on-the-web/setup/hosting-font-awesome-yourself
