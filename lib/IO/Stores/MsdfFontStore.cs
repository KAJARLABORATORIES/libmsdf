using osu.Framework.Text;

namespace kajarlabs.osu.Framework.MsdfTextRendering.IO.Stores;

public class MsdfFontStore : IDisposable, ITexturedGlyphLookupStore
{
    public ITexturedCharacterGlyph? Get(string? fontName, char character)
    {
        throw new NotImplementedException();
    }

    public Task<ITexturedCharacterGlyph?> GetAsync(string fontName, char character)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
