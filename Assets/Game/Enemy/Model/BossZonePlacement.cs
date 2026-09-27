namespace Game.Enemy
{
    /// <summary>Where a boss ground zone appears (Game Design «Враги, волны, элиты и боссы», DECISION-0066).</summary>
    public enum BossZonePlacement
    {
        /// <summary>One circle under the player at the moment it appears.</summary>
        AtPlayer,
        /// <summary>Several circles at random points near the player, not overlapping each other.</summary>
        AroundPlayer,
        /// <summary>Circles appear one after another under the player's current position: bombing along the path.</summary>
        Trail,
        /// <summary>One circle around the boss.</summary>
        AroundSelf,
        /// <summary>Everything is hit except inside a few safe circles near the player.</summary>
        SafeCircles
    }
}
