using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Game.Enemy
{
    /// <summary>
    /// Production ordinary enemies (IP-20). FIELD-001 packet F1-04 ships exactly ENEMY-001…005 and ENEMY-007
    /// (DECISION-0052), generated from the approved baseline by scripts/generate_field001_content.py.
    /// </summary>
    public static class ProductionEnemyCatalog
    {
        public const string ResourcePath = "Content/Enemies/ProductionEnemies";
        private static readonly Regex ProductionId = new Regex("^ENEMY-[0-9]{3}$");

        public static IReadOnlyList<EnemyDefinition> Create()
        {
            var definitions = FixtureEnemyCatalog.Load(ResourcePath);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in definitions)
            {
                var id = definition.Id.ToString();
                if (!ProductionId.IsMatch(id) || !seen.Add(id))
                    throw new InvalidOperationException($"Invalid or duplicate production enemy '{id}'.");
            }
            return definitions;
        }
    }
}
