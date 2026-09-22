using System;
using System.Collections.Generic;
using Game.Content;
using Game.Progression;
using Game.Presentation;

namespace Game.ActiveSkill
{
    public sealed class ActiveSkillProgressionDefinition : BuildEntryDefinition, IReferencesContent
    {
        private readonly ActiveSkillLevelDefinition[] _levels;

        public IReadOnlyList<ActiveSkillLevelDefinition> Levels => _levels;
        public ContentRef<SpriteDefinition> Icon { get; }

        public ActiveSkillProgressionDefinition(
            ContentId id,
            string displayName,
            params ActiveSkillLevelDefinition[] levels)
            : this(id, displayName, default, levels)
        {
        }

        public ActiveSkillProgressionDefinition(
            ContentId id,
            string displayName,
            ContentRef<SpriteDefinition> icon,
            params ActiveSkillLevelDefinition[] levels)
            : base(id, BuildEntryKind.ActiveSkill, displayName)
        {
            if (levels == null || levels.Length != MaxLevel)
                throw new ArgumentException($"Active skill progression requires exactly {MaxLevel} levels.", nameof(levels));
            for (var i = 0; i < levels.Length; i++)
            {
                if (levels[i] == null)
                    throw new ArgumentException("Skill levels cannot contain null.", nameof(levels));
                if (levels[i].Targeting.RandomSeed != levels[0].Targeting.RandomSeed)
                    throw new ArgumentException("A skill uses one explicit random seed across all levels.", nameof(levels));
                foreach (var wave in levels[i].Waves)
                    foreach (var effect in wave.Effects)
                        if (effect is ProjectileBurstEffect projectile && projectile.Layout == ProjectileLayout.IndependentRandom && !levels[i].Targeting.RandomSeed.HasValue)
                            throw new ArgumentException("Independent random directions require a configured skill seed.", nameof(levels));
            }

            _levels = (ActiveSkillLevelDefinition[])levels.Clone();
            Icon = icon;
        }

        public ActiveSkillLevelDefinition GetLevel(int level)
        {
            if (level < 1 || level > MaxLevel)
                throw new ArgumentOutOfRangeException(nameof(level));
            return _levels[level - 1];
        }

        public override DraftOptionPreview CreateDraftPreview(int currentLevel, int nextLevel)
        {
            var current = currentLevel == 0 ? null : GetLevel(currentLevel);
            var next = GetLevel(nextLevel);
            var values = new List<DraftValueChange>
            {
                new DraftValueChange("Base damage", current?.BaseDamage ?? 0f, next.BaseDamage),
                new DraftValueChange("Base cooldown", current?.CooldownSeconds ?? 0f, next.CooldownSeconds, " s"),
                new DraftValueChange("Waves", current?.Waves.Count ?? 0, next.Waves.Count)
            };
            if (current != null)
            {
                var before = SkillParameterPreview.Capture(current);
                foreach (var pair in SkillParameterPreview.Capture(next))
                {
                    before.TryGetValue(pair.Key, out var previous);
                    if (previous != pair.Value) values.Add(new DraftValueChange(pair.Key, previous, pair.Value));
                }
            }
            return new DraftOptionPreview(currentLevel, nextLevel, values);
        }

        // Only levels that opted into a distinct Visual get validated; levels
        // without one simply have no presentation asset to check yet.
        public IEnumerable<ContentReference> GetReferencedContent()
        {
            if (Icon.Id.IsValid)
                yield return Icon.ToReference();

            foreach (var level in _levels)
            {
                if (level.Visual.Id.IsValid)
                    yield return level.Visual.ToReference();
            }
        }
    }
}
