using Game.Combat;

namespace Game.Enemy.Json
{
    /// <summary>Optional boss teleport-slam authoring layer; absent means the boss never teleports (DECISION-0059).</summary>
    public sealed class BossTeleportData
    {
        public float? FarDistance { get; set; }
        public float? FarSeconds { get; set; }
        public float? LandingDistance { get; set; }
        public float? TelegraphSeconds { get; set; }
        public float? ImpactRadius { get; set; }
        public float? ImpactDamage { get; set; }
        public CombatControlData ImpactControls { get; set; }
        public float? ImpactEffectSeconds { get; set; }
        public float[] TelegraphColor { get; set; }
        public float[] ImpactColor { get; set; }
    }
}
