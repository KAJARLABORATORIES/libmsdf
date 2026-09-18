using System.Text.Json;
using System.Text.Json.Serialization;
using kajarlabs.osu.Framework.MsdfTextRendering.Text;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Text;

namespace kajarlabs.osu.Framework.MsdfTextRendering.IO.Stores;

internal sealed class MsdfGlyphSource : IDisposable
{
    public Texture Atlas
    {
        get
        {
            ensureLoaded();
            return field;
        }
        private set;
    } = null!;

    public IShader Shader
    {
        get
        {
            ensureLoaded();
            return field;
        }
        private set;
    } = null!;

    public float Ascender
    {
        get
        {
            ensureLoaded();
            return field;
        }
        private set;
    }

    public float Descender
    {
        get
        {
            ensureLoaded();
            return field;
        }
        private set;
    }

    public float LineHeight
    {
        get
        {
            ensureLoaded();
            return field;
        }
        private set;
    }

    public float DistanceRange
    {
        get
        {
            ensureLoaded();
            return field;
        }
        private set;
    }

    public string Family { get; }

    public string FontName { get; }

    public IReadOnlyDictionary<char, MsdfGlyph> Glyphs
    {
        get
        {
            ensureLoaded();
            return field;
        }
        private set;
    } = new Dictionary<char, MsdfGlyph>();

    public IReadOnlyDictionary<(char First, char Second), float> Kerning
    {
        get
        {
            ensureLoaded();
            return field;
        }
        private set;
    } = new Dictionary<(char, char), float>();

    private readonly IRenderer renderer;
    private readonly ShaderManager shaders;
    private readonly IResourceStore<byte[]> store;
    private readonly string weight;

    private readonly TaskCompletionSource<bool> loadCompletionSource = new();

    private TextureStore? textures;

    public MsdfGlyphSource(IRenderer renderer, ShaderManager shaders, IResourceStore<byte[]> store, string family, string weight)
    {
        this.renderer = renderer;
        this.shaders = shaders;
        this.store = store;
        this.weight = weight;

        Family = family;
        FontName = string.IsNullOrEmpty(weight) ? family : $"{family}-{weight}";
    }

    public void Load()
    {
        textures = new TextureStore(
            renderer,
            new TextureLoaderStore(store),
            useAtlas: false,
            filteringMode: TextureFilteringMode.Linear,
            manualMipmaps: true
        );

        var texturePath = $@"Typefaces/{Family}/{Family}-{weight}.png";
        var jsonPath = $@"Typefaces/{Family}/{Family}-{weight}.json";

        Atlas = textures.Get(texturePath);
        Shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "MsdfGlyph");

        var json = store.Get(jsonPath) ?? throw new InvalidOperationException($"Could not find embedded resource '{jsonPath}'.");
        var atlasJson = parseAtlasJson(json);

        Ascender = atlasJson.Metrics.Ascender;
        Descender = atlasJson.Metrics.Descender;
        LineHeight = atlasJson.Metrics.LineHeight;
        DistanceRange = atlasJson.Atlas.DistanceRange;

        var newGlyphs = new Dictionary<char, MsdfGlyph>();

        foreach (var glyph in atlasJson.Glyphs)
        {
            newGlyphs[(char)glyph.Unicode] = new MsdfGlyph(
                glyph.Advance,
                toRectangle(glyph.PlaneBounds),
                toRectangle(glyph.AtlasBounds)
            );
        }

        Glyphs = newGlyphs;

        var newKerning = new Dictionary<(char, char), float>();

        foreach (var pair in atlasJson.Kerning)
            newKerning[((char)pair.Unicode1, (char)pair.Unicode2)] = pair.Advance;

        Kerning = newKerning;

        loadCompletionSource.SetResult(true);
    }

    public ITexturedCharacterGlyph? Get(char character)
    {
        ensureLoaded();

        if (!Glyphs.TryGetValue(character, out var glyph))
        {
            Logger.Log($"{nameof(MsdfGlyphSource)}: no glyph for character '{character}' (U+{(int)character:X4}) in atlas '{FontName}' -- skipping.", LoggingTarget.Runtime, LogLevel.Debug);
            return null;
        }

        var texture = glyph.AtlasBounds is RectangleF atlasBound ? Atlas.Crop(atlasBound) : Atlas;
        return new MsdfTexturedCharacterGlyph(this, character, glyph, texture);
    }

    public static float ReadDistanceRange(byte[] json)
        => parseAtlasJson(json).Atlas.DistanceRange;

    private void ensureLoaded()
        => loadCompletionSource.Task.GetResultSafely();

    private static AtlasJson parseAtlasJson(byte[] json)
        => JsonSerializer.Deserialize<AtlasJson>(json) ?? throw new InvalidOperationException("Failed to parse MSDF atlas JSON.");

    private static RectangleF? toRectangle(BoundsJson? bounds)
        => bounds == null ? null : new RectangleF(bounds.Left, bounds.Top, bounds.Right - bounds.Left, bounds.Bottom - bounds.Top);

    public void Dispose()
        => textures?.Dispose();

    private sealed class AtlasJson
    {
        [JsonPropertyName("atlas")]
        public AtlasMetaJson Atlas { get; set; } = new AtlasMetaJson();

        [JsonPropertyName("metrics")]
        public MetricsJson Metrics { get; set; } = new MetricsJson();

        [JsonPropertyName("glyphs")]
        public List<GlyphJson> Glyphs { get; set; } = new List<GlyphJson>();

        [JsonPropertyName("kerning")]
        public List<KerningJson> Kerning { get; set; } = new List<KerningJson>();
    }

    private sealed class AtlasMetaJson
    {
        [JsonPropertyName("distanceRange")]
        public float DistanceRange { get; set; }
    }

    private sealed class MetricsJson
    {
        [JsonPropertyName("ascender")]
        public float Ascender { get; set; }

        [JsonPropertyName("descender")]
        public float Descender { get; set; }

        [JsonPropertyName("lineHeight")]
        public float LineHeight { get; set; }
    }

    private sealed class GlyphJson
    {
        [JsonPropertyName("unicode")]
        public int Unicode { get; set; }

        [JsonPropertyName("advance")]
        public float Advance { get; set; }

        [JsonPropertyName("planeBounds")]
        public BoundsJson? PlaneBounds { get; set; }

        [JsonPropertyName("atlasBounds")]
        public BoundsJson? AtlasBounds { get; set; }
    }

    private sealed class BoundsJson
    {
        [JsonPropertyName("left")]
        public float Left { get; set; }

        [JsonPropertyName("top")]
        public float Top { get; set; }

        [JsonPropertyName("right")]
        public float Right { get; set; }

        [JsonPropertyName("bottom")]
        public float Bottom { get; set; }
    }

    private sealed class KerningJson
    {
        [JsonPropertyName("unicode1")]
        public int Unicode1 { get; set; }

        [JsonPropertyName("unicode2")]
        public int Unicode2 { get; set; }

        [JsonPropertyName("advance")]
        public float Advance { get; set; }
    }
}
