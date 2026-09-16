using System;
using System.Collections.Generic;
using Game.Character;
using Game.Character.Json;
using Game.Content;
using Game.Content.Json;
using Game.Presentation;

namespace Game.Progression
{
    public static class FixtureCharacterDefinitionCatalog
    {
        private const string ResourcePath = "Content/Characters/FixtureCharacters";

        public static CharacterRoster Create()
        {
            var data = JsonContentFile.Load<CharacterDefinitionData[]>(ResourcePath);
            var definitions = new CharacterDefinition[data.Length];
            var unlocked = new List<ContentId>();
            for (var i = 0; i < data.Length; i++)
            {
                definitions[i] = ToDefinition(data[i]);
                if (data[i].InitiallyUnlocked)
                    unlocked.Add(definitions[i].Id);
            }

            return new CharacterRoster(definitions, unlocked);
        }

        private static CharacterDefinition ToDefinition(CharacterDefinitionData data)
        {
            if (data == null)
                throw new InvalidOperationException("Fixture character data cannot contain null entries.");
            if (data.BaseStats == null)
                throw new InvalidOperationException($"Fixture character '{data.Id}' has no base stats.");

            var stats = data.BaseStats;
            var weightsData = data.DraftWeights ?? Array.Empty<CharacterDraftWeightData>();
            var weights = new CharacterDraftWeight[weightsData.Length];
            for (var i = 0; i < weights.Length; i++)
                weights[i] = new CharacterDraftWeight(weightsData[i].SkillId, weightsData[i].Weight);

            return new CharacterDefinition(
                data.Id,
                data.DisplayName,
                new CharacterBaseStats(
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
                    stats.PickupRadius),
                data.StartingActiveSkillId,
                string.IsNullOrWhiteSpace(data.VisualId)
                    ? default
                    : new ContentRef<SpriteDefinition>(data.VisualId),
                string.IsNullOrWhiteSpace(data.MotionProfileId)
                    ? default
                    : new ContentRef<SpriteMotionProfile>(data.MotionProfileId),
                weights);
        }
    }
}
