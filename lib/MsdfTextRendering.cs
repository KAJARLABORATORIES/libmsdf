using osu.Framework.IO.Stores;

namespace kajarlabs.osu.Framework.MsdfTextRendering;

public static class MsdfTextRendering
{
    public static void CreateCapabilities(ResourceStore<byte[]> store)
    {
        store.AddStore(new NamespacedResourceStore<byte[]>(new DllResourceStore(typeof(MsdfTextRendering).Assembly), @"Resources"));
    }
}
