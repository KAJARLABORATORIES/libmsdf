using kajarlabs.osu.Framework.MsdfTextRendering.IO.Stores;
using osu.Framework.Graphics.Textures;
using osu.Framework.Text;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Text;

public readonly struct MsdfTexturedCharacterGlyph : ITexturedCharacterGlyph
{
    public Texture Texture { get; }

    public float Width { get; }

    public float Height { get; }

    public float XOffset { get; }

    public float YOffset { get; }

    public float XAdvance { get; }

    public float Baseline { get; }

    public char Character { get; }

    private readonly MsdfGlyphSource source;

    internal MsdfTexturedCharacterGlyph(MsdfGlyphSource source, char character, MsdfGlyph glyph, Texture texture)
    {
        this.source = source;

        Character = character;
        Texture = texture;
    }

    public float GetKerning<T>(T lastGlyph)
        where T : ICharacterGlyph
    {
        throw new NotImplementedException();
    }
}
