using osu.Framework.Graphics;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Graphics.Containers;

public abstract class MsdfTextPart : IMsdfTextPart
{
    public IEnumerable<Drawable> Drawables { get; }

    public event Action<IEnumerable<Drawable>>? DrawablePartsRecreated;

    private readonly List<Drawable> drawables = [];

    protected MsdfTextPart()
    {
        Drawables = drawables.AsReadOnly();
    }

    public void RecreateDrawablesFor(MsdfTextFlowContainer textFlowContainer)
    {
        drawables.Clear();
        drawables.AddRange(CreateDrawablesFor(textFlowContainer));
        DrawablePartsRecreated?.Invoke(drawables);
    }

    protected abstract IEnumerable<Drawable> CreateDrawablesFor(MsdfTextFlowContainer textFlowContainer);
}
