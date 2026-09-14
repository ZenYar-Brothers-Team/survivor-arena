using System.Collections.Generic;
using Game.Character;

namespace Game.Progression
{
    public static class FixturePassiveCatalog
    {
        public static IReadOnlyList<PassiveProgressionDefinition> Create()
        {
            return new[]
            {
                Create("FIXTURE-PASSIVE-VITALITY", "Fixture Vitality", level =>
                    new CharacterStatModifier(maxHealthMultiplierBonus: level * 0.1f)),
                Create("FIXTURE-PASSIVE-HASTE", "Fixture Haste", level =>
                    new CharacterStatModifier(
                        movementSpeedMultiplierBonus: level * 0.05f,
                        activeSkillDamageMultiplierBonus: level * 0.05f,
                        activeSkillCooldownReductionBonus: level * 0.1f)),
                Create("FIXTURE-PASSIVE-MEMORY", "Fixture Memory", level =>
                    new CharacterStatModifier(
                        incomingDamageReductionBonus: level * 0.05f,
                        healthRestorationMultiplierBonus: level * 0.1f,
                        healthRegenerationPerSecondBonus: level * 0.2f,
                        disappearingXpRecoveryBonus: level * 0.1f,
                        pickedUpXpMultiplierBonus: level * 0.05f,
                        xpDropLifetimeBonusSeconds: level * 5f))
            };
        }

        private static PassiveProgressionDefinition Create(
            string id,
            string displayName,
            System.Func<int, CharacterStatModifier> levelFactory)
        {
            var levels = new CharacterStatModifier[BuildEntryDefinition.MaxLevel];
            for (var level = 1; level <= levels.Length; level++)
                levels[level - 1] = levelFactory(level);
            return new PassiveProgressionDefinition(id, displayName, levels);
        }
    }
}
