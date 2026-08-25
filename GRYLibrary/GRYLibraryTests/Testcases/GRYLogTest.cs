using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Console = GRYLibrary.Core.Logging.GRYLogger.ConcreteLogTargets.Console;

namespace GRYLibrary.Tests.Testcases
{
    [TestClass]
    public class GRYLogTest
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void TestLogProgress()
        {
            GRYLog logObject = GRYLog.Create();
            logObject.Configuration.Initliaze();
            logObject.Configuration.StoreProcessedLogItemsInternally = true;
            logObject.LogProgress(0, 4);
            logObject.LogProgress(3, 122);
            logObject.LogProgress(73, 73);
            Assert.AreEqual(3, logObject.ProcessedLogItems.Count);
            Assert.AreEqual("Processed 0/4 items (0.0%)", logObject.ProcessedLogItems[0].PlainMessage);
            Assert.AreEqual("Processed 003/122 items (2.46%)", logObject.ProcessedLogItems[1].PlainMessage);
            Assert.AreEqual("Processed 73/73 items (100.0%)", logObject.ProcessedLogItems[2].PlainMessage);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void TestLogTimezone()
        {
            StringWriter stringWriter = new StringWriter();
            System.Console.SetOut(stringWriter);

            GRYLog logObject = GRYLog.Create();
            logObject.Configuration.Initliaze();
            Console logTarget = new Console
            {
                Format = GRYLogLogFormat.GRYLogFormat
            };
            logObject.Configuration.LogTargets = [logTarget];
            DateTimeOffset moment = new System.DateTimeOffset(2025, 10, 19, 00, 25, 04, TimeSpan.FromHours(2));
            logObject.Log(new LogItem(moment, nameof(TestLogTimezone), Microsoft.Extensions.Logging.LogLevel.Information));
            LogItem logItem = logObject.LastLogEntries.Dequeue();
            string content = stringWriter.ToString().Replace("\n", string.Empty).Replace("\r", string.Empty);
            Assert.AreEqual($"[2025-10-19T00:25:04+02:00] [Information] {nameof(TestLogTimezone)}", content);
        }
    }
}