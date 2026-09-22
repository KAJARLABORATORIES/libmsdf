# libmsdf
 
[![Build status](https://github.com/KAJARLABORATORIES/libmsdf/actions/workflows/release.yml/badge.svg?branch=main&event=push)](https://github.com/KAJARLABORATORIES/libmsdf/actions/workflows/release.yml)
[![NuGet](https://img.shields.io/nuget/vpre/kajarlabs.osu.Framework.MsdfTextRendering.svg)](https://www.nuget.org/packages/kajarlabs.osu.Framework.MsdfTextRendering)
[![GitHub release](https://img.shields.io/github/v/release/KAJARLABORATORIES/libmsdf?include_prereleases)](https://github.com/KAJARLABORATORIES/libmsdf/releases/latest)
 
MSDF text rendering for [osu!framework](https://github.com/ppy/osu-framework).
 
## Using libmsdf in your game
 
If you are interested in **using** the library, install the package and start from the [setting up MSDF fonts](https://github.com/KAJARLABORATORIES/libmsdf/wiki/Setting-Up-MSDF-Fonts) wiki page, which walks through generating an atlas, registering it and drawing text with it.
 
```
dotnet add package kajarlabs.osu.Framework.MsdfTextRendering --prerelease
```
 
The library is pre-`1.0.0`. The public API may change between `0.x` releases, so pin an exact version if you depend on it.
 
The rest of the information on this page is related to working *on* the library, not *using* it!
 
## Objectives
 
osu!framework renders text from pre-rasterised bitmap atlases. That is fast and predictable, but a glyph baked at one size degrades once it is drawn far beyond it, which shows up in any UI that zooms, animates scale, or shares a typeface across a wide range of sizes.
 
`libmsdf` provides a second rendering path built on [multi-channel signed distance fields](https://github.com/Chlumsky/msdfgen), where glyph coverage is reconstructed in the fragment shader instead of sampled from a fixed-resolution texture.
 
- Stay a **drop-in alternative**, not a replacement. `MsdfSpriteText` mirrors `SpriteText`, and MSDF glyphs implement the framework's own glyph abstractions so the existing text builder, layout and localisation keep working.
- Keep the atlas pipeline **external and standard**. Atlases are produced by [`msdf-atlas-gen`](https://github.com/Chlumsky/msdf-atlas-gen), so nothing here is a bespoke font format.
- Cover the same text features users already expect: kerning, multiline, truncation, fixed width, fallback glyphs and shadows.
MSDF is not a free upgrade for every case. At small, static sizes a hinted bitmap font is cheaper and often looks better; this library is aimed at the cases where one atlas has to serve many sizes.
 
## Requirements
 
- A desktop platform with the [.NET 10.0 SDK](https://dotnet.microsoft.com/download).
- [`msdf-atlas-gen`](https://github.com/Chlumsky/msdf-atlas-gen) for generating font atlases.
- When working with the codebase, we recommend an IDE with intellisense and syntax highlighting, such as [Visual Studio](https://visualstudio.microsoft.com/vs/), [JetBrains Rider](https://www.jetbrains.com/rider/), or [Visual Studio Code](https://code.visualstudio.com/) with the [EditorConfig](https://marketplace.visualstudio.com/items?itemName=EditorConfig.EditorConfig) and [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) extensions installed.

### Building
 
```
dotnet build
dotnet test
```
 
Visual test scenes live in `tests`, and are the recommended place to develop and debug rendering changes: a distance-field bug is something you see, not something an assertion catches.

## Contributing
 
Contributions can be made via pull requests to this repository. If you are unsure where to start, check the [list of open issues](https://github.com/KAJARLABORATORIES/libmsdf/issues).
 
When reporting a rendering bug, include a minimal reproduction and the atlas and JSON metadata that trigger it. "The glyph looks wrong" is impossible to act on without the distance range it was generated with.
 
## License
 
This library is licensed under the [MIT license](https://opensource.org/licenses/MIT). Please see [the license file](../COPYING) for more information. [tl;dr](https://tldrlegal.com/license/mit-license) you can do whatever you want as long as you include the original copyright and license notice in any copy of the software/source.
 
The MSDF technique itself comes from [msdfgen](https://github.com/Chlumsky/msdfgen) by Viktor Chlumský, also MIT licensed. Atlases generated with `msdf-atlas-gen` carry the license of the font they were generated from; check it before shipping one.
