# libmsdf

[![NuGet](https://img.shields.io/nuget/vpre/kajarlabs.osu.Framework.MsdfTextRendering.svg)](https://www.nuget.org/packages/kajarlabs.osu.Framework.MsdfTextRendering)
[![License](https://img.shields.io/github/license/KAJARLABORATORIES/libmsdf.svg)](https://github.com/KAJARLABORATORIES/libmsdf/blob/main/COPYING)

MSDF text rendering for [osu!framework](https://github.com/ppy/osu-framework).

`libmsdf` replaces bitmap glyph rendering with [multi-channel signed distance fields](https://github.com/Chlumsky/msdfgen), so text stays sharp at any size or scale. Glyph coverage is reconstructed in the fragment shader instead of being baked into a texture at a fixed resolution.

> **Pre-release.** The current version is `0.1.0-alpha`. The public API is expected to change before `1.0.0`. Pin an exact version if you depend on it.

## Installation

```bash
dotnet add package kajarlabs.osu.Framework.MsdfTextRendering --prerelease
```

Or, as a `PackageReference`:

```xml
<PackageReference Include="kajarlabs.osu.Framework.MsdfTextRendering" Version="0.1.0-alpha.2" />
```

`--prerelease` is required while the package has no stable release: NuGet hides pre-release versions by default.

## Quick start

Register the MSDF resources and the fonts you want to use, then draw text with `MsdfSpriteText`:

```csharp
public partial class MsdfShowcase : Game
{
    private DependencyContainer dependencies = null!;

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        => dependencies = new DependencyContainer(parent);

    [BackgroundDependencyLoader]
    private void load()
    {
        MsdfTextRendering.CreateCapabilities(Resources);

        var msdfFonts = new MsdfFontStore(Host.Renderer, Shaders);

        msdfFonts.AddFont(Resources, "WorkSans", "Regular");
        msdfFonts.AddFont(Resources, "WorkSans", "Bold");

        dependencies.CacheAs(msdfFonts);

        Add(new MsdfSpriteText
        {
            Text = "Hello, MSDF!",
            Font = new FontUsage("WorkSans", 32, "Regular"),
        });
    }
}
```

`MsdfSpriteText` behaves like `SpriteText`: same layout model, same sizing rules, same localisation support. Only the glyph rendering path differs.

The fonts themselves are not shipped with the package. See the next section.

## Generating font atlases

An MSDF font is an atlas texture plus a JSON metadata file. Both are required, and both are generated with [`msdf-atlas-gen`](https://github.com/Chlumsky/msdf-atlas-gen):

```bash
msdf-atlas-gen -font WorkSans-Regular.ttf \
               -type msdf \
               -format png \
               -size 48 \
               -pxrange 4 \
               -yorigin top \
               -imageout WorkSans-Regular.png \
               -json WorkSans-Regular.json
```

| Option         | Why it matters                                                     |
| -------------- | ------------------------------------------------------------------ |
| `-type msdf`   | Multi-channel distance field. `sdf` will not work with the shader. |
| `-size 48`     | Em size used for generation. Larger means a larger atlas.          |
| `-pxrange 4`   | Distance range around glyph edges. Too low causes soft corners.    |
| `-yorigin top` | Matches the coordinate system osu!framework expects.               |

The JSON carries glyph metrics, atlas and plane bounds, kerning pairs, and the `distanceRange` the shader uses to reconstruct coverage.

Generate one pair per weight and style, and name the files after the family and style you pass to `AddFont`:

```text
Resources/Typefaces/
└── WorkSans/
    ├── WorkSans-Regular.png
    ├── WorkSans-Regular.json
    ├── WorkSans-Bold.png
    ├── WorkSans-Bold.json
    ├── WorkSans-RegularItalic.png
    └── WorkSans-RegularItalic.json
```

## What's supported

- Arbitrary sizes and scales without re-rasterisation
- Font families with weight and italic variants
- Kerning, baselines and glyph metrics
- Multiline text, truncation with ellipsis, fixed width
- Auto-sizing and maximum width
- Fallback glyphs
- Shadows
- Localisation
- Glyph lookup with caching

## API

### `MsdfSpriteText`

The drawable you use. Mirrors `SpriteText`.

```csharp
new MsdfSpriteText
{
    Text = "The quick brown fox jumps over the lazy dog.",
    Font = new FontUsage("WorkSans", 24, "Regular"),
    Shadow = true,
}
```

### `MsdfFontStore`

Resolves families and styles to glyphs, and caches them. Register fonts once, at load time:

```csharp
msdfFonts.AddFont(Resources, "WorkSans", "Regular");
msdfFonts.AddFont(Resources, "WorkSans", "Bold");
msdfFonts.AddFont(Resources, "WorkSans", "RegularItalic");
```

Cache it with `CacheAs` so `MsdfSpriteText` can resolve it through dependency injection.

## Shaders

The package ships two fragment shaders:

| Shader                        | Use                                                       |
| ----------------------------- | --------------------------------------------------------- |
| `sh_MsdfGlyph.fs`             | Default. Anti-aliased coverage from the distance field.    |
| `sh_MsdfGlyphHardThreshold.fs`| Hard cutoff, no anti-aliasing. Useful for pixel-style UI.  |

## Contributing

Issues and pull requests are welcome. For bug reports, include a minimal reproduction and, where relevant, the atlas and JSON metadata that trigger the problem.

## License

MIT. See [COPYING](https://github.com/KAJARLABORATORIES/libmsdf/blob/main/COPYING).
