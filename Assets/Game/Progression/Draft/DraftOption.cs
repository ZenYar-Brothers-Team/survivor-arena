namespace Game.Progression
{
    public readonly struct DraftOption
    {
        public BuildEntryDefinition Definition { get; }
        public bool IsUpgrade { get; }
        public int ResultingLevel { get; }
        public DraftOptionPreview Preview { get; }

        public DraftOption(BuildEntryDefinition definition, bool isUpgrade, int resultingLevel)
        {
            Definition = definition;
            IsUpgrade = isUpgrade;
            ResultingLevel = resultingLevel;
            Preview = definition.CreateDraftPreview(isUpgrade ? resultingLevel - 1 : 0, resultingLevel);
        }
    }
}
