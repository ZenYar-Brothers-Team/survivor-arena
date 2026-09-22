namespace Game.Run
{
    /// <summary>Owner-supplied, immutable result data. Capture must not mutate the run.</summary>
    public interface IRunOutcomeContributor
    {
        string Key { get; }
        RunOutcomeContribution Capture();
    }
}
