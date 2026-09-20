namespace Game.Enemy.Json
{
    // An omitted multiplier means "leave this stat unchanged"; the neutral value is owned
    // by WaveEnemyModifiers.Identity, not repeated here.
    public sealed class WaveEnemyModifiersData
    {
        public float? HealthMultiplier { get; set; }
        public float? SpeedMultiplier { get; set; }
        public float? ContactDamageMultiplier { get; set; }
        public float? AttackDamageMultiplier { get; set; }
    }
}
