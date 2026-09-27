namespace Game.Enemy
{
    /// <summary>Shapes the boss hazard presentation draws (DECISION-0066).</summary>
    public enum BossHazardVisualKind
    {
        /// <summary>Outline of a filling zone.</summary>
        ZoneEdge,
        /// <summary>Disc that grows from the center while a zone fills.</summary>
        ZoneFill,
        /// <summary>Expanding ring right after a zone hits.</summary>
        ZoneImpact,
        /// <summary>Burning ground left by a zone.</summary>
        Burning,
        /// <summary>Area covered by a safe-circles attack (everything but the circles).</summary>
        DangerWash,
        /// <summary>A safe circle of a safe-circles attack.</summary>
        SafeCircle,
        /// <summary>Thin warning line of a beam.</summary>
        BeamTelegraph,
        /// <summary>Active beam.</summary>
        BeamActive,
        /// <summary>Ground marker where a summoned enemy will appear.</summary>
        SummonMarker
    }
}
