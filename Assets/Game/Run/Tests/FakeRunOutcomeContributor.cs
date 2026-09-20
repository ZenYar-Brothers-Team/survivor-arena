using System;

namespace Game.Run.Tests
{
    internal sealed class FakeRunOutcomeContributor : IRunOutcomeContributor
    {
        public string Key { get; }
        public Func<RunOutcomeContribution> CaptureAction { get; set; }
        public int CaptureCount { get; private set; }

        public FakeRunOutcomeContributor(string key, Func<RunOutcomeContribution> capture)
        {
            Key = key;
            CaptureAction = capture;
        }

        public RunOutcomeContribution Capture()
        {
            CaptureCount++;
            return CaptureAction();
        }
    }
}
