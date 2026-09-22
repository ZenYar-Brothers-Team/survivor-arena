namespace Game.UI
{
    public readonly struct SkillObservabilityViewState
    {
        public string Summary { get; }
        public SkillObservabilityViewState(string summary) { Summary = summary ?? string.Empty; }
    }
}
