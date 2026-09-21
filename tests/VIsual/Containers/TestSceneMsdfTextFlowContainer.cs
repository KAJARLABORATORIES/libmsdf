using kajarlabs.osu.Framework.MsdfTextRendering.Graphics.Containers;
using kajarlabs.osu.Framework.MsdfTextRendering.Graphics.Sprites;
using kajarlabs.osu.Framework.MsdfTextRendering.Tests.Visual.Sprites;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osuTK;
using osuTK.Graphics;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Tests.Visual.Containers;

public partial class TestSceneMsdfTextFlowContainer : MsdfComparisonTestScene
{
    private const float demo_font_size = 20.0f;
    private const float flow_width = 380.0f;

    private const string multi_paragraph_text =
        "The quick brown fox jumps over the lazy dog. A swift movement across the tranquil meadow.\n\n" +
        "Second paragraph with more descriptive text to demonstrate multi-line wrapping and paragraph spacing.\n" +
        "This line follows a single newline break.";

    private const string cjk_and_urls_text =
        "Visit https://osu.ppy.sh/home or browse \\local\\storage\\path. " +
        "We also support hyphen-based and anti-aliased text splitting. " +
        "CJK support: こんにちは世界！(Hello World) and 日本語の文章です。";

    private static readonly FontUsage default_fu = FontUsage.Default.With(family: "WorkSans", size: demo_font_size, weight: "Regular");

    private TextFlowContainer leftFlow = null!;
    private MsdfTextFlowContainer rightFlow = null!;

    protected override string LeftLabel => "TextFlowContainer (without MSDF, bitmap WorkSans)";
    protected override string RightLabel => "MsdfTextFlowContainer (with MSDF)";

    protected override Drawable CreateLeftSample() => new Container
    {
        AutoSizeAxes = Axes.Both,
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        Child = leftFlow = new TextFlowContainer(s => s.Font = default_fu)
        {
            Width = flow_width,
            AutoSizeAxes = Axes.Y,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        }
    };

    protected override Drawable CreateRightSample() => new Container
    {
        AutoSizeAxes = Axes.Both,
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        Child = rightFlow = new MsdfTextFlowContainer(s => s.Font = default_fu)
        {
            Width = flow_width,
            AutoSizeAxes = Axes.Y,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        }
    };

    protected override void LoadComplete()
    {
        base.LoadComplete();

        AddStep("Default multi-paragraph", () =>
        {
            resetProperties();
            leftFlow.Text = multi_paragraph_text;
            rightFlow.Text = multi_paragraph_text;
        });

        AddStep("AddText (\\n is paragraph)", () =>
        {
            resetProperties();
            leftFlow.Clear();
            rightFlow.Clear();

            leftFlow.AddText("Line 1\nLine 2\nLine 3");
            rightFlow.AddText("Line 1\nLine 2\nLine 3");

            Schedule(logFlowMetrics);
        });

        AddStep("AddParagraph (\\n is line break)", () =>
        {
            resetProperties();
            leftFlow.Clear();
            rightFlow.Clear();

            leftFlow.AddParagraph("Line 1\nLine 2\nLine 3");
            rightFlow.AddParagraph("Line 1\nLine 2\nLine 3");
        });

        foreach (var anchor in new[]
        {
            Anchor.TopLeft, Anchor.TopCentre, Anchor.TopRight,
            Anchor.CentreLeft, Anchor.Centre, Anchor.CentreRight,
            Anchor.BottomLeft, Anchor.BottomCentre, Anchor.BottomRight
        })
        {
            AddStep($"Anchor: {anchor}", () =>
            {
                leftFlow.TextAnchor = anchor;
                rightFlow.TextAnchor = anchor;
            });
        }

        AddStep("Indents: FirstLine=40, Content=20", () =>
        {
            resetProperties();
            leftFlow.FirstLineIndent = 40;
            leftFlow.ContentIndent = 20;
            rightFlow.FirstLineIndent = 40;
            rightFlow.ContentIndent = 20;
            leftFlow.Text = multi_paragraph_text;
            rightFlow.Text = multi_paragraph_text;
        });

        AddStep("Spacing: Para=2.0, Line=10", () =>
        {
            resetProperties();
            leftFlow.ParagraphSpacing = 2.0f;
            leftFlow.LineSpacing = 10f;
            rightFlow.ParagraphSpacing = 2.0f;
            rightFlow.LineSpacing = 10f;
            leftFlow.Text = multi_paragraph_text;
            rightFlow.Text = multi_paragraph_text;
        });

        AddStep("URLs, Hyphens & CJK", () =>
        {
            resetProperties();
            leftFlow.Text = cjk_and_urls_text;
            rightFlow.Text = cjk_and_urls_text;
        });

        AddStep("Mixed manual instances", () =>
        {
            resetProperties();
            leftFlow.Clear();
            rightFlow.Clear();

            leftFlow.AddText("Before manual: ");
            leftFlow.AddText(new SpriteText
            {
                Text = "[MANUAL INSTANCE]",
                Colour = Color4.Gold,
                Font = default_fu
            });
            leftFlow.AddText(" - After manual text continuation.");

            rightFlow.AddText("Before manual: ");
            rightFlow.AddText(new MsdfSpriteText
            {
                Text = "[MANUAL INSTANCE]",
                Colour = Color4.Gold,
                Font = default_fu
            });
            rightFlow.AddText(" - After manual text continuation.");
        });

        AddStep("Baseline alignment (mixed sizes)", () =>
        {
            resetProperties();
            leftFlow.Clear();
            rightFlow.Clear();

            leftFlow.AddText("Normal (20) ", s => s.Font = default_fu.With(size: 20));
            leftFlow.AddText("Huge (36) ", s => s.Font = default_fu.With(size: 36));
            leftFlow.AddText("Small (14) ", s => s.Font = default_fu.With(size: 14));
            leftFlow.AddText("on same baseline.");

            rightFlow.AddText("Normal (20) ", s => s.Font = default_fu.With(size: 20));
            rightFlow.AddText("Huge (36) ", s => s.Font = default_fu.With(size: 36));
            rightFlow.AddText("Small (14) ", s => s.Font = default_fu.With(size: 14));
            rightFlow.AddText("on same baseline.");
        });
    }

    private void logFlowMetrics()
    {
        Logger.Log($"--- left (native) flow.Height={leftFlow.DrawHeight:F3} ---");
        foreach (var c in leftFlow.Children)
            Logger.Log($"  {c.GetType().Name,-20} '{(c as SpriteText)?.Text}' Y={c.Y,8:F3} H={c.DrawHeight,8:F3}");

        Logger.Log($"--- right (msdf) flow.Height={rightFlow.DrawHeight:F3} ---");
        foreach (var c in rightFlow.Children)
            Logger.Log($"  {c.GetType().Name,-20} '{(c as MsdfSpriteText)?.Text}' Y={c.Y,8:F3} H={c.DrawHeight,8:F3}");
    }

    private void resetProperties()
    {
        leftFlow.TextAnchor = Anchor.TopLeft;
        rightFlow.TextAnchor = Anchor.TopLeft;

        leftFlow.FirstLineIndent = 0;
        rightFlow.FirstLineIndent = 0;

        leftFlow.ContentIndent = 0;
        rightFlow.ContentIndent = 0;

        leftFlow.ParagraphSpacing = 0.5f;
        rightFlow.ParagraphSpacing = 0.5f;

        leftFlow.LineSpacing = 0;
        rightFlow.LineSpacing = 0;

        leftFlow.Spacing = Vector2.Zero;
        rightFlow.Spacing = Vector2.Zero;
    }
}

