using System;

namespace Game.Telemetry.Tests
{
    internal sealed class FakePlaytestExportSink : IPlaytestExportSink
    {
        public bool Fail;
        public PlaytestReport Report;
        public string Write(PlaytestReport report)
        {
            if (Fail) throw new InvalidOperationException("Synthetic disk failure");
            Report = report; return "fake/output";
        }
    }
}
