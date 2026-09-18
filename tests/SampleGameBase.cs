using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.IO.Stores;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Tests;

public abstract partial class SampleGameBase : Game
{
    private DependencyContainer? dependencies;

    [BackgroundDependencyLoader]
    private void load()
    {
        Resources.AddStore(new NamespacedResourceStore<byte[]>(new DllResourceStore(typeof(SampleGameBase).Assembly), "Resources"));
    }

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
}
