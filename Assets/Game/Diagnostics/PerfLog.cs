using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Game.Diagnostics
{
    // Backs PerfGuard: decides whether a slow measurement is worth a console
    // warning, and throttles repeats so a hot path that stays slow every frame
    // doesn't flood the console once it starts warning.
    public static class PerfLog
    {
        private const double ThrottleSeconds = 2.0;

        private static readonly Stopwatch RealClock = Stopwatch.StartNew();
        private static readonly Dictionary<string, double> LastWarnedAt = new Dictionary<string, double>(StringComparer.Ordinal);
        private static Func<double> _nowSeconds = () => RealClock.Elapsed.TotalSeconds;

        public static void ReportIfSlow(string operationName, double elapsedMilliseconds, float warningMilliseconds)
        {
            if (string.IsNullOrEmpty(operationName))
                throw new ArgumentException("Operation name cannot be empty.", nameof(operationName));
            if (elapsedMilliseconds < warningMilliseconds)
                return;

            var now = _nowSeconds();
            if (LastWarnedAt.TryGetValue(operationName, out var lastWarnedAt) && now - lastWarnedAt < ThrottleSeconds)
                return;

            LastWarnedAt[operationName] = now;
            UnityEngine.Debug.LogWarning(
                $"[Perf] '{operationName}' took {elapsedMilliseconds:F2} ms (warning threshold {warningMilliseconds:F2} ms).");
        }

        // Test seam: lets tests control throttle timing and reset state between
        // cases instead of waiting on the real clock.
        public static void SetClockForTests(Func<double> nowSeconds)
        {
            _nowSeconds = nowSeconds ?? throw new ArgumentNullException(nameof(nowSeconds));
        }

        public static void ResetForTests()
        {
            _nowSeconds = () => RealClock.Elapsed.TotalSeconds;
            LastWarnedAt.Clear();
        }
    }
}
