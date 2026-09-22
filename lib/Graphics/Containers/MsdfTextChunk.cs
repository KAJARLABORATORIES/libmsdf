using System.Globalization;
using System.Text;
using kajarlabs.osu.Framework.MsdfTextRendering.Graphics.Sprites;
using osu.Framework.Graphics;
using osu.Framework.Localisation;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Graphics.Containers;

public class MsdfTextChunk<TMsdfSpriteText> : MsdfTextPart
    where TMsdfSpriteText : MsdfSpriteText, new()
{
    private readonly LocalisableString text;
    private readonly bool newLineIsParagraph;
    private readonly Func<TMsdfSpriteText> creationFunc;
    private readonly Action<TMsdfSpriteText>? creationParameters;

    public MsdfTextChunk(LocalisableString text, bool newLineIsParagraph, Func<TMsdfSpriteText> creationFunc, Action<TMsdfSpriteText>? creationParameters = null)
    {
        this.text = text;
        this.newLineIsParagraph = newLineIsParagraph;
        this.creationFunc = creationFunc;
        this.creationParameters = creationParameters;
    }

    protected override IEnumerable<Drawable> CreateDrawablesFor(MsdfTextFlowContainer textFlowContainer)
    {
        var currentContent = textFlowContainer.Localisation?.GetLocalisedString(text) ?? text.ToString();

        var drawables = new List<Drawable>();

        if (!newLineIsParagraph)
        {
            var newLine = new MsdfTextNewLine(true);

            newLine.RecreateDrawablesFor(textFlowContainer);
            drawables.AddRange(newLine.Drawables);
        }

        drawables.AddRange(CreateDrawablesFor(currentContent, textFlowContainer));
        return drawables;
    }

    protected virtual IEnumerable<Drawable> CreateDrawablesFor(string text, MsdfTextFlowContainer textFlowContainer)
    {
        var first = true;
        var sprites = new List<Drawable>();

        foreach (var l in text.Split('\n'))
        {
            if (!first)
            {
                var lastChild = sprites.LastOrDefault() ?? textFlowContainer.Children.LastOrDefault();

                if (lastChild != null)
                {
                    var newLine = new MsdfTextFlowContainer.NewLineContainer(newLineIsParagraph);

                    sprites.Add(newLine);
                }
            }

            foreach (var word in SplitWords(l))
            {
                if (string.IsNullOrEmpty(word)) continue;

                var textSprite = CreateSpriteText(textFlowContainer);

                textSprite.Text = word;
                sprites.Add(textSprite);
            }

            first = false;
        }

        return sprites;
    }

    protected string[] SplitWords(string text)
    {
        var words = new List<string>();
        var builder = new StringBuilder();

        for (var i = 0; i < text.Length; i++)
        {
            if (i == 0
                || char.IsSeparator(text[i - 1])
                || char.IsControl(text[i - 1])
                || char.GetUnicodeCategory(text[i - 1]) == UnicodeCategory.DashPunctuation
                || text[i - 1] == '/'
                || text[i - 1] == '\\'
                || (isCjkCharacter(text[i - 1]) && !char.IsPunctuation(text[i])))
            {
                words.Add(builder.ToString());
                builder.Clear();
            }

            builder.Append(text[i]);
        }

        if (builder.Length > 0)
            words.Add(builder.ToString());

        return words.ToArray();

        static bool isCjkCharacter(char c) => c >= '\x2E80' && c <= '\x9FFF';
    }

    protected virtual TMsdfSpriteText CreateSpriteText(MsdfTextFlowContainer textFlowContainer)
    {
        var spriteText = creationFunc.Invoke();
        textFlowContainer.ApplyDefaultCreationParameters(spriteText);
        creationParameters?.Invoke(spriteText);
        return spriteText;
    }
}
