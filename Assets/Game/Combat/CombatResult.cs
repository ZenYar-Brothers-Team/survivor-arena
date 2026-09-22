namespace Game.Combat
{
    /// <summary>One resolved request, independent of scene/UI/exporter lifetimes.</summary>
    public readonly struct CombatResult
    {
        public CombatSource Source { get; }
        public CombatIdentity Target { get; }
        public HealthChange Health { get; }
        public float ResolvedKnockbackDistance { get; }
        public CombatResult(CombatSource source, CombatIdentity target, HealthChange health, float resolvedKnockbackDistance = 0f)
        {
            Source = source;
            Target = target;
            Health = health;
            ResolvedKnockbackDistance = resolvedKnockbackDistance;
        }
    }
}
