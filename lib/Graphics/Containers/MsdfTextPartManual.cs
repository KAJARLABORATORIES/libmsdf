using System.Collections.Immutable;
using osu.Framework.Graphics;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Graphics.Containers;

public class MsdfTextPartManual : IMsdfTextPart
{
    public IEnumerable<Drawable> Drawables { get; }

    public event Action<IEnumerable<Drawable>>? DrawablePartsRecreated
    {
        add { }
        remove { }
    }

    public MsdfTextPartManual(IEnumerable<Drawable> drawables)
    {
        Drawables = drawables.ToImmutableArray();
    }

    public void RecreateDrawablesFor(MsdfTextFlowContainer textFlowContainer)
    {
    }
}
