using System.Collections.Generic;

namespace Game.Enemy
{
    /// <summary>
    /// Production BOSS-001 and MIDBOSS-001 (IP-21, F1-06), generated from the approved baseline by
    /// scripts/generate_field001_content.py. Attack payloads are boss-owned inline carriers, so no ordinary
    /// enemy ID is imported; register <see cref="BossEncounterDefinition.OwnedAttacks"/> with the encounter.
    /// </summary>
    public static class ProductionBossCatalog
    {
        public const string ResourcePath = "Content/Bosses/ProductionBosses";

        public static IReadOnlyList<BossEncounterDefinition> Create() =>
            FixtureBossCatalog.Load(ResourcePath, new List<EnemyDefinition>());
    }
}
