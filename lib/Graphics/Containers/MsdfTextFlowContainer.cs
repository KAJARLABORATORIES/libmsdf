using System.Buffers;
using kajarlabs.osu.Framework.MsdfTextRendering.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osuTK;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Graphics.Containers;

public partial class MsdfTextFlowContainer : CompositeDrawable
{
    private readonly Action<MsdfSpriteText>? defaultCreationParameters;

    private readonly List<IMsdfTextPart> parts = [];

    private readonly Cached partsCache = new();

    public float FirstLineIndent
    {
        get => Flow.FirstLineIndent;
        set => Flow.FirstLineIndent = value;
    }

    public float ContentIndent
    {
        get => Flow.ContentIndent;
        set => Flow.ContentIndent = value;
    }

    public float ParagraphSpacing
    {
        get => Flow.ParagraphSpacing;
        set => Flow.ParagraphSpacing = value;
    }

    public float LineSpacing
    {
        get => Flow.LineSpacing;
        set => Flow.LineSpacing = value;
    }

    public Anchor TextAnchor
    {
        get => Flow.TextAnchor;
        set => Flow.TextAnchor = value;
    }

    public LocalisableString Text
    {
        set
        {
            Flow.Clear();
            parts.Clear();

            AddText(value);
        }
    }

    public new Axes RelativeSizeAxes
    {
        get => base.RelativeSizeAxes;
        set
        {
            base.RelativeSizeAxes = value;
            setFlowSizing();
        }
    }

    public new Axes AutoSizeAxes
    {
        get => base.AutoSizeAxes;
        set
        {
            base.AutoSizeAxes = value;
            setFlowSizing();
        }
    }

    public override float Width
    {
        get => base.Width;
        set
        {
            base.Width = value;
            setFlowSizing();
        }
    }

    private void setFlowSizing()
    {
        if (AutoSizeAxes.HasFlagFast(Axes.X))
        {
            Flow.RelativeSizeAxes = Axes.None;
            Flow.AutoSizeAxes = Axes.Both;
        }
        else
        {
            Flow.AutoSizeAxes = Axes.Y;
            Flow.RelativeSizeAxes = Axes.X;
        }
    }

    public new MarginPadding Padding
    {
        get => base.Padding;
        set => base.Padding = value;
    }

    public Vector2 Spacing
    {
        get => Flow.Spacing;
        set => Flow.Spacing = value;
    }

    public Vector2 MaximumSize
    {
        get => Flow.MaximumSize;
        set => Flow.MaximumSize = value;
    }

    public new bool Masking
    {
        get => base.Masking;
        set => base.Masking = value;
    }

    public FillDirection Direction
    {
        get => Flow.Direction;
        set => Flow.Direction = value;
    }

    public IEnumerable<Drawable> Children => Flow.Children;

    [Resolved]
    internal LocalisationManager? Localisation { get; private set; }

    protected readonly MsdfInnerFlow Flow;
    private readonly Bindable<LocalisationParameters> localisationParameters = new();

    public MsdfTextFlowContainer(Action<MsdfSpriteText>? defaultCreationParameters = null)
    {
        this.defaultCreationParameters = defaultCreationParameters;

        InternalChild = Flow = CreateFlow().With(f => f.AutoSizeAxes = Axes.Both);
    }

    protected virtual MsdfInnerFlow CreateFlow() => new();

    protected override void LoadAsyncComplete()
    {
        base.LoadAsyncComplete();

        if (Localisation != null)
            localisationParameters.Value = Localisation.CurrentParameters.Value;

        RecreateAllParts();
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        if (Localisation != null)
        {
            localisationParameters.BindValueChanged(_ => partsCache.Invalidate());
            ((IBindable<LocalisationParameters>)localisationParameters).BindTo(Localisation.CurrentParameters);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!partsCache.IsValid)
            RecreateAllParts();
    }

    protected override int Compare(Drawable x, Drawable y)
    {
        if (TextAnchor.HasFlagFast(Anchor.x2))
            return base.Compare(y, x);

        return base.Compare(x, y);
    }

    public IMsdfTextPart AddText<TMsdfSpriteText>(LocalisableString text, Action<TMsdfSpriteText>? creationParameters = null)
        where TMsdfSpriteText : MsdfSpriteText, new()
        => AddPart(CreateChunkFor(text, true, () => new TMsdfSpriteText(), creationParameters));

    public IMsdfTextPart AddText(LocalisableString text, Action<MsdfSpriteText>? creationParameters = null)
        => AddPart(CreateChunkFor(text, true, CreateSpriteText, creationParameters));

