using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Graphics.Containers;

public class MsdfTextNewLine : MsdfTextPart
{
    private readonly bool indicatesNewParagraph;

    public MsdfTextNewLine(bool indicatesNewParagraph)
    {
        this.indicatesNewParagraph = indicatesNewParagraph;
    }

    protected override IEnumerable<Drawable> CreateDrawablesFor(MsdfTextFlowContainer textFlowContainer)
    {
        var newLineContainer = new MsdfTextFlowContainer.NewLineContainer(indicatesNewParagraph);
        return newLineContainer.Yield();
    }
}
