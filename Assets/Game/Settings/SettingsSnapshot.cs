using Game.Content;
namespace Game.Settings
{
    public sealed class SettingsSnapshot
    {
        public float Master { get; }
        public float Music { get; }
        public float Sfx { get; }
        public bool Shake { get; }
        public VideoMode Video { get; }
        public SettingsSnapshot(float master, float music, float sfx, bool shake, VideoMode video)
        {
            NumericValidation.ValidateRange(master, 0, 1, nameof(master));
            NumericValidation.ValidateRange(music, 0, 1, nameof(music));
            NumericValidation.ValidateRange(sfx, 0, 1, nameof(sfx));
            Master = master; Music = music; Sfx = sfx; Shake = shake;
            Video = video ?? throw new System.ArgumentNullException(nameof(video));
        }
        public SettingsSnapshot WithVideo(VideoMode mode) => new SettingsSnapshot(Master, Music, Sfx, Shake, mode);
        public float Gain(bool music, float sourceGain = 1)
        { NumericValidation.ValidateRange(sourceGain, 0, 1, nameof(sourceGain)); return Master * (music ? Music : Sfx) * sourceGain; }
    }
}
