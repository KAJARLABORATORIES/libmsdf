using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Tests.Visual.Sprites;

public abstract partial class MsdfComparisonTestScene : SampleTestScene
{
    private const float top_bar_height = 40;
    private const float min_zoom = 0.5f;
    private const float max_zoom = 32f;

    private readonly BindableNumber<float> zoom = new(1f)
    {
        MinValue = min_zoom,
        MaxValue = max_zoom,
        Precision = 0.01f,
    };

    private readonly Bindable<Vector2> pan = new();

    private SpriteText zoomValueText = null!;

    protected abstract string LeftLabel { get; }
    protected abstract string RightLabel { get; }

    protected abstract Drawable CreateLeftSample();
    protected abstract Drawable CreateRightSample();

    [BackgroundDependencyLoader]
    private void load()
    {
        Children =
        [
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.MidnightBlue,
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Top = top_bar_height },
                Children =
                [
                    new ComparisonColumn(LeftLabel, CreateLeftSample(), zoom, pan)
                    {
                        RelativeSizeAxes = Axes.Both,
                        RelativePositionAxes = Axes.X,
                        Width = 0.5f,
                        Padding = new MarginPadding { Top = 8 },
                    },
                    new ComparisonColumn(RightLabel, CreateRightSample(), zoom, pan)
                    {
                        RelativeSizeAxes = Axes.Both,
                        RelativePositionAxes = Axes.X,
                        Width = 0.5f,
                        X = 0.5f,
                        Padding = new MarginPadding { Top = 8 },
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.Y,
                        RelativePositionAxes = Axes.X,
                        Width = 2,
                        X = 0.5f,
                        Origin = Anchor.TopCentre,
                        Colour = Color4.White,
                        Alpha = 0.25f,
                    },
                ],
            },
            new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = top_bar_height,
                Children =
                [
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.6f,
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        X = 16,
                        Text = "Scroll to zoom, drag to pan",
                        Font = FontUsage.Default.With(size: 16),
                        Colour = Color4.LightGray,
                    },
                    zoomValueText = new SpriteText
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        X = -84,
                        Font = FontUsage.Default.With(size: 18),
                    },
                    new BasicButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        X = -8,
                        Width = 64,
                        Height = 28,
                        Text = "Reset",
                        Action = reset,
                    },
                ],
            },
        ];
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        zoom.BindValueChanged(e => zoomValueText.Text = $"{e.NewValue:0.00}x", true);
    }

    protected override bool OnScroll(ScrollEvent e)
    {
        var factor = MathF.Pow(1.2f, e.ScrollDelta.Y);
        zoom.Value = Math.Clamp(zoom.Value * factor, min_zoom, max_zoom);
        return true;
    }

    protected override bool OnDragStart(DragStartEvent e) => true;

    protected override void OnDrag(DragEvent e)
    {
        pan.Value += e.Delta;
        base.OnDrag(e);
    }

    private void reset()
    {
        zoom.Value = 1f;
        pan.Value = Vector2.Zero;
    }

    private partial class ComparisonColumn : Container
    {
        private readonly string label;
        private readonly Drawable sample;
        private readonly Bindable<float> zoom;
        private readonly Bindable<Vector2> pan;

        private Container zoomArea = null!;

        public ComparisonColumn(string label, Drawable sample, Bindable<float> zoom, Bindable<Vector2> pan)
        {
            this.label = label;
            this.sample = sample;
            this.zoom = zoom.GetBoundCopy();
            this.pan = pan.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;

            InternalChildren =
            [
                new SpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = label,
                    Font = FontUsage.Default.With(size: 20),
                },
                zoomArea = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = sample,
                },
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            zoom.BindValueChanged(e => zoomArea.Scale = new Vector2(e.NewValue), true);
            pan.BindValueChanged(e => zoomArea.Position = e.NewValue, true);
        }
    }
}
