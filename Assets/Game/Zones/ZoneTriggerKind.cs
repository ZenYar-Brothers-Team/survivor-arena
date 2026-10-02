namespace Game.Zones
{
    /// <summary>A moment when a zone visibly does something the player should hear.</summary>
    public enum ZoneTriggerKind
    {
        /// <summary>An altar or seal switched on (its active window began) near the player.</summary>
        Activated,
        /// <summary>A shrine filled and fired its reward.</summary>
        ShrineReward,
        /// <summary>A strike altar's circles hit.</summary>
        StrikeImpact,
        /// <summary>A burst zone went off.</summary>
        BurstFired,
        /// <summary>A portal teleported the player.</summary>
        PortalJump
    }
}
