namespace Game.Progression.Json
{
    // JSON shape for RunSetupConfig (Resources/Content/Run/*.json). Every field is required;
    // the catalog rejects a missing one by name instead of inventing a tuning default.
    public sealed class RunSetupConfigData
    {
        public string StartingCharacterId { get; set; }
        public DraftSettingsData Draft { get; set; }
        public ExperienceSettingsData Experience { get; set; }
    }
}
