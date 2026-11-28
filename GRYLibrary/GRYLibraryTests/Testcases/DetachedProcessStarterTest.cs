using GRYLibrary.Core.ExecutePrograms.Detached;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class DetachedProcessStarterTest
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void GetForCurrentPlatformReturnsTheImplementationForTheCurrentOperatingSystem()
        {
            //act
            IDetachedProcessStarter starter = DetachedProcessStarter.GetForCurrentPlatform();

            //assert
            if (System.OperatingSystem.IsWindows())
            {
                Assert.IsInstanceOfType<WindowsDetachedProcessStarter>(starter);
            }
            else
            {
                Assert.IsInstanceOfType<UnixDetachedProcessStarter>(starter);
            }
        }
    }
}
