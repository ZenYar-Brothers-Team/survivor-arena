using System;
using System.Linq;
using Game.Character.Json;
using Game.Content.Json;

namespace Game.Character
{
    // Non-production character content, config-driven instead of hardcoded: see
    // Resources/Content/Characters/FixtureCharacters.json and AGENTS.md's content-config rule.
    public static class FixtureCharacterCatalog
    {
        private const string ResourcePath = "Content/Characters/FixtureCharacters";
        public const string DefaultCharacterId = "FIXTURE-CHARACTER-AGILE";

        public static CharacterBaseStats CreateDefault()
        {
            return Create(DefaultCharacterId);
        }

        public static CharacterBaseStats Create(string id)
        {
            var all = JsonContentFile.Load<CharacterDefinitionData[]>(ResourcePath);
            var data = all.FirstOrDefault(entry => entry.Id == id);
            if (data == null)
                throw new InvalidOperationException($"No fixture character with id '{id}' in {ResourcePath}.json.");

            if (data.BaseStats == null)
                throw new InvalidOperationException($"Fixture character '{id}' has no base stats in {ResourcePath}.json.");

            var stats = data.BaseStats;
            return new CharacterBaseStats(
                stats.MaxHealth,
                stats.MovementSpeed,
                stats.ActiveSkillDamageMultiplier,
                stats.ActiveSkillCooldownMultiplier,
                stats.IncomingDamageMultiplier,
                stats.HealthRestorationMultiplier,
                stats.HealthRegenerationPerSecond,
                stats.DisappearingXpRecovery,
                stats.PickedUpXpMultiplier,
                stats.XpDropLifetimeBonusSeconds,
                stats.PickupRadius);
        }
    }
}