    public void AddText<TMsdfSpriteText>(TMsdfSpriteText text, Action<TMsdfSpriteText>? creationParameters = null)
        where TMsdfSpriteText : MsdfSpriteText
    {
        defaultCreationParameters?.Invoke(text);
        creationParameters?.Invoke(text);
        AddPart(new MsdfTextPartManual(text.Yield()));
    }

    public IMsdfTextPart AddParagraph<TMsdfSpriteText>(LocalisableString paragraph, Action<TMsdfSpriteText>? creationParameters = null)
        where TMsdfSpriteText : MsdfSpriteText, new()
        => AddPart(CreateChunkFor(paragraph, false, () => new TMsdfSpriteText(), creationParameters));

    /// <inheritdoc cref="AddParagraph{TMsdfSpriteText}(LocalisableString,Action{TMsdfSpriteText}?)"/>
    public IMsdfTextPart AddParagraph(LocalisableString paragraph, Action<MsdfSpriteText>? creationParameters = null)
        => AddPart(CreateChunkFor(paragraph, false, CreateSpriteText, creationParameters));

    protected internal virtual MsdfTextChunk<TMsdfSpriteText> CreateChunkFor<TMsdfSpriteText>(LocalisableString text, bool newLineIsParagraph, Func<TMsdfSpriteText> creationFunc, Action<TMsdfSpriteText>? creationParameters = null)
        where TMsdfSpriteText : MsdfSpriteText, new()
        => new(text, newLineIsParagraph, creationFunc, creationParameters);

    public void NewLine() => AddPart(new MsdfTextNewLine(false));

    public void NewParagraph() => AddPart(new MsdfTextNewLine(true));

    protected internal virtual MsdfSpriteText CreateSpriteText() => new();

    internal void ApplyDefaultCreationParameters(MsdfSpriteText spriteText) => defaultCreationParameters?.Invoke(spriteText);

    public void Clear(bool disposeChildren = true)
    {
        Flow.Clear(disposeChildren);
        parts.Clear();
    }

    protected internal IMsdfTextPart AddPart(IMsdfTextPart part)
    {
        parts.Add(part);

        if (partsCache.IsValid)
            recreatePart(part);

        return part;
    }

    public bool RemovePart(IMsdfTextPart partToRemove)
    {
        if (!parts.Remove(partToRemove))
            return false;

        partsCache.Invalidate();
        return true;
    }

    protected virtual void RecreateAllParts()
    {
        foreach (var manualPart in parts.OfType<MsdfTextPartManual>())
            Flow.RemoveRange(manualPart.Drawables, false);

        Flow.Clear(true);

        foreach (var part in parts)
            recreatePart(part);

        partsCache.Validate();
    }

    private void recreatePart(IMsdfTextPart part)
    {
        part.RecreateDrawablesFor(this);
        foreach (var drawable in part.Drawables)
            Flow.Add(drawable);
    }

    protected partial class MsdfInnerFlow : FillFlowContainer
    {
        public float FirstLineIndent
        {
            get;
            set
            {
                if (field == value) return;

                field = value;

                InvalidateLayout();
            }
        }

        public float ContentIndent
        {
            get;
            set
            {
                if (field == value) return;

                field = value;

                InvalidateLayout();
            }
        }

        public float ParagraphSpacing
        {
            get;
            set
            {
                if (field == value) return;

                field = value;

                InvalidateLayout();
            }
        } = 0.5f;

        public float LineSpacing
        {
            get;
            set
            {
                if (field == value) return;

                field = value;

                InvalidateLayout();
            }
        }

        public Anchor TextAnchor
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;

                Anchor = value;
                Origin = value;

