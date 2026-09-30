using Game.Enemy.Json;
namespace Game.Traveler.Json
{
    public sealed class TravelerData
    {
        public string Id;
        public string Name;
        public TravelerRole? Role;
        public EnemyDefinitionData Body;
        public float? PresenceSeconds, WanderSeconds, RestSeconds, AvoidRadius, AvoidSeconds, GuardOffset;
        public TravelerSupportKind? Support;
        public float? SupportRadius, Reduction, Resistance, ShieldHp, ShieldSeconds, SupportCooldown;
        public int? SupportTargets;
        // DECISION-0120: movement styles of peaceful Travelers, required per style/kind by TravelerDefinition.
        public TravelerMovementStyle? MovementStyle;
        public float? ZigzagAngleDegrees, ZigzagSeconds, EscapeDashDistance, EscapeDashSeconds, EscapeDashCooldownSeconds;
        public float? OrbitRadiusX, OrbitRadiusY, OrbitLeadDegrees, TeleportMinSeconds, TeleportMaxSeconds;
        // DECISION-0120: SpeedBurst/Heal support and the color of support/teleport effects.
        public float? EffectRadius, SpeedBonus, EffectSeconds, HealAmount, SupportVerticalScale;
        public float[] EffectColor;
        public TravelerEffectShape? EffectShape;
        public string Marker;
        public float[] Color;
    }
}
