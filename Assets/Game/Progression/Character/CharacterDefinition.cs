using System;
using System.Collections.Generic;
using Game.Character;
using Game.Content;
using Game.Presentation;

namespace Game.Progression
{
    public sealed class CharacterDefinition : IContentDefinition, IReferencesContent
    {
        private readonly Dictionary<ContentId, float> _draftWeights;

        public ContentId Id { get; }
        public string DisplayName { get; }
        public CharacterBaseStats BaseStats { get; }
        public ContentRef<BuildEntryDefinition> StartingActiveSkill { get; }
        public ContentRef<SpriteDefinition> Visual { get; }
        public ContentRef<SpriteMotionProfile> MotionProfile { get; }
        public IReadOnlyDictionary<ContentId, float> DraftWeights => _draftWeights;

        public CharacterDefinition(
            ContentId id,
            string displayName,
            CharacterBaseStats baseStats,
            ContentId startingActiveSkillId,
            params CharacterDraftWeight[] draftWeights)
            : this(
                id,
                displayName,
                baseStats,
                startingActiveSkillId,
                default,
                default,
                draftWeights)
        {
        }

        public CharacterDefinition(
            ContentId id,
            string displayName,
            CharacterBaseStats baseStats,
            ContentId startingActiveSkillId,
            ContentRef<SpriteDefinition> visual,
            ContentRef<SpriteMotionProfile> motionProfile,
            params CharacterDraftWeight[] draftWeights)
        {
            if (!id.IsValid)
                throw new ArgumentException("Character requires a valid content id.", nameof(id));
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Character display name cannot be empty.", nameof(displayName));
            if (!startingActiveSkillId.IsValid)
                throw new ArgumentException("Character requires a valid starting active skill id.", nameof(startingActiveSkillId));
            if (draftWeights == null)
                throw new ArgumentNullException(nameof(draftWeights));

            baseStats.Validate();
            Id = id;
            DisplayName = displayName;
            BaseStats = baseStats;
            StartingActiveSkill = new ContentRef<BuildEntryDefinition>(startingActiveSkillId);
            Visual = visual;
            MotionProfile = motionProfile;
            _draftWeights = new Dictionary<ContentId, float>();
            for (var i = 0; i < draftWeights.Length; i++)
            {
                var weight = draftWeights[i];
                if (!_draftWeights.TryAdd(weight.SkillId, weight.Weight))
                    throw new ArgumentException($"Duplicate draft weight for skill '{weight.SkillId}'.", nameof(draftWeights));
            }
        }

        public float GetDraftWeight(ContentId skillId)
        {
            return _draftWeights.TryGetValue(skillId, out var weight) ? weight : 1f;
        }

        public BuildEntryDefinition ResolveStartingActiveSkill(ContentRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            var definition = StartingActiveSkill.Resolve(registry);
            if (definition.Kind != BuildEntryKind.ActiveSkill)
                throw new InvalidOperationException(
                    $"Character '{Id}' starting entry '{definition.Id}' is not an active skill.");
            return definition;
        }

        public void ValidateDraftSkillReferences(ContentRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            foreach (var id in _draftWeights.Keys)
            {
                var definition = registry.Get<BuildEntryDefinition>(id);
                if (definition.Kind != BuildEntryKind.ActiveSkill)
                    throw new InvalidOperationException(
                        $"Character '{Id}' draft weight entry '{id}' is not an active skill.");
            }
        }

        public IEnumerable<ContentReference> GetReferencedContent()
        {
            yield return StartingActiveSkill.ToReference();
            if (Visual.Id.IsValid)
                yield return Visual.ToReference();
            if (MotionProfile.Id.IsValid)
                yield return MotionProfile.ToReference();
            foreach (var id in _draftWeights.Keys)
                yield return new ContentRef<BuildEntryDefinition>(id).ToReference();
        }
    }
}
