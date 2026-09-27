namespace Game.Enemy
{
    /// <summary>Base direction of a dash-end volley entry (DECISION-0066, E4).</summary>
    public enum EnemyDashVolleyOrientation
    {
        /// <summary>Toward the player when the dash series ends.</summary>
        TowardPlayer,
        /// <summary>Back along the last dash line (MIDBOSS-007 rear fan).</summary>
        AwayFromDash,
        /// <summary>Centered on the boss itself; ground zones only.</summary>
        Self
    }
}
