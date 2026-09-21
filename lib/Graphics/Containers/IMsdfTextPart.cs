using osu.Framework.Graphics;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Graphics.Containers;

public interface IMsdfTextPart
{
    IEnumerable<Drawable> Drawables { get; }

    event Action<IEnumerable<Drawable>>? DrawablePartsRecreated;

    void RecreateDrawablesFor(MsdfTextFlowContainer textFlowContainer);
}
