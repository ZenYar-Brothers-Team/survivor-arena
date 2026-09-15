using System.Collections.Generic;
using System.Linq;
using Game.ActiveSkill;
using Game.Character;
using Game.Content;
using Game.Enemy;
using Game.Presentation;
using Game.Progression;

namespace Game.Bootstrap
{
    public sealed class FixtureRuntimeContentCatalog
    {
        // All content here is immutable once built, so it only needs parsing once
        // per game session (this static field resets on domain reload, i.e. once
        // per Editor Play session or once per real process launch) rather than
        // once per run/scene reload.
        private static FixtureRuntimeContentCatalog _cached;

        public ContentRegistry Registry { get; }
        public IReadOnlyList<BuildEntryDefinition> BuildEntries { get; }
        public IReadOnlyList<ActiveSkillProgressionDefinition> ActiveSkills { get; }
        public IReadOnlyList<PassiveProgressionDefinition> Passives { get; }
        public IReadOnlyList<EnemyDefinition> Enemies { get; }
        public CharacterBaseStats DefaultCharacterBaseStats { get; }

        private FixtureRuntimeContentCatalog(
            ContentRegistry registry,
            IReadOnlyList<BuildEntryDefinition> buildEntries,
            IReadOnlyList<ActiveSkillProgressionDefinition> activeSkills,
            IReadOnlyList<PassiveProgressionDefinition> passives,
            IReadOnlyList<EnemyDefinition> enemies,
            CharacterBaseStats defaultCharacterBaseStats)
        {
            Registry = registry;
            BuildEntries = buildEntries;
            ActiveSkills = activeSkills;
            Passives = passives;
            Enemies = enemies;
            DefaultCharacterBaseStats = defaultCharacterBaseStats;
        }

        public static FixtureRuntimeContentCatalog Create()
        {
            if (_cached != null)
                return _cached;

            var activeSkills = FixtureActiveSkillCatalog.Create();
            var passives = FixturePassiveCatalog.Create();
            var enemies = FixtureEnemyCatalog.Create();
            var defaultCharacterBaseStats = FixtureCharacterCatalog.CreateDefault();

            var buildEntries = new List<BuildEntryDefinition>(activeSkills.Count + passives.Count);
            var allDefinitions = new List<IContentDefinition>(buildEntries.Capacity + enemies.Count);
            for (var i = 0; i < activeSkills.Count; i++)
            {
                buildEntries.Add(activeSkills[i]);
                allDefinitions.Add(activeSkills[i]);
            }
            for (var i = 0; i < passives.Count; i++)
            {
                buildEntries.Add(passives[i]);
                allDefinitions.Add(passives[i]);
            }
            for (var i = 0; i < enemies.Count; i++)
                allDefinitions.Add(enemies[i]);

            // Backfill a placeholder sprite for every visual reference declared above,
            // so fixture content never has to remember to register one by hand; real
            // content (IP-17+) registers actual art here instead and the registry's
            // own reference validation catches anything still missing.
            var visualIds = allDefinitions
                .OfType<IReferencesContent>()
                .SelectMany(definition => definition.GetReferencedContent())
                .Select(reference => reference.Id);
            allDefinitions.AddRange(FixtureSpriteCatalog.CreateFor(visualIds));

            _cached = new FixtureRuntimeContentCatalog(
                ContentRegistry.BuildFrom(allDefinitions),
                buildEntries,
                activeSkills,
                passives,
                enemies,
                defaultCharacterBaseStats);
            return _cached;
        }
    }
}
