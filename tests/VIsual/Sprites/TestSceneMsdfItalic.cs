using kajarlabs.osu.Framework.MsdfTextRendering.Graphics.Sprites;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Tests.Visual.Sprites;

public partial class TestSceneMsdfItalic : MsdfComparisonTestScene
{
    private const float demo_font_size = 28.0f;
    private const string demo_text = "MSDF Sample Aa Oo 123";

    protected override string LeftLabel => "SpriteText (without MSDF, bitmap WorkSans)";
    protected override string RightLabel => "MsdfSpriteText (with MSDF)";

    protected override Drawable CreateLeftSample() => createFlow(
        text => new SpriteText { Anchor = Anchor.TopCentre, Origin = Anchor.TopCentre, Text = text },
        (drawable, italics) => ((SpriteText)drawable).Font = new FontUsage("WorkSans", demo_font_size, "Regular", italics));

    protected override Drawable CreateRightSample() => createFlow(
        text => new MsdfSpriteText { Anchor = Anchor.TopCentre, Origin = Anchor.TopCentre, Text = text },
        (drawable, italics) => ((MsdfSpriteText)drawable).Font = new FontUsage("WorkSans", demo_font_size, "Regular", italics));

    private static Drawable createFlow(Func<string, Drawable> create, Action<Drawable, bool> setFont)
    {
        var upright = create($"Regular: {demo_text}");
        var italic = create($"Italic: {demo_text}");

        setFont(upright, false);
        setFont(italic, true);

        return new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Vertical,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Spacing = new Vector2(0, 16),
            Children = new[] { upright, italic },
        };
    }
}
