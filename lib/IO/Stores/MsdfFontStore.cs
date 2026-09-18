using System.Collections.Concurrent;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Text;

namespace kajarlabs.osu.Framework.MsdfTextRendering.IO.Stores;

public class MsdfFontStore : IDisposable, ITexturedGlyphLookupStore
{
    private readonly IRenderer renderer;
    private readonly ShaderManager shaders;

    private readonly List<MsdfGlyphSource> sources = [];
    private readonly ConcurrentDictionary<(string, char), ITexturedCharacterGlyph?> namespacedGlyphCache = [];

    private Task? childStoreLoadTasks;

    public MsdfFontStore(IRenderer renderer, ShaderManager shaders)
    {
        this.renderer = renderer;
        this.shaders = shaders;
    }

    public void AddFont(IResourceStore<byte[]> store, string family, string weight)
    {
        var source = new MsdfGlyphSource(renderer, shaders, store, family, weight);

        sources.Add(source);
        queueLoad(source);
    }

    public ITexturedCharacterGlyph? Get(string? fontName, char character)
    {
        var key = (fontName ?? string.Empty, character);

        if (namespacedGlyphCache.TryGetValue(key, out var existing))
            return existing;

        var source = tryGetSource(fontName);
        return namespacedGlyphCache[key] = source?.Get(character);
    }

    public float? GetDistanceRange(string? fontName)
        => tryGetSource(fontName)?.DistanceRange;

    public IShader? GetShader(string? fontName)
        => tryGetSource(fontName)?.Shader;

    ITexturedCharacterGlyph? ITexturedGlyphLookupStore.Get(string? fontName, char character)
        => Get(fontName, character);

    Task<ITexturedCharacterGlyph?> ITexturedGlyphLookupStore.GetAsync(string fontName, char character)
        => Task.FromResult(Get(fontName, character));

    public static float ReadDistanceRange(byte[] json)
        => MsdfGlyphSource.ReadDistanceRange(json);

    private MsdfGlyphSource? tryGetSource(string? fontName)
    {
        var name = fontName ?? string.Empty;

        foreach (var source in sources)
        {
            if (source.FontName.EndsWith(name, StringComparison.Ordinal))
                return source;
        }

        foreach (var source in sources)
        {
            if (source.Family.Equals(name, StringComparison.Ordinal))
                return source;
        }

        return null;
    }

    private void queueLoad(MsdfGlyphSource source)
    {
        var previousLoadStream = childStoreLoadTasks;

        childStoreLoadTasks = Task.Run(async () =>
        {
            if (previousLoadStream != null)
                await previousLoadStream.ConfigureAwait(false);

            try
            {
                Logger.Log($"Loading Typeface {source.FontName}...", level: LogLevel.Debug);
                source.Load();
                Logger.Log($"Loaded Typeface {source.FontName}!", level: LogLevel.Debug);
            }
            catch
            {
            }
        });
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
