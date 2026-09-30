namespace Game.Enemy.Json
{
    // No tuning defaults live here: every field the chosen movement kind reads must be
    // written in the config file (FixtureEnemyCatalog rejects a missing one by name).
    // Fields the kind never reads may be omitted.
    public sealed class EnemyMovementProfileData
    {
        public string Kind { get; set; }
        public float? PreferredDistance { get; set; }
        public float? DistanceTolerance { get; set; }
        public float? LateralStrength { get; set; }
        public float? CycleSeconds { get; set; }
        public float? DirectPursuitSeconds { get; set; }
        public float? BlockedTriggerSeconds { get; set; }
        public float? BlockedProgressFraction { get; set; }
        public float? SidestepSeconds { get; set; }
        public float? SidestepCooldownSeconds { get; set; }
        public float? SidestepNearDistance { get; set; }
        public float? SidestepNearSeconds { get; set; }
        public float? TurnResponseSeconds { get; set; }
        public float? DashTelegraphSeconds { get; set; }
        public float? DashDurationSeconds { get; set; }
        public float? DashCooldownSeconds { get; set; }
        public float? DashSpeedMultiplier { get; set; }
        public float? RepositionSeconds { get; set; }
        /// <summary>TelegraphedDash only, optional: dashes per sequence (default 1) and follow-up telegraph.</summary>
        public int? DashCount { get; set; }
        public float? FollowUpTelegraphSeconds { get; set; }
        /// <summary>TelegraphedDash only, optional: false hides the dash aim line (neutral: shown).</summary>
        public bool? ShowDashTelegraphLine { get; set; }
        /// <summary>TelegraphedDash only, optional (DECISION-0118): fixed dash length; when present dashSpeedMultiplier may be omitted.</summary>
        public float? DashDistance { get; set; }
        public float? DashTelegraphWidth { get; set; }
        /// <summary>TelegraphedDash only, optional: drawn line length; omitted draws the whole dash.</summary>
        public float? DashTelegraphLength { get; set; }
        /// <summary>TelegraphedDash only, optional: shove radius; when present the shove distance and seconds are required.</summary>
        public float? DashShoveRadius { get; set; }
        public float? DashShoveDistance { get; set; }
        public float? DashShoveSeconds { get; set; }
    }
}
