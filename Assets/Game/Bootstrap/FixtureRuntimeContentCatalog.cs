using System.Collections.Generic;
using Game.ActiveSkill;
using Game.Content;
using Game.Enemy;
using Game.Progression;

namespace Game.Bootstrap
{
    public sealed class FixtureRuntimeContentCatalog
    {
        public ContentRegistry Registry { get; }
        public IReadOnlyList<BuildEntryDefinition> BuildEntries { get; }
        public IReadOnlyList<ActiveSkillProgressionDefinition> ActiveSkills { get; }
        public IReadOnlyList<PassiveProgressionDefinition> Passives { get; }
        public IReadOnlyList<EnemyDefinition> Enemies { get; }

        private FixtureRuntimeContentCatalog(
            ContentRegistry registry,
            IReadOnlyList<BuildEntryDefinition> buildEntries,
            IReadOnlyList<ActiveSkillProgressionDefinition> activeSkills,
            IReadOnlyList<PassiveProgressionDefinition> passives,
            IReadOnlyList<EnemyDefinition> enemies)
        {
            Registry = registry;
            BuildEntries = buildEntries;
            ActiveSkills = activeSkills;
            Passives = passives;
            Enemies = enemies;
        }

        public static FixtureRuntimeContentCatalog Create()
        {
            var activeSkills = FixtureActiveSkillCatalog.Create();
            var passives = FixturePassiveCatalog.Create();
            var enemies = new[]
            {
                new EnemyDefinition(
                    "FIXTURE-ENEMY-SEEKER",
                    maxHealth: 10f,
                    collisionSize: 1f,
                    movementSpeed: 1f,
                    contactDamage: 1f,
                    contactDamageInterval: 1f,
                    experienceReward: 1f)
            };

            var buildEntries = new List<BuildEntryDefinition>(activeSkills.Count + passives.Count);
            var allDefinitions = new List<IContentDefinition>(buildEntries.Capacity + enemies.Length);
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
            for (var i = 0; i < enemies.Length; i++)
                allDefinitions.Add(enemies[i]);

            return new FixtureRuntimeContentCatalog(
                ContentRegistry.BuildFrom(allDefinitions),
                buildEntries,
                activeSkills,
                passives,
                enemies);
        }
    }
}
