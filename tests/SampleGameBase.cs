using kajarlabs.osu.Framework.MsdfTextRendering.IO.Stores;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.IO.Stores;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Tests;

public abstract partial class SampleGameBase : Game
{
    private DependencyContainer? dependencies;
    private MsdfFontStore? msdfFonts;

    [BackgroundDependencyLoader]
    private void load()
    {
        Resources.AddStore(new NamespacedResourceStore<byte[]>(new DllResourceStore(typeof(SampleGameBase).Assembly), "Resources"));
        MsdfTextRendering.CreateCapabilities(Resources);

        msdfFonts = new MsdfFontStore(Host.Renderer, Shaders);

        foreach (var weight in new[] { "Thin", "ExtraLight", "Light", "Regular", "Medium", "SemiBold", "Bold", "Black" })
        {
            AddFont(Resources, $"Fonts/WorkSans/WorkSans-{weight}");
            AddFont(Resources, $"Fonts/WorkSans/WorkSans-{weight}Italic");

            msdfFonts.AddFont(Resources, $"WorkSans", weight);
            msdfFonts.AddFont(Resources, $"WorkSans", $"{weight}Italic");
        }

        msdfFonts.AddFont(Resources, $"NotoSansJP", "Regular");

        dependencies?.CacheAs(msdfFonts);
    }

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
}
