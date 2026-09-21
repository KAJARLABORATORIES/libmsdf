using kajarlabs.osu.Framework.MsdfTextRendering.Graphics.Sprites;
using kajarlabs.osu.Framework.MsdfTextRendering.Tests.Visual;
using NUnit.Framework;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osuTK.Graphics;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Tests.Sprites;

[HeadlessTest]
public partial class TestSceneMsdfSpriteTextPresence : SampleTestScene
{
    [Test]
    public void TestNormalSpriteText()
    {
        Container container = null!;
        MsdfSpriteText text = null!;

        AddStep("reset", () =>
        {
            Child = container = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Children =
                [
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Red.Opacity(0.3f)
                        },
                        text = new MsdfSpriteText
                        {
                            Text = "Hello world!",
                            Font = new FontUsage("WorkSans", 12, "Regular"),
                        }
                ]
            };
        });

        AddAssert("is present", () => text.IsPresent);
        AddAssert("height == 12", () => Precision.AlmostEquals(12, container.Height));
        AddStep("empty text", () => text.Text = string.Empty);
        AddAssert("not present", () => !text.IsPresent);
        AddAssert("height == 0", () => Precision.AlmostEquals(0, container.Height));
    }

    [Test]
    public void TestAlwaysPresentSpriteText()
    {
        Container container = null!;
        MsdfSpriteText text = null!;

        AddStep("reset", () =>
        {
            Child = container = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Children =
                [
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Red.Opacity(0.3f)
                        },
                        text = new AlwaysPresentSpriteText
                        {
                            Text = "Hello world!",
                            Font = new FontUsage(size: 12),
                        }
                ]
            };
        });

        AddAssert("is present", () => text.IsPresent);
        AddAssert("height == 12", () => Precision.AlmostEquals(12, container.Height));
        AddStep("empty text", () => text.Text = string.Empty);
        AddAssert("is present", () => text.IsPresent);
        AddAssert("height == 0", () => Precision.AlmostEquals(0, container.Height));
    }

    [Test]
    public void TestPresenceRemainsTheSameDuringFlow()
    {
        AddStep("reset", () =>
        {
            Child = new FillFlowContainer
            {
                Child = new MsdfSpriteText()
            };
        });

        AddWaitStep("wait for some update frames", 2);
    }

    private partial class AlwaysPresentSpriteText : MsdfSpriteText
    {
        public override bool IsPresent => true;
    }
}
