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
    private readonly float lineHeight;

    internal MsdfTexturedCharacterGlyph(MsdfGlyphSource source, MsdfGlyphSource metricsSource, char character, MsdfGlyph glyph, Texture texture)
    {
        this.source = source;

        Character = character;
        Texture = texture;

        this.lineHeight = metricsSource.LineHeight;
        var lineHeight = this.lineHeight;
        var planeBounds = glyph.PlaneBounds ?? default;

        Width = planeBounds.Width / lineHeight;
        Height = planeBounds.Height / lineHeight;
        XOffset = planeBounds.Left / lineHeight;
        YOffset = (planeBounds.Top - metricsSource.Ascender) / lineHeight;

        XAdvance = glyph.Advance / lineHeight;

        Baseline = -metricsSource.Ascender / lineHeight;
    }

    public float GetKerning<T>(T lastGlyph)
        where T : ICharacterGlyph
        => source.Kerning.TryGetValue((lastGlyph.Character, Character), out var kerning) ? kerning / lineHeight : 0;
}
