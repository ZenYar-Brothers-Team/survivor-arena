using Game.Content;
namespace Game.Settings
{
    public sealed class SettingsSnapshot
    {
        public float Master { get; }
        public float Music { get; }
        public float Sfx { get; }
        public bool Shake { get; }
        public bool MouseMovement { get; }
        public VideoMode Video { get; }
        /// <summary>0 = automatic (from screen height); otherwise one of <see cref="UiScales"/>.</summary>
        public float UiScale { get; }
        public static readonly float[] UiScales = { 0f, 1f, 1.25f, 1.5f };
        public SettingsSnapshot(float master, float music, float sfx, bool shake, VideoMode video, bool mouseMovement = false, float uiScale = 0f)
        {
            NumericValidation.ValidateRange(master, 0, 1, nameof(master));
            NumericValidation.ValidateRange(music, 0, 1, nameof(music));
            NumericValidation.ValidateRange(sfx, 0, 1, nameof(sfx));
            Master = master; Music = music; Sfx = sfx; Shake = shake; MouseMovement = mouseMovement;
            Video = video ?? throw new System.ArgumentNullException(nameof(video));
            if (System.Array.IndexOf(UiScales, uiScale) < 0) throw new System.ArgumentOutOfRangeException(nameof(uiScale), "UI scale must be Auto (0), 1, 1.25 or 1.5.");
            UiScale = uiScale;
        }
        public SettingsSnapshot WithVideo(VideoMode mode) => new SettingsSnapshot(Master, Music, Sfx, Shake, mode, MouseMovement, UiScale);
        public SettingsSnapshot WithUiScale(float uiScale) => new SettingsSnapshot(Master, Music, Sfx, Shake, Video, MouseMovement, uiScale);
        public float Gain(bool music, float sourceGain = 1)
        { NumericValidation.ValidateRange(sourceGain, 0, 1, nameof(sourceGain)); return Master * (music ? Music : Sfx) * sourceGain; }
    }
}
