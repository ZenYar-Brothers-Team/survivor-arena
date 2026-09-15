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
        public const string DefaultCharacterId = "FIXTURE-CHARACTER-DEFAULT";

        public static CharacterBaseStats CreateDefault()
        {
            return Create(DefaultCharacterId);
        }

        public static CharacterBaseStats Create(string id)
        {
            var all = JsonContentFile.Load<CharacterBaseStatsData[]>(ResourcePath);
            var data = all.FirstOrDefault(entry => entry.Id == id);
            if (data == null)
                throw new InvalidOperationException($"No fixture character with id '{id}' in {ResourcePath}.json.");

            return new CharacterBaseStats(
                data.MaxHealth,
                data.MovementSpeed,
                data.ActiveSkillDamageMultiplier,
                data.ActiveSkillCooldownMultiplier,
                data.IncomingDamageMultiplier,
                data.HealthRestorationMultiplier,
                data.HealthRegenerationPerSecond,
                data.DisappearingXpRecovery,
                data.PickedUpXpMultiplier,
                data.XpDropLifetimeBonusSeconds,
                data.PickupRadius);
        }
    }
}
