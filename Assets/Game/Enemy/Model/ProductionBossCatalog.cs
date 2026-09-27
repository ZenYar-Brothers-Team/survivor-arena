using System.Collections.Generic;

namespace Game.Enemy
{
    /// <summary>
    /// Production BOSS-001…010 and MIDBOSS-001…010 (IP-21: F1-06, DECISION-0063, DECISION-0066), generated from the approved baseline by
    /// scripts/generate_field001_content.py. Attack payloads are boss-owned inline carriers, so no ordinary
    /// enemy ID is imported; register <see cref="BossEncounterDefinition.OwnedAttacks"/> with the encounter.
    /// </summary>
    public static class ProductionBossCatalog
    {
        public const string ResourcePath = "Content/Bosses/ProductionBosses";

        /// <summary>Summons (DECISION-0066) resolve ordinary production enemies; phases still use only inline carriers.</summary>
        public static IReadOnlyList<BossEncounterDefinition> Create() =>
            FixtureBossCatalog.Load(ResourcePath, ProductionEnemyCatalog.Create());
    }
}
