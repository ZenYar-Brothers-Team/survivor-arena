namespace Game.Enemy
{
    /// <summary>How an attack schedules its wind-ups (DECISION-0053/0054).</summary>
    public enum EnemyAttackCadence
    {
        /// <summary>First wind-up immediately; cooldown counts from each shot; aim follows the target during wind-up.</summary>
        CooldownAfterShot,

        /// <summary>
        /// First wind-up after one full cooldown from spawn; cooldown counts from the start of one wind-up to the
        /// start of the next; aim is fixed when the wind-up starts. Production ranged ordinary enemies use this.
        /// </summary>
        WindupStartToStart
    }
}
