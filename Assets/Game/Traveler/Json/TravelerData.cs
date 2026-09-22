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
        public string Marker;
        public float[] Color;
    }
}
