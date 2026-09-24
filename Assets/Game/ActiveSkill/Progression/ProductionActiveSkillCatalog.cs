using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Content.Json;

namespace Game.ActiveSkill
{
    /// <summary>
    /// Production active skills (IP-17). Generated from the approved FIELD-001 baseline by
    /// scripts/generate_field001_content.py; FIELD-001 startup packet F1-01 ships exactly
    /// SKILL-001…007/010/013/014 (DECISION-0050/0051). Fixture IDs are rejected here.
    /// </summary>
    public static class ProductionActiveSkillCatalog
    {
        public const string ResourcePath = "Content/ActiveSkills/ProductionActiveSkills";
        private static readonly Regex ProductionId = new Regex("^SKILL-[0-9]{3}$");

        public static IReadOnlyList<ActiveSkillProgressionDefinition> Create() => FromJson(JsonContentFile.ReadText(ResourcePath));

        public static IReadOnlyList<ActiveSkillProgressionDefinition> FromJson(string json)
        {
            var definitions = FixtureActiveSkillCatalog.FromJson(json);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in definitions)
            {
                var id = definition.Id.ToString();
                if (!ProductionId.IsMatch(id))
                    throw new InvalidOperationException($"Production active skill '{id}' must use a SKILL-### id.");
                if (!seen.Add(id))
                    throw new InvalidOperationException($"Duplicate production active skill '{id}'.");
                if (!definition.Icon.Id.IsValid)
                    throw new InvalidOperationException($"Production active skill '{id}' requires an icon visual.");
            }
            return definitions;
        }
    }
}
