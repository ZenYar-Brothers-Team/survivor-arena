namespace Game.ActiveSkill.Json
{
    // JSON shape for the IActiveSkillEffect hierarchy. Each concrete effect
    // carries its own Kind so ActiveSkillEffectJsonConverter can pick the
    // right DTO type on the way in; see that converter for the read side.
    public interface IActiveSkillEffectData
    {
        ActiveSkillEffectKind Kind { get; }
    }
}
