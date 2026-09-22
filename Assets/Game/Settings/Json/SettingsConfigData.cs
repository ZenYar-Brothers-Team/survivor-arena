namespace Game.Settings
{
    public sealed class SettingsConfigData
    {
        public float? NotificationSeconds { get; set; }
        public float? Master { get; set; }
        public float? Music { get; set; }
        public float? Sfx { get; set; }
        public bool? Shake { get; set; }
        public int? SafeWidth { get; set; }
        public int? SafeHeight { get; set; }
        public float? ConfirmSeconds { get; set; }
        public float? ApplyWaitSeconds { get; set; }
        public float? ShakeFraction { get; set; }
        public float? ShakeSeconds { get; set; }
        public float? ShakeFrequency { get; set; }
        public float? PreviewSeconds { get; set; }
        public int? PreviewSampleRate { get; set; }
        public float? MusicPreviewHz { get; set; }
        public float? SfxPreviewHz { get; set; }
        public float? PreviewGain { get; set; }
    }
}
