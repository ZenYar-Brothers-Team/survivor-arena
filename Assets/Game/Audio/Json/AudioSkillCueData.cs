namespace Game.Audio.Json
{
    /// <summary>Binds one active skill to the sound family played when it activates; unbound skills use "skill.cast".</summary>
    public sealed class AudioSkillCueData
    {
        public string SkillId;
        public string Cue;
    }
}
