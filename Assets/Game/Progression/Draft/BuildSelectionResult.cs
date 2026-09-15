namespace Game.Progression
{
    public readonly struct BuildSelectionResult
    {
        public BuildEntry Entry { get; }
        public bool WasNewEntry { get; }

        public BuildSelectionResult(BuildEntry entry, bool wasNewEntry)
        {
            Entry = entry;
            WasNewEntry = wasNewEntry;
        }
    }
}
