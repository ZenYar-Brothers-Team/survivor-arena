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
            var reasons = new Dictionary<ContentId, string>();
            for (var i = 0; i < data.Length; i++)
            {
                definitions[i] = ToDefinition(data[i]);
                if (data[i].InitiallyUnlocked)
                    unlocked.Add(definitions[i].Id);
                else
                    reasons.Add(definitions[i].Id, data[i].LockReason);
            }

            return new CharacterRoster(definitions, new FixtureCharacterAccessProvider(unlocked, reasons));
        }

        public static CharacterComparisonBaseline CreateBaseline()
        {
            var data = JsonContentFile.Load<CharacterBaselineData>("Content/Characters/FixtureCharacterBaseline");
            if (data?.BaseStats == null) throw new InvalidOperationException("Character baseline stats are required.");
            return new CharacterComparisonBaseline(data.Id, CharacterBaseStatsMapper.Map(data.BaseStats));
        }

        public static CharacterPresentation MapPresentation(CharacterPresentationData data)
        {
            if (data?.Highlights == null) throw new InvalidOperationException("Presentation and explicit highlights are required.");
            var fields = new CharacterStatField[data.Highlights.Length];
            for (var i = 0; i < fields.Length; i++)
                if (!Enum.TryParse(data.Highlights[i], true, out fields[i]) ||
                    !string.Equals(Enum.GetName(typeof(CharacterStatField), fields[i]), data.Highlights[i], StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Unknown character highlight '{data.Highlights[i]}'.");
            return new CharacterPresentation(data.Role, data.BaselineId, data.CropId, data.IconId, fields);
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
                CharacterBaseStatsMapper.Map(stats),
                data.StartingActiveSkillId,
                string.IsNullOrWhiteSpace(data.VisualId)
                    ? default
                    : new ContentRef<SpriteDefinition>(data.VisualId),
                string.IsNullOrWhiteSpace(data.MotionProfileId)
                    ? default
                    : new ContentRef<SpriteMotionProfile>(data.MotionProfileId),
                MapPresentation(data.Presentation),
                weights);
        }
    }
}
