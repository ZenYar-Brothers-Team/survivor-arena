namespace Game.Enemy
{
    /// <summary>
    /// A moment of an enemy's own special action worth a sound or cue: a ranged volley, a wind-up, a dash or a boss special.
    /// Raised once per moment (not per projectile); consumers decide what, if anything, to play.
    /// </summary>
    public enum EnemyActionKind
    {
        /// <summary>A ranged attack began its wind-up telegraph.</summary>
        AttackWindup,
        /// <summary>A ranged attack released its volley (one event per volley).</summary>
        Shot,
        /// <summary>A dash began its telegraph line.</summary>
        DashWindup,
        /// <summary>The dash itself started.</summary>
        DashStart,
        /// <summary>A boss step or dash end started a hazard zone.</summary>
        ZoneStart,
        /// <summary>A boss step started a beam.</summary>
        BeamStart,
        /// <summary>A boss step called a summon.</summary>
        SummonStart,
        /// <summary>A boss teleport-slam showed its landing marker.</summary>
        TeleportWindup,
        /// <summary>A boss teleport-slam landed.</summary>
        TeleportSlam
    }
}
