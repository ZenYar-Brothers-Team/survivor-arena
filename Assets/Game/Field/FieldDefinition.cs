using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Enemy;

namespace Game.Field
{
    /// <summary>IP-16 field data. Difficulty is authored metadata; thumbnail is a labelled placeholder until art approval.</summary>
    public sealed class FieldDefinition : IContentDefinition, IReferencesContent
    {
        public ContentId Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public string ThumbnailPlaceholder { get; }
        public int Difficulty { get; }
        public string UnlockDescription { get; }
        public ContentRef<FieldEnvironmentDefinition> Environment { get; }
        public ContentRef<WaveTimelineDefinition> Timeline { get; }
        public ContentRef<BossEncounterDefinition> FinalBoss { get; }
        public ContentRef<BossEncounterDefinition>? MidBoss { get; }
        public ContentRef<FieldTravelerScheduleDefinition>? Travelers { get; }
        public IReadOnlyList<ContentRef<EnemyDefinition>> Enemies { get; }

        public FieldDefinition(ContentId id, string displayName, string description, string thumbnailPlaceholder,
            int difficulty, string unlockDescription, ContentId environment, ContentId timeline, ContentId finalBoss,
            IEnumerable<ContentId> enemies, ContentId? midBoss = null, ContentId? travelers = null)
        {
            if (!id.IsValid || !environment.IsValid || !timeline.IsValid || !finalBoss.IsValid ||
                (midBoss.HasValue && !midBoss.Value.IsValid) || (travelers.HasValue && !travelers.Value.IsValid))
                throw new ArgumentException("Field requires valid typed references.");
            if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(thumbnailPlaceholder) || string.IsNullOrWhiteSpace(unlockDescription))
                throw new ArgumentException("Field requires display name, description, thumbnail placeholder and unlock description.");
            NumericValidation.ValidateRange(difficulty, 1, 5, nameof(difficulty));
            var ids = new List<ContentId>(enemies ?? throw new ArgumentNullException(nameof(enemies)));
            if (ids.Count == 0 || ids.Any(value => !value.IsValid) || ids.Distinct().Count() != ids.Count)
                throw new ArgumentException("Field requires a unique nonempty enemy pool.", nameof(enemies));
            Id = id;
            DisplayName = displayName;
            Description = description;
            ThumbnailPlaceholder = thumbnailPlaceholder;
            Difficulty = difficulty;
            UnlockDescription = unlockDescription;
            Environment = new ContentRef<FieldEnvironmentDefinition>(environment);
            Timeline = new ContentRef<WaveTimelineDefinition>(timeline);
            FinalBoss = new ContentRef<BossEncounterDefinition>(finalBoss);
            MidBoss = midBoss.HasValue ? new ContentRef<BossEncounterDefinition>(midBoss.Value) : (ContentRef<BossEncounterDefinition>?)null;
            Travelers = travelers.HasValue ? new ContentRef<FieldTravelerScheduleDefinition>(travelers.Value) : (ContentRef<FieldTravelerScheduleDefinition>?)null;
            Enemies = ids.Select(value => new ContentRef<EnemyDefinition>(value)).ToList().AsReadOnly();
        }

        public IEnumerable<ContentReference> GetReferencedContent()
        {
            yield return Environment.ToReference();
            yield return Timeline.ToReference();
            yield return FinalBoss.ToReference();
            if (MidBoss.HasValue) yield return MidBoss.Value.ToReference();
            if (Travelers.HasValue) yield return Travelers.Value.ToReference();
            foreach (var enemy in Enemies) yield return enemy.ToReference();
        }

        /// <summary>Resolve before touching any run subsystem; rejects cross-field enemy pools and mismatched boss hooks.</summary>
        public ResolvedFieldConfiguration Resolve(ContentRegistry registry)
        {
            var environment = Environment.Resolve(registry);
            var timeline = Timeline.Resolve(registry);
            var enemies = Enemies.Select(reference => reference.Resolve(registry)).ToList();
            var ids = new HashSet<ContentId>(enemies.Select(enemy => enemy.Id));
            if (timeline.GetReferencedContent().Any(reference => !ids.Contains(reference.Id)))
                throw new InvalidOperationException($"Field '{Id}' timeline uses an enemy outside its pool.");
            var bosses = new List<BossEncounterDefinition> { FinalBoss.Resolve(registry) };
            if (bosses[0].Hook != WaveHookKind.FinalBoss)
                throw new InvalidOperationException("Final boss reference must bind FinalBoss.");
            if (MidBoss.HasValue)
            {
                var mid = MidBoss.Value.Resolve(registry);
                if (mid.Hook != WaveHookKind.MidBoss) throw new InvalidOperationException("Midboss reference must bind MidBoss.");
                bosses.Add(mid);
            }
            if (!timeline.Hooks.Any(hook => hook.Kind == WaveHookKind.FinalBoss) ||
                timeline.Hooks.Any(hook => !bosses.Any(boss => boss.Hook == hook.Kind)) ||
                bosses.Any(boss => !timeline.Hooks.Any(hook => hook.Kind == boss.Hook)))
                throw new InvalidOperationException("Field encounter definitions and timeline hooks must match.");
            return new ResolvedFieldConfiguration(this, environment, timeline, enemies, bosses,
                Travelers.HasValue ? Travelers.Value.Resolve(registry) : null);
        }
    }
}
