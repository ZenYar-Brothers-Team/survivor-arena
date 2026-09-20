using System;
using System.Linq;
using NUnit.Framework;

namespace Game.Enemy.Tests
{
    public class WaveDefinitionTests
    {
        [Test]
        public void Phase_RejectsInvalidConfiguration()
        {
            var entry = WaveTestData.Entry("FIXTURE-ENEMY-A");
            var one = new[] { entry };

            Assert.Throws<ArgumentException>(() => new WavePhaseDefinition(default, "Name", WavePhaseTag.Ordinary, 5f, 1f, 3, one));
            Assert.Throws<ArgumentException>(() => new WavePhaseDefinition("FIXTURE-P", " ", WavePhaseTag.Ordinary, 5f, 1f, 3, one));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WavePhaseDefinition("FIXTURE-P", "N", (WavePhaseTag)99, 5f, 1f, 3, one));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WavePhaseDefinition("FIXTURE-P", "N", WavePhaseTag.Ordinary, 0f, 1f, 3, one));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WavePhaseDefinition("FIXTURE-P", "N", WavePhaseTag.Ordinary, 5f, 0f, 3, one));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WavePhaseDefinition("FIXTURE-P", "N", WavePhaseTag.Ordinary, 5f, 1f, 0, one));
            Assert.Throws<ArgumentException>(() => new WavePhaseDefinition("FIXTURE-P", "N", WavePhaseTag.Ordinary, 5f, 1f, 3, Array.Empty<WaveCompositionEntry>()));
            Assert.Throws<ArgumentException>(() => new WavePhaseDefinition("FIXTURE-P", "N", WavePhaseTag.Ordinary, 5f, 1f, 3, new[] { entry, entry }));
        }

        [Test]
        public void CompositionAndModifiers_RejectInvalidValues()
        {
            Assert.Throws<ArgumentException>(() => new WaveCompositionEntry(default, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveCompositionEntry("FIXTURE-ENEMY-A", 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveEnemyModifiers(healthMultiplier: 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveEnemyModifiers(speedMultiplier: -1f));
            Assert.IsTrue(WaveEnemyModifiers.Identity.IsIdentity);
            Assert.IsFalse(new WaveEnemyModifiers(speedMultiplier: 1.2f).IsIdentity);
        }

        [Test]
        public void Timeline_RejectsInvalidPhasesAndHooks()
        {
            var phase = WaveTestData.Phase("FIXTURE-P", WavePhaseTag.Ordinary, 5f, 1f, 3, null, WaveTestData.Entry("FIXTURE-ENEMY-A"));

            Assert.Throws<ArgumentException>(() => new WaveTimelineDefinition("FIXTURE-T", 1, WaveTestData.SpawnRadius, Array.Empty<WavePhaseDefinition>()));
            Assert.Throws<ArgumentException>(() => new WaveTimelineDefinition("FIXTURE-T", 1, WaveTestData.SpawnRadius, new[] { phase, phase }));
            Assert.Throws<ArgumentException>(() => new WaveTimelineDefinition(
                "FIXTURE-T",
                1,
                WaveTestData.SpawnRadius,
                new[] { phase },
                new[]
                {
                    new WaveHookDefinition(WaveHookKind.MidBoss, 1f),
                    new WaveHookDefinition(WaveHookKind.MidBoss, 2f)
                }));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveTimelineDefinition("FIXTURE-T", 1, 0f, new[] { phase }));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WaveHookDefinition(WaveHookKind.FinalBoss, -1f));
        }

        [Test]
        public void Timeline_ExposesTotalDurationAndReferencedEnemies()
        {
            var timeline = WaveTestData.ThreePhaseTimeline();

            Assert.AreEqual(30f, timeline.TotalDurationSeconds);
            var referenced = timeline.GetReferencedContent().Select(reference => reference.Id.ToString()).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "FIXTURE-ENEMY-A", "FIXTURE-ENEMY-B" }, referenced);
            Assert.IsTrue(timeline.GetReferencedContent().All(reference => reference.ExpectedType == typeof(EnemyDefinition)));
        }

        [Test]
        public void Scaler_AppliesIndependentMultipliersAndKeepsIdentity()
        {
            var attack = new EnemyAttackProfile(EnemyProjectilePattern.Fan, 4f, 2f, 3f, 3f, 3, 40f);
            var movement = new EnemyMovementProfile(EnemyMovementKind.KeepDistance, 4f, 0.5f);
            var source = new EnemyDefinition("FIXTURE-ENEMY-A", 10f, 1f, 2f, 3f, 1f, 1f, default, movement, attack);

            Assert.AreSame(source, WaveEnemyScaler.Apply(source, WaveEnemyModifiers.Identity));

            var scaled = WaveEnemyScaler.Apply(source, new WaveEnemyModifiers(0.5f, 1.5f, 2f, 0.25f));

            Assert.AreEqual(source.Id, scaled.Id);
            Assert.AreEqual(5f, scaled.MaxHealth);
            Assert.AreEqual(3f, scaled.MovementSpeed);
            Assert.AreEqual(6f, scaled.ContactDamage);
            Assert.AreEqual(1f, scaled.Attack.Damage);
            Assert.AreEqual(EnemyProjectilePattern.Fan, scaled.Attack.Pattern);
            Assert.AreEqual(3, scaled.Attack.ProjectileCount);
            Assert.AreSame(movement, scaled.Movement);
            Assert.IsNull(WaveEnemyScaler.Apply(WaveTestData.Enemy("FIXTURE-ENEMY-B"), new WaveEnemyModifiers(2f)).Attack);
        }

        [Test]
        public void FixtureTimeline_CoversRhythmAndReferencesKnownEnemies()
        {
            var timeline = FixtureWaveTimelineCatalog.Create();
            var enemyIds = FixtureEnemyCatalog.Create().Select(enemy => enemy.Id).ToList();

            foreach (var reference in timeline.GetReferencedContent())
                CollectionAssert.Contains(enemyIds, reference.Id);

            var tags = timeline.Phases.Select(phase => phase.Tag).ToList();
            CollectionAssert.IsSubsetOf(
                new[] { WavePhaseTag.Ordinary, WavePhaseTag.Pressure, WavePhaseTag.Elite, WavePhaseTag.Rest },
                tags);
            Assert.Greater(timeline.Phases.Count, 4);
            Assert.AreEqual(24680, timeline.Seed, "Seed comes from the JSON, not a component default.");
            Assert.AreEqual(8f, timeline.SpawnRadius, "Spawn radius comes from the JSON, not a component default.");
            Assert.AreEqual(WavePhaseTag.Ordinary, tags[0]);
            Assert.GreaterOrEqual(timeline.TotalDurationSeconds, 15f * 60f);
            CollectionAssert.AreEquivalent(
                new[] { WaveHookKind.MidBoss, WaveHookKind.FinalBoss },
                timeline.Hooks.Select(hook => hook.Kind));
        }

        [Test]
        public void FixtureTimeline_HardPhasesPressureMoreThanRespites()
        {
            var timeline = FixtureWaveTimelineCatalog.Create();
            var elite = timeline.Phases.Where(phase => phase.Tag == WavePhaseTag.Elite).ToList();
            var rest = timeline.Phases.Where(phase => phase.Tag == WavePhaseTag.Rest).ToList();

            Assert.IsNotEmpty(elite);
            Assert.IsNotEmpty(rest);
            Assert.Greater(
                elite.Min(phase => 1f / phase.SpawnIntervalSeconds),
                rest.Max(phase => 1f / phase.SpawnIntervalSeconds));
            Assert.Greater(elite.Min(phase => phase.MaxAliveEnemies), rest.Max(phase => phase.MaxAliveEnemies));
        }

        [Test]
        public void FixtureTimeline_LaterPhaseTradesStatsInsteadOfOnlyScalingUp()
        {
            var phases = FixtureWaveTimelineCatalog.Create().Phases;
            var trades = false;
            for (var earlier = 0; earlier < phases.Count && !trades; earlier++)
            {
                for (var later = earlier + 1; later < phases.Count; later++)
                {
                    var before = phases[earlier].Modifiers;
                    var after = phases[later].Modifiers;
                    if (after.SpeedMultiplier > before.SpeedMultiplier && after.HealthMultiplier < before.HealthMultiplier)
                    {
                        trades = true;
                        break;
                    }
                }
            }

            Assert.IsTrue(trades, "A later phase should be faster but frailer than an earlier one.");
        }
    }
}
