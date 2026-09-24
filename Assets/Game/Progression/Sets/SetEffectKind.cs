namespace Game.Progression
{
    /// <summary>
    /// SlowedTargetBonus: additive damage/knockback bonus against a target slowed before the hit (skill-specific
    /// or all player skills). OrbitSlowAura: refreshing slow inside the current radius of an existing orbit skill.
    /// </summary>
    public enum SetEffectKind { StatBuff, SkillTransform, ActivationProc, RewardProc, LevelHeal, IndependentAttack, SlowedTargetBonus, OrbitSlowAura }
}
