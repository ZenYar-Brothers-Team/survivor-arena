using System;
using Game.Content;
using Game.Content.Json;
namespace Game.Settings
{
    /// <summary>DECISION-0038 defaults and bounded presentation-only tuning.</summary>
    public sealed class SettingsConfig
    {
        public float NotificationSeconds { get; }
        public float Master { get; }
        public float Music { get; }
        public float Sfx { get; }
        public bool Shake { get; }
        public int SafeWidth { get; }
        public int SafeHeight { get; }
        public float ConfirmSeconds { get; }
        public float ApplyWaitSeconds { get; }
        public float ShakeFraction { get; }
        public float ShakeSeconds { get; }
        public float ShakeFrequency { get; }
        public float PreviewSeconds { get; }
        public int PreviewSampleRate { get; }
        public float MusicPreviewHz { get; }
        public float SfxPreviewHz { get; }
        public float PreviewGain { get; }
        private static T Required<T>(T? value, string field) where T : struct => value ?? throw new ArgumentException(field + " required.");
        public SettingsConfig(SettingsConfigData d)
        {
            if (d == null) throw new ArgumentNullException(nameof(d));
            NotificationSeconds=Required(d.NotificationSeconds,nameof(d.NotificationSeconds));
            NumericValidation.ValidatePositive(NotificationSeconds,nameof(NotificationSeconds));
            Master=Required(d.Master,nameof(d.Master));Music=Required(d.Music,nameof(d.Music));Sfx=Required(d.Sfx,nameof(d.Sfx));Shake=Required(d.Shake,nameof(d.Shake));
            SafeWidth=Required(d.SafeWidth,nameof(d.SafeWidth));SafeHeight=Required(d.SafeHeight,nameof(d.SafeHeight));
            ConfirmSeconds=Required(d.ConfirmSeconds,nameof(d.ConfirmSeconds));ApplyWaitSeconds=Required(d.ApplyWaitSeconds,nameof(d.ApplyWaitSeconds));
            ShakeFraction=Required(d.ShakeFraction,nameof(d.ShakeFraction));ShakeSeconds=Required(d.ShakeSeconds,nameof(d.ShakeSeconds));ShakeFrequency=Required(d.ShakeFrequency,nameof(d.ShakeFrequency));
            PreviewSeconds=Required(d.PreviewSeconds,nameof(d.PreviewSeconds));PreviewSampleRate=Required(d.PreviewSampleRate,nameof(d.PreviewSampleRate));
            MusicPreviewHz=Required(d.MusicPreviewHz,nameof(d.MusicPreviewHz));SfxPreviewHz=Required(d.SfxPreviewHz,nameof(d.SfxPreviewHz));PreviewGain=Required(d.PreviewGain,nameof(d.PreviewGain));
            _ = Defaults(new VideoMode(SafeWidth,SafeHeight,false));
            NumericValidation.ValidatePositive(ConfirmSeconds,nameof(ConfirmSeconds));NumericValidation.ValidatePositive(ApplyWaitSeconds,nameof(ApplyWaitSeconds));
            NumericValidation.ValidateRange(ShakeFraction,0,.01f,nameof(ShakeFraction));NumericValidation.ValidateRange(ShakeSeconds,.05f,.25f,nameof(ShakeSeconds));NumericValidation.ValidateRange(ShakeFrequency,10,40,nameof(ShakeFrequency));
            NumericValidation.ValidateRange(PreviewSeconds,.05f,2,nameof(PreviewSeconds));NumericValidation.ValidateRange(PreviewSampleRate,8000,48000,nameof(PreviewSampleRate));
            NumericValidation.ValidateRange(MusicPreviewHz,20,PreviewSampleRate/2f,nameof(MusicPreviewHz));NumericValidation.ValidateRange(SfxPreviewHz,20,PreviewSampleRate/2f,nameof(SfxPreviewHz));NumericValidation.ValidateRange(PreviewGain,0,1,nameof(PreviewGain));
        }
        public SettingsSnapshot Defaults(VideoMode desktop) => new SettingsSnapshot(Master,Music,Sfx,Shake,desktop);
        public static SettingsConfig Load() => new SettingsConfig(JsonContentFile.Load<SettingsConfigData>("Content/Settings/SettingsDefaults"));
    }
}
