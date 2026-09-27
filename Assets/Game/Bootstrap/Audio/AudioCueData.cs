namespace Game.Bootstrap.Audio
{
    /// <summary>Authoring data for one sound family. Cooldown is measured in real seconds.</summary>
    public sealed class AudioCueData
    {
        public string Id;
        public string[] Clips;
        public float? Gain;
        public float? CooldownSeconds;
        public int? Priority;
        public bool? Ui;
    }
}
