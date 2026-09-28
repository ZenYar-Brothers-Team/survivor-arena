using Game.Run;
namespace Game.UI.Tests
{
    public sealed class ResultsTestContributor : IRunOutcomeContributor
    {
        public string Key { get; }
        private readonly RunOutcomeContribution _value;
        public ResultsTestContributor(string key, RunOutcomeContribution value) { Key = key; _value = value; }
        public RunOutcomeContribution Capture() => _value;
    }
}
