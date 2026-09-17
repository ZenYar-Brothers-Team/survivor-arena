using System;
using System.Diagnostics;

namespace Game.Diagnostics
{
    // Scopes a potentially-heavy operation for automatic perf warnings:
    //   using (PerfGuard.Measure("EnemyRegistry.TryFindNearest", warningMilliseconds: 1f))
    //   {
    //       ...
    //   }
    // A warning is only logged when the measured time crosses the threshold, so
    // this is safe to leave in a hot path that is currently fast (see PerfLog for
    // the throttling that keeps a persistently slow path from flooding logs).
    public readonly struct PerfGuard : IDisposable
    {
        private readonly string _operationName;
        private readonly float _warningMilliseconds;
        private readonly long _startTimestamp;

        private PerfGuard(string operationName, float warningMilliseconds)
        {
            _operationName = operationName;
            _warningMilliseconds = warningMilliseconds;
            _startTimestamp = Stopwatch.GetTimestamp();
        }

        public static PerfGuard Measure(string operationName, float warningMilliseconds)
        {
            if (string.IsNullOrEmpty(operationName))
                throw new ArgumentException("Operation name cannot be empty.", nameof(operationName));
            if (warningMilliseconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(warningMilliseconds));

            return new PerfGuard(operationName, warningMilliseconds);
        }

        public void Dispose()
        {
            var elapsedTicks = Stopwatch.GetTimestamp() - _startTimestamp;
            var elapsedMilliseconds = elapsedTicks * 1000.0 / Stopwatch.Frequency;
            PerfLog.ReportIfSlow(_operationName, elapsedMilliseconds, _warningMilliseconds);
        }
    }
}
