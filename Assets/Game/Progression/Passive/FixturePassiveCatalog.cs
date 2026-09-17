using System.Collections.Generic;
using Game.Character;
using Game.Content.Json;
using Game.Progression.Json;

namespace Game.Progression
{
    // Non-production passive content, config-driven instead of hardcoded: see
    // Resources/Content/Passives/FixturePassives.json and AGENTS.md's content-config rule.
    public static class FixturePassiveCatalog
    {
        private const string ResourcePath = "Content/Passives/FixturePassives";

        public static IReadOnlyList<PassiveProgressionDefinition> Create()
        {
            var data = JsonContentFile.Load<PassiveProgressionData[]>(ResourcePath);
            var definitions = new PassiveProgressionDefinition[data.Length];
            for (var i = 0; i < data.Length; i++)
                definitions[i] = ToDefinition(data[i]);

            return definitions;
        }

        private static PassiveProgressionDefinition ToDefinition(PassiveProgressionData data)
        {
            var levels = new CharacterStatModifier[data.Levels.Length];
            for (var i = 0; i < levels.Length; i++)
                levels[i] = ToModifier(data.Levels[i]);

            return new PassiveProgressionDefinition(data.Id, data.DisplayName, levels);
        }

        private static CharacterStatModifier ToModifier(CharacterStatModifierData data)
        {
            return new CharacterStatModifier(
                data.MaxHealthMultiplierBonus,
                data.MovementSpeedMultiplierBonus,
                data.ActiveSkillDamageMultiplierBonus,
                data.ActiveSkillCooldownReductionBonus,
                data.IncomingDamageReductionBonus,
                data.HealthRestorationMultiplierBonus,
                data.HealthRegenerationPerSecondBonus,
                data.DisappearingXpRecoveryBonus,
                data.PickedUpXpMultiplierBonus,
                data.XpDropLifetimeBonusSeconds);
        }
    }
}
