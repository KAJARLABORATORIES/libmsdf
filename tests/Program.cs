using kajarlabs.osu.Framework.MsdfTextRendering.Tests;
using osu.Framework;

using var host = Host.GetSuitableDesktopHost("kajarlabs-visual-tests", new HostOptions
{
    PortableInstallation = true
});
using var game = new SampleGameTestBrowser();

host.Run(game);
