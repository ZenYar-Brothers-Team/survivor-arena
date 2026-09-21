using Game.Run;
namespace Game.Meta.Tests
{
    public sealed class MetaOutcomeContributor : IRunOutcomeContributor
    {
        public string Key { get; }
        private readonly RunOutcomeContribution _value;
        public MetaOutcomeContributor(string key, RunOutcomeContribution value) { Key = key; _value = value; }
        public RunOutcomeContribution Capture() => _value;
    }
}
