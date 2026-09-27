namespace Game.Audio.Json
{
    /// <summary>Optional ambience for an explicitly authored field; gain is in [0, 1].</summary>
    public sealed class AudioAmbienceData
    {
        public string FieldId;
        public string Clip;
        public float? Gain;
    }
}
