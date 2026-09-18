using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK.Graphics;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Tests.Visual.Sprites;

public partial class TestSceneSpriteText : SampleTestScene
{
    public TestSceneSpriteText()
    {
        FillFlowContainer flow;

        Children =
        [
                new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children =
                    [
                        flow = new FillFlowContainer
                        {
                            Anchor = Anchor.TopLeft,
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Direction = FillDirection.Vertical,
                        }
                    ]
                }
        ];

        flow.Add(new SpriteText
        {
            Text = @"the quick red fox jumps over the lazy brown 子犬🐶"
        });
        flow.Add(new SpriteText
        {
            Text = @"THE QUICK RED FOX JUMPS OVER THE LAZY BROWN 子犬🐶"
        });
        flow.Add(new SpriteText
        {
            Text = @"0123456789!@#$%^&*()_-+-[]{}.,<>;'\"
        });

        flow.Add(new Container
        {
            Margin = new MarginPadding { Vertical = 5 },
            AutoSizeAxes = Axes.Both,
            Children =
            [
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = Color4.Black,
                        UseFullGlyphHeight = true,
                        Text = "UseFullGlyphHeight = true",
                    },
            ]
        });

        flow.Add(new Container
        {
            Margin = new MarginPadding { Vertical = 5 },
            AutoSizeAxes = Axes.Both,
            Children =
            [
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = Color4.Black,
                        UseFullGlyphHeight = false,
                        Text = "UseFullGlyphHeight = false",
                    },
            ]
        });

        for (var i = 1; i <= 200; i++)
        {
            var text = new SpriteText
            {
                Text = $@"Font testy at size {i}",
                Font = new FontUsage("WorkSans", i, i % 4 > 1 ? "Bold" : "Regular", i % 2 == 1),
                AllowMultiline = true,
                RelativeSizeAxes = Axes.X,
            };

            flow.Add(text);
        }
    }
}
