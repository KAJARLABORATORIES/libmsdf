using osu.Framework.Testing;

namespace kajarlabs.osu.Framework.MsdfTextRendering.Tests.Visual;

public abstract partial class SampleTestScene : TestScene
{
    protected override ITestSceneTestRunner CreateRunner()
        => new SampleGameTestRunner();

    private partial class SampleGameTestRunner : SampleGameBase, ITestSceneTestRunner
    {
        private TestSceneTestRunner.TestRunner? runner;

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();

            Add(runner = new TestSceneTestRunner.TestRunner());
        }

        public void RunTestBlocking(TestScene test)
            => runner?.RunTestBlocking(test);
    }
}