                InvalidateLayout();
            }
        } = Anchor.TopLeft;

        protected override IEnumerable<Vector2> ComputeLayoutPositions()
        {
            var max = MaximumSize;

            if (max == Vector2.Zero)
            {
                var s = ChildSize;

                max.X = AutoSizeAxes.HasFlagFast(Axes.X) ? float.PositiveInfinity : s.X;
                max.Y = AutoSizeAxes.HasFlagFast(Axes.Y) ? float.PositiveInfinity : s.Y;
            }

            var children = FlowingChildren.ToArray();
            if (children.Length == 0)
                yield break;

            var layoutPositions = ArrayPool<Vector2>.Shared.Rent(children.Length);

            var rowIndices = ArrayPool<int>.Shared.Rent(children.Length);

            var rowWidths = new List<float> { 0 };
            var lineBaseHeights = new List<float> { 0 };

            var rowHeight = 0.0f;
            var current = Vector2.Zero;

            var size = Vector2.Zero;

            try
            {
                for (var i = 0; i < children.Length; ++i)
                {
                    var c = children[i];

                    static Axes toAxes(FillDirection direction)
                    {
                        switch (direction)
                        {
                            case FillDirection.Full:
                                return Axes.Both;

                            case FillDirection.Horizontal:
                                return Axes.X;

                            case FillDirection.Vertical:
                                return Axes.Y;

                            default:
                                throw new ArgumentException($"{direction} is not defined");
                        }
                    }

                    if ((c.RelativeSizeAxes & AutoSizeAxes & toAxes(Direction)) != 0
                        && (c.FillMode != FillMode.Fit || c.RelativeSizeAxes != Axes.Both || c.Size.X > RelativeChildSize.X
                            || c.Size.Y > RelativeChildSize.Y || AutoSizeAxes == Axes.Both))
                    {
                        throw new InvalidOperationException(
                            "Drawables inside a fill flow container may not have a relative size axis that the fill flow container is filling in and auto sizing for. " +
                            $"The fill flow container is set to flow in the {Direction} direction and autosize in {AutoSizeAxes} axes and the child is set to relative size in {c.RelativeSizeAxes} axes.");
                    }

                    if (c.RelativeAnchorPosition != Vector2.Zero)
                        throw new InvalidOperationException($"All drawables in a {nameof(MsdfTextFlowContainer)} must not specify custom {nameof(RelativeAnchorPosition)}s. Only (0,0) is supported.");
                    if (c.RelativeOriginPosition != Vector2.Zero)
                        throw new InvalidOperationException($"All drawables in a {nameof(MsdfTextFlowContainer)} must not specify custom {nameof(RelativeOriginPosition)}s. Only (0,0) is supported.");

                    if (i == 0)
                    {
                        size = c.BoundingBox.Size;
                        current.X = ContentIndent + FirstLineIndent;
                    }

                    var rowWidth = current.X + size.X;

                    if (Direction != FillDirection.Horizontal && (Precision.DefinitelyBigger(rowWidth, max.X) || Direction == FillDirection.Vertical || c is NewLineContainer))
                    {
                        current.X = ContentIndent;
                        current.Y += rowHeight * (1 + LineSpacing);

                        if (c is NewLineContainer nlc)
                            current.Y += nlc.IndicatesNewParagraph ? rowHeight * ParagraphSpacing : 0;

                        layoutPositions[i] = current;

                        rowWidths.Add(current.X + size.X);
                        lineBaseHeights.Add((c as IHasLineBaseHeight)?.LineBaseHeight ?? 0);

                        rowHeight = 0;
                    }
                    else
                    {
                        layoutPositions[i] = current;

                        rowWidths[^1] = rowWidth;
                        lineBaseHeights[^1] = Math.Max(lineBaseHeights[^1], (c as IHasLineBaseHeight)?.LineBaseHeight ?? 0);
                    }

                    rowIndices[i] = rowWidths.Count - 1;
                    var stride = Vector2.Zero;

                    if (i < children.Length - 1)
                    {
                        stride = size;

                        c = children[i + 1];
                        size = c.BoundingBox.Size;
                    }

                    stride += Spacing;

                    if (stride.Y > rowHeight)
                        rowHeight = stride.Y;

                    current.X += stride.X;
                }

                if (!float.IsFinite(max.X))
                {
                    var newMax = 0.0f;
                    foreach (var rowWidth in rowWidths)
                        newMax = MathF.Max(newMax, rowWidth);
                    max.X = newMax;
                }

                for (var i = 0; i < children.Length; i++)
                {
                    var c = children[i];
                    var layoutPosition = layoutPositions[i];
                    var rowOffsetToEnd = max.X - rowWidths[rowIndices[i]];

                    if (TextAnchor.HasFlagFast(Anchor.x1))
                        layoutPosition.X += rowOffsetToEnd / 2;
                    else if (TextAnchor.HasFlagFast(Anchor.x2))
                        layoutPosition.X += rowOffsetToEnd;

                    if (c is IHasLineBaseHeight hasLineBaseHeight)
                        layoutPosition.Y += lineBaseHeights[rowIndices[i]] - hasLineBaseHeight.LineBaseHeight;

                    yield return layoutPosition;
                }
            }
            finally
            {
                ArrayPool<Vector2>.Shared.Return(layoutPositions);
                ArrayPool<int>.Shared.Return(rowIndices);
            }
        }
    }

    public partial class NewLineContainer : Container
    {
        public readonly bool IndicatesNewParagraph;

        public NewLineContainer(bool newParagraph)
        {
            IndicatesNewParagraph = newParagraph;
        }
    }
}
