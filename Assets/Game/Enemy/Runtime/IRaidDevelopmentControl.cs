using System.Collections.Generic;

namespace Game.Enemy
{
    /// <summary>Development-panel access to the roundup (DECISION-0155): launch a template now and read its live state.</summary>
    public interface IRaidDevelopmentControl
    {
        IReadOnlyList<RaidTemplateKind> Templates { get; }
        int TotalEnemyCount { get; }
        /// <summary>Ordinary enemies inside the crowd radius (one screen width from the player).</summary>
        int NearbyEnemyCount { get; }
        int TriggerEnemyCount { get; }
        bool RaidActive { get; }
        RaidTemplateKind ActiveTemplate { get; }
        float RaidRemainingSeconds { get; }
        /// <summary>Seconds until the next automatic roundup; it only runs down while a crowd exists.</summary>
        float NextRaidSeconds { get; }
        bool CountdownRunning { get; }
        /// <summary>Starts the template now; false when a roundup is already active or none is configured.</summary>
        bool TryStartRaid(RaidTemplateKind kind);
    }
}
