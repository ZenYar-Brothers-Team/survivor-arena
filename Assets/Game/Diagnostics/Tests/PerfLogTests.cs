using NUnit.Framework;
using UnityEngine.TestTools;

namespace Game.Diagnostics.Tests
{
    public class PerfLogTests
    {
        [TearDown]
        public void TearDown()
        {
            PerfLog.ResetForTests();
        }

        [Test]
        public void ReportIfSlow_BelowThreshold_DoesNotLog()
        {
            PerfLog.ReportIfSlow("fast-op", elapsedMilliseconds: 0.1, warningMilliseconds: 1f);

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ReportIfSlow_AtOrAboveThreshold_LogsWarningOnce()
        {
            var clockSeconds = 0.0;
            PerfLog.SetClockForTests(() => clockSeconds);

            PerfLog.ReportIfSlow("slow-op", elapsedMilliseconds: 5f, warningMilliseconds: 1f);
            LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("slow-op"));

            // Immediately repeating the same slow measurement is throttled.
            PerfLog.ReportIfSlow("slow-op", elapsedMilliseconds: 5f, warningMilliseconds: 1f);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ReportIfSlow_AfterThrottleWindow_LogsAgain()
        {
            var clockSeconds = 0.0;
            PerfLog.SetClockForTests(() => clockSeconds);

            PerfLog.ReportIfSlow("throttled-op", elapsedMilliseconds: 5f, warningMilliseconds: 1f);
            LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("throttled-op"));

            clockSeconds = 10.0;
            PerfLog.ReportIfSlow("throttled-op", elapsedMilliseconds: 5f, warningMilliseconds: 1f);
            LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("throttled-op"));
        }

        [Test]
        public void PerfGuard_FastOperation_DoesNotLog()
        {
            using (PerfGuard.Measure("guard-fast", warningMilliseconds: 1000f))
            {
            }

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void PerfGuard_ZeroThreshold_AlwaysLogs()
        {
            using (PerfGuard.Measure("guard-slow", warningMilliseconds: 0f))
            {
            }

            LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("guard-slow"));
        }
    }
}
