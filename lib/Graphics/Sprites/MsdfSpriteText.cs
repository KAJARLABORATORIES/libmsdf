using System.Diagnostics;
using System.Runtime.InteropServices;
using kajarlabs.osu.Framework.MsdfTextRendering.IO.Stores;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Layout;
using osu.Framework.Localisation;
using osu.Framework.Text;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Graphics.Sprites;

public partial class MsdfSpriteText : Drawable, IHasLineBaseHeight, ITexturedShaderDrawable, IHasText, IHasFilterTerms, IFillFlowContainer, IHasCurrentValue<string>
{
    private static readonly char[] default_never_fixed_width_characters = ['.', ',', ':', ' ', '\u00A0', '\u202F'];

    [Resolved]
    private MsdfFontStore msdfFonts { get; set; } = null!;

    [Resolved]
    private LocalisationManager localisation { get; set; } = null!;

    private ILocalisedBindableString localisedText = null!;

    public IShader? TextureShader { get; protected set; }

    private float distanceRange;
    private bool storeReady;

    public MsdfSpriteText()
    {
        current.BindValueChanged(text =>
        {
            if (localisedText == null || text.NewValue != localisedText.Value)
                Text = text.NewValue;
        });

        AddLayout(charactersCache);
        AddLayout(shadowOffsetCache);
        AddLayout(textBuilderCache);
    }

    [BackgroundDependencyLoader]
    private void load(ShaderManager shaders)
    {
        TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "MsdfGlyph");

        localisedText = localisation.GetLocalisedBindableString(Text);

        updateFontStore();

        storeReady = true;
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        localisedText.BindValueChanged(str =>
        {
            current.Value = localisedText.Value;

            if (string.IsNullOrEmpty(str.NewValue))
            {
                if (requiresAutoSizedWidth)
                    base.Width = Padding.TotalHorizontal;
                if (requiresAutoSizedHeight)
                    base.Height = Padding.TotalVertical;
            }

            invalidate(true);
        }, true);
    }

    public LocalisableString Text
    {
        get;
        set
        {
            if (field.Equals(value))
                return;

            field = value;
            localisedText?.Text = value;
        }
    } = string.Empty;

    private readonly BindableWithCurrent<string> current = new();

    public Bindable<string> Current
    {
        get => current.Current;
        set => current.Current = value;
    }

    private string displayedText => localisedText?.Value ?? Text.ToString();

    public FontUsage Font
    {
        get;
        set
        {
            field = value;
            invalidate(true, true);
            shadowOffsetCache.Invalidate();

            if (storeReady)
                updateFontStore();
        }
    }

    public bool AllowMultiline
    {
        get;
        set
        {
            if (field == value)
                return;

            if (value)
                Truncate = false;

            field = value;
            invalidate(true, true);
        }
    } = true;

    public bool Shadow
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            Invalidate(Invalidation.DrawNode);
        }
    }

    public Color4 ShadowColour
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            Invalidate(Invalidation.DrawNode);
        }
    } = new Color4(0, 0, 0, 0.2f);

    public Vector2 ShadowOffset
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            invalidate(true);
            shadowOffsetCache.Invalidate();
        }
    } = new Vector2(0, 0.06f);

    public bool UseFullGlyphHeight
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            invalidate(true, true);
        }
    } = true;

    public bool Truncate
    {
        get;
        set
        {
            if (field == value)
                return;

            if (value)
                AllowMultiline = false;

            field = value;
            invalidate(true, true);
        }
    }

    public string EllipsisString
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            invalidate(true, true);
        }
    } = "…";

    public bool IsTruncated { get; private set; }

    private bool requiresAutoSizedWidth => explicitWidth == null && (RelativeSizeAxes & Axes.X) == 0;

    private bool requiresAutoSizedHeight => explicitHeight == null && (RelativeSizeAxes & Axes.Y) == 0;

    private float? explicitWidth;

    public override float Width
    {
        get
        {
            if (requiresAutoSizedWidth)
                computeCharacters();
            return base.Width;
        }
        set
        {
            if (explicitWidth == value)
                return;

            base.Width = value;
            explicitWidth = value;

            invalidate(true, true);
        }
    }

    public float MaxWidth
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            invalidate(true, true);
        }
    } = float.PositiveInfinity;

    private float? explicitHeight;

    public override float Height
    {
        get
        {
            if (requiresAutoSizedHeight)
                computeCharacters();
            return base.Height;
        }
        set
        {
            if (explicitHeight == value)
                return;

            base.Height = value;
            explicitHeight = value;

            invalidate(true, true);
        }
    }

    public override Vector2 Size
    {
        get
        {
            if (requiresAutoSizedWidth || requiresAutoSizedHeight)
                computeCharacters();
            return base.Size;
        }
        set
        {
            Width = value.X;
            Height = value.Y;
        }
    }

    public Vector2 Spacing
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            invalidate(true, true);
        }
    }

    public MarginPadding Padding
    {
        get;
        set
        {
            if (field.Equals(value))
                return;

            if (!Validation.IsFinite(value))
                throw new ArgumentException($@"{nameof(Padding)} must be finite, but is {value}.");

            field = value;
            invalidate(true, true);
        }
    }

    public override bool IsPresent => base.IsPresent && (AlwaysPresent || !string.IsNullOrEmpty(displayedText));

    private readonly LayoutValue charactersCache = new(Invalidation.DrawSize | Invalidation.Presence, InvalidationSource.Parent);

    private readonly List<TextBuilderGlyph> charactersBacking = [];

    private List<TextBuilderGlyph> characters
    {
        get
        {
            computeCharacters();
            return charactersBacking;
        }
    }

    private void computeCharacters()
    {
        if (LoadState >= LoadState.Loaded)
            Debug.Assert(ThreadSafety.IsUpdateThread);

        if (!storeReady)
            return;

        if (charactersCache.IsValid)
            return;

        IsTruncated = false;

        charactersBacking.Clear();

        var textBounds = Vector2.Zero;

        try
        {
            if (string.IsNullOrEmpty(displayedText))
                return;

            var textBuilder = getTextBuilder();

            textBuilder.Reset();
            textBuilder.AddText(displayedText);
            textBounds = textBuilder.Bounds;

            if (textBuilder is TruncatingTextBuilder truncatingTextBuilder)
                IsTruncated = truncatingTextBuilder.IsTruncated;
        }
        finally
        {
            if (requiresAutoSizedWidth)
                base.Width = textBounds.X + Padding.Right;
            if (requiresAutoSizedHeight)
                base.Height = textBounds.Y + Padding.Bottom;

            base.Width = Math.Min(base.Width, MaxWidth);

            charactersCache.Validate();
        }
    }

    private readonly LayoutValue<Vector2> shadowOffsetCache = new(Invalidation.DrawInfo, InvalidationSource.Parent);

    private Vector2 premultipliedShadowOffset =>
        shadowOffsetCache.IsValid ? shadowOffsetCache.Value : shadowOffsetCache.Value = ToScreenSpace(ShadowOffset * Font.Size) - ToScreenSpace(Vector2.Zero);

    private void invalidate(bool characters = false, bool textBuilder = false)
    {
        if (characters)
            charactersCache.Invalidate();

        if (textBuilder)
            InvalidateTextBuilder();

        Invalidate(Invalidation.RequiredParentSizeToFit);
    }

    protected override DrawNode CreateDrawNode() => new MsdfSpriteTextDrawNode(this);

    /// <summary>
    /// The characters that should be excluded from fixed-width application. Defaults to (".", ",", ":", " ") if null.
    /// </summary>
    protected virtual char[]? FixedWidthExcludeCharacters => null;

    /// <summary>
    /// The character to use to calculate the fixed width width. Defaults to 'm'.
    /// </summary>
    protected virtual char FixedWidthReferenceCharacter => 'm';

    /// <summary>
    /// The character to fallback to use if a character glyph lookup failed.
    /// </summary>
    /// <remarks>
    /// Native <see cref="SpriteText"/> defaults this to '•', but MSDF atlases are generated from a
    /// charset file that does not necessarily include it, in
    /// which case the fallback lookup itself silently fails and the character is dropped entirely.
    /// '?' is used instead since it's part of the basic ASCII range every generated atlas covers.
    /// </remarks>
    protected virtual char FallbackCharacter => '?';

    private readonly LayoutValue<TextBuilder> textBuilderCache = new(Invalidation.DrawSize, InvalidationSource.Parent);

    protected void InvalidateTextBuilder() => textBuilderCache.Invalidate();

    protected virtual TextBuilder CreateTextBuilder(ITexturedGlyphLookupStore store)
    {
        var excludeCharacters = FixedWidthExcludeCharacters ?? default_never_fixed_width_characters;

        var builderMaxWidth = requiresAutoSizedWidth
            ? MaxWidth
            : ApplyRelativeAxes(RelativeSizeAxes, new Vector2(Math.Min(MaxWidth, base.Width), base.Height), FillMode).X - Padding.Right;

        if (AllowMultiline)
        {
            return new MultilineTextBuilder(store, Font, builderMaxWidth, UseFullGlyphHeight, new Vector2(Padding.Left, Padding.Top), Spacing, charactersBacking,
                excludeCharacters, FallbackCharacter, FixedWidthReferenceCharacter);
        }

        if (Truncate)
        {
            return new TruncatingTextBuilder(store, Font, builderMaxWidth, EllipsisString, UseFullGlyphHeight, new Vector2(Padding.Left, Padding.Top), Spacing, charactersBacking,
                excludeCharacters, FallbackCharacter, FixedWidthReferenceCharacter);
        }

        return new TextBuilder(store, Font, builderMaxWidth, UseFullGlyphHeight, new Vector2(Padding.Left, Padding.Top), Spacing, charactersBacking,
            excludeCharacters, FallbackCharacter, FixedWidthReferenceCharacter);
    }

    private TextBuilder getTextBuilder()
    {
        if (!textBuilderCache.IsValid)
            textBuilderCache.Value = CreateTextBuilder(msdfFonts);

        return textBuilderCache.Value;
    }

    private void updateFontStore()
    {
        distanceRange = msdfFonts.GetDistanceRange(Font.FontName) ?? 0f;
    }

    public override string ToString() => $@"""{displayedText}"" " + base.ToString();

    public float LineBaseHeight
    {
        get
        {
            computeCharacters();
            return textBuilderCache.Value.LineBaseHeight;
        }
    }

    public IEnumerable<LocalisableString> FilterTerms => Text.Yield();

    private class MsdfSpriteTextDrawNode : TexturedShaderDrawNode
    {
        protected new MsdfSpriteText Source => (MsdfSpriteText)base.Source;

        private bool shadow;
        private ColourInfo shadowColour;
        private Vector2 shadowOffset;
        private float distanceRange;

        private List<ScreenSpaceGlyphPart>? parts;
        private IUniformBuffer<MsdfParameters>? parametersBuffer;

        public MsdfSpriteTextDrawNode(MsdfSpriteText source)
            : base(source)
        {
        }

        public override void ApplyState()
        {
            base.ApplyState();

            updateScreenSpaceCharacters();
            distanceRange = Source.distanceRange;
            shadow = Source.Shadow;

            if (shadow)
            {
                shadowColour = Source.ShadowColour;
                shadowOffset = Source.premultipliedShadowOffset;
            }
        }

        protected override void Draw(IRenderer renderer)
        {
            Debug.Assert(parts != null);

            base.Draw(renderer);

            BindTextureShader(renderer);

            var avgColour = (Color4)DrawColourInfo.Colour.AverageColour;
            var shadowAlpha = MathF.Pow(Math.Max(Math.Max(avgColour.R, avgColour.G), avgColour.B), 2);

            var finalShadowColour = DrawColourInfo.Colour;
            finalShadowColour.ApplyChild(shadowColour.MultiplyAlpha(shadowAlpha));

            for (var i = 0; i < parts.Count; i++)
            {
                if (shadow)
                {
                    var shadowQuad = parts[i].DrawQuad;

                    renderer.DrawQuad(parts[i].Texture,
                        new Quad(
                            shadowQuad.TopLeft + shadowOffset,
                            shadowQuad.TopRight + shadowOffset,
                            shadowQuad.BottomLeft + shadowOffset,
                            shadowQuad.BottomRight + shadowOffset),
                        finalShadowColour, inflationPercentage: parts[i].InflationPercentage);
                }

                renderer.DrawQuad(parts[i].Texture, parts[i].DrawQuad, DrawColourInfo.Colour, inflationPercentage: parts[i].InflationPercentage);
            }

            UnbindTextureShader(renderer);
        }

        protected override void BindUniformResources(IShader shader, IRenderer renderer)
        {
            base.BindUniformResources(shader, renderer);

            parametersBuffer ??= renderer.CreateUniformBuffer<MsdfParameters>();
            parametersBuffer.Data = new MsdfParameters { DistanceRange = new UniformFloat { Value = distanceRange } };

            shader.BindUniformBlock("m_MsdfParameters", parametersBuffer);
        }

        private void updateScreenSpaceCharacters()
        {
            var partCount = Source.characters.Count;

            if (parts == null)
                parts = new List<ScreenSpaceGlyphPart>(partCount);
            else
            {
                parts.Clear();
                parts.EnsureCapacity(partCount);
            }

            foreach (var character in Source.characters)
            {
                parts.Add(new ScreenSpaceGlyphPart
                {
                    DrawQuad = Source.ToScreenSpace(character.DrawRectangle),
                    InflationPercentage = Vector2.Zero,
                    Texture = character.Texture,
                });
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private record struct MsdfParameters
        {
            public UniformFloat DistanceRange;
            private readonly UniformPadding12 padding;
        }

        private struct ScreenSpaceGlyphPart
        {
            public Quad DrawQuad;
            public Vector2 InflationPercentage;
            public Texture Texture;
        }
    }
}
