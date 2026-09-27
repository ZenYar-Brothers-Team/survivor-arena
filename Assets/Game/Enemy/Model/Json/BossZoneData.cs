using Game.Combat;

namespace Game.Enemy.Json
{
    /// <summary>Boss ground zone (DECISION-0066); placement decides which fields are required (see BossSpecialCatalog).</summary>
    public sealed class BossZoneData
    {
        public string Placement { get; set; }
        public int? Count { get; set; }
        public float? Radius { get; set; }
        public float? ScatterRadius { get; set; }
        public float? MinSpacing { get; set; }
        public float? IntervalSeconds { get; set; }
        public float? FillSeconds { get; set; }
        public float? Damage { get; set; }
        public CombatControlData Controls { get; set; }
        public float? LingerSeconds { get; set; }
        public float? LingerDamagePerSecond { get; set; }
        public float? LingerTickSeconds { get; set; }
        public float? ImpactEffectSeconds { get; set; }
        public float[] TelegraphColor { get; set; }
        public float[] ImpactColor { get; set; }
    }
}
