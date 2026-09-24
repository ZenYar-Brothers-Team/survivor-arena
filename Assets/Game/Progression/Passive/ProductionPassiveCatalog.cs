using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Game.Progression
{
    /// <summary>
    /// Production passive items (IP-18). Generated from the approved FIELD-001 baseline by
    /// scripts/generate_field001_content.py; startup packet F1-02 ships exactly
    /// PASSIVE-001…005/007…009/011/012 (DECISION-0050/0051). Each level replaces the previous one.
    /// </summary>
    public static class ProductionPassiveCatalog
    {
        public const string ResourcePath = "Content/Passives/ProductionPassives";
        private static readonly Regex ProductionId = new Regex("^PASSIVE-[0-9]{3}$");

        public static IReadOnlyList<PassiveProgressionDefinition> Create()
        {
            var definitions = FixturePassiveCatalog.Load(ResourcePath);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in definitions)
            {
                var id = definition.Id.ToString();
                if (!ProductionId.IsMatch(id) || !seen.Add(id))
                    throw new InvalidOperationException($"Invalid or duplicate production passive '{id}'.");
                if (!definition.Icon.Id.IsValid)
                    throw new InvalidOperationException($"Production passive '{id}' requires an icon visual.");
            }
            return definitions;
        }
    }
}
