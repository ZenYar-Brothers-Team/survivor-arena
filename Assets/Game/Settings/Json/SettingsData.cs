namespace Game.Settings
{
    /// <summary>Persisted schema v1. Nullable fields reject incomplete documents rather than silently using defaults.</summary>
    public sealed class SettingsData
    {
        public int? SchemaVersion { get; set; }
        public float? Master { get; set; }
        public float? Music { get; set; }
        public float? Sfx { get; set; }
        public bool? Shake { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public bool? Borderless { get; set; }
    }
}
