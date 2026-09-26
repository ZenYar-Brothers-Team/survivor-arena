namespace Game.Presentation
{
    /// <summary>Which procedural world shape a skill uses for a gameplay pattern that has no projectile sprite.</summary>
    public enum SkillWorldEffectKind
    {
        ExpandingRing,
        ChainArc,
        StrikeTelegraph,
        /// <summary>Pulsing beam band redrawn on every damage tick (SKILL-012).</summary>
        Beam
    }
}
