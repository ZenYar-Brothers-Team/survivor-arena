using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Content.Json;

namespace Game.ActiveSkill
{
    /// <summary>
    /// Attack templates owned by set effects (SET-017 «Падающая звезда»). They are registered for the set host
    /// only and never enter the draft pool; one level is used, six identical levels satisfy the skill contract.
    /// </summary>
    public static class ProductionSetAttackCatalog
    {
        public const string ResourcePath = "Content/ActiveSkills/ProductionSetAttacks";
        private static readonly Regex TemplateId = new Regex("^SET-[0-9]{3}-ATTACK$");

        public static IReadOnlyList<ActiveSkillProgressionDefinition> Create()
        {
            var definitions = FixtureActiveSkillCatalog.FromJson(JsonContentFile.ReadText(ResourcePath));
            foreach (var definition in definitions)
                if (!TemplateId.IsMatch(definition.Id.ToString()))
                    throw new InvalidOperationException($"Set attack template '{definition.Id}' must use a SET-###-ATTACK id.");
            return definitions;
        }
    }
}
