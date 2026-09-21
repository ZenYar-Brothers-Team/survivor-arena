using Game.Combat;

namespace Game.Enemy.Json
{
    // JSON shape for EnemyDefinition. Field names are camelCase in the config
    // files; Newtonsoft matches them to these properties case-insensitively.
    public sealed class EnemyDefinitionData
    {
        public float? KnockbackResistance { get; set; }
        public CombatControlData DashContactControls { get; set; }
        public CombatControlData ContactControls { get; set; }
        public string Id { get; set; }
        public float MaxHealth { get; set; }
        public float CollisionSize { get; set; }
        public float MovementSpeed { get; set; }
        public float ContactDamage { get; set; }
        public float ContactDamageInterval { get; set; }
        public float ExperienceReward { get; set; }
        public string VisualId { get; set; }
        public string MotionProfileId { get; set; }
        public EnemyMovementProfileData Movement { get; set; }
        public EnemyAttackProfileData Attack { get; set; }
    }
}
