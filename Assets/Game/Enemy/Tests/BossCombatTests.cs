using System;
using System.Linq;
using Game.Content.Json;
using Game.Enemy.Json;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    public sealed class BossCombatTests
    {
        private static BossEncounterDefinition Final() => FixtureBossCatalog.Create(FixtureEnemyCatalog.Create())
            .Single(d => d.Hook == WaveHookKind.FinalBoss);

        [TestCase(.6001f, 0)]
        [TestCase(.6f, 1)]
        [TestCase(.3f, 2)]
        [TestCase(.01f, 2)]
        public void Phase_ThresholdCrossing_SelectsDeepestOnceAndHealingDoesNotReverse(float health, int expected)
        {
            var combat = new BossCombatController(Final());
            var changes = 0;
            combat.PhaseChanged += (_, _) => changes++;
            combat.Tick(0, true, health, Vector2.right);
            Assert.AreEqual(expected, combat.PhaseIndex);
            Assert.AreEqual(expected == 0 ? 0 : 1, changes);
            combat.Tick(0, true, 1, Vector2.right);
            combat.Tick(0, true, health, Vector2.right);
            Assert.AreEqual(expected, combat.PhaseIndex);
            Assert.AreEqual(expected == 0 ? 0 : 1, changes);
        }

        [Test]
        public void Phase_PauseAndLethalHealth_DoNotAdvanceOrEmitAttacks()
        {
            var combat = new BossCombatController(Final());
            combat.Tick(0, true, 1, Vector2.right);
            var remaining = combat.Attack.PhaseRemaining;
            Assert.IsEmpty(combat.Tick(100, false, .1f, Vector2.up));
            Assert.AreEqual(0, combat.PhaseIndex);
            Assert.AreEqual(remaining, combat.Attack.PhaseRemaining);
            Assert.AreEqual(Vector2.right, combat.Attack.AimDirection);
            Assert.IsEmpty(combat.Tick(100, true, 0, Vector2.up));
            Assert.AreEqual(0, combat.PhaseIndex);
        }

        [Test]
        public void Phase_ChangeDuringWindup_CancelsOldAttackAndTelegraphsNewOne()
        {
            var combat = new BossCombatController(Final());
            combat.Tick(0, true, 1, Vector2.right);
            Assert.IsEmpty(combat.Tick(100, true, .3f, Vector2.right));
            Assert.AreEqual(EnemyProjectilePattern.Ring, combat.AttackDefinition.Attack.Pattern);
            Assert.AreEqual(EnemyAttackPhase.Telegraphing, combat.Attack.Phase);
            Assert.AreEqual(combat.AttackDefinition.Attack.TelegraphSeconds, combat.Attack.PhaseRemaining);
            var shots = combat.Tick(combat.Attack.PhaseRemaining, true, .3f, Vector2.right);
            Assert.AreEqual(combat.AttackDefinition.Attack.ProjectileCount, shots.Length);
        }

        [Test]
        public void Sequence_CompletedCooldown_AlternatesThenWrapsWithFreshWindups()
        {
            var combat = new BossCombatController(Final());
            foreach (var expected in new[] { EnemyProjectilePattern.Fan, EnemyProjectilePattern.Ring, EnemyProjectilePattern.Fan })
            {
                combat.Tick(0, true, 1, Vector2.right);
                Assert.AreEqual(expected, combat.AttackDefinition.Attack.Pattern);
                var profile = combat.AttackDefinition.Attack;
                Assert.AreEqual(EnemyAttackPhase.Telegraphing, combat.Attack.Phase);
                Assert.AreEqual(profile.ProjectileCount, combat.Tick(profile.TelegraphSeconds, true, 1, Vector2.right).Length);
                Assert.IsEmpty(combat.Tick(profile.CooldownSeconds, true, 1, Vector2.right));
            }
        }

        [Test]
        public void Schema_InvalidThresholdOrderMissingFieldsOrUnknownReferences_AreRejected()
        {
            var enemies = FixtureEnemyCatalog.Create();
            BossEncounterData[] Data() => JsonContentFile.Load<BossEncounterData[]>("Content/Bosses/FixtureBosses");
            var data = Data(); data[0].Phases[1].HealthThreshold = 1;
            Assert.Throws<ArgumentException>(() => FixtureBossCatalog.FromData(data, enemies));
            data = Data(); data[0].SpawnOffsetX = null;
            Assert.Throws<ArgumentException>(() => FixtureBossCatalog.FromData(data, enemies));
            data = Data(); data[0].Phases[0].AttackEnemyIds[0] = "FIXTURE-NOT-FOUND";
            Assert.Throws<ArgumentException>(() => FixtureBossCatalog.FromData(data, enemies));
            data = Data(); data[0].Phases[1].HealthThreshold = float.NaN;
            Assert.Throws<ArgumentOutOfRangeException>(() => FixtureBossCatalog.FromData(data, enemies));
            data = Data(); data[0].Hook = "MidBoss";
            Assert.Throws<ArgumentException>(() => FixtureBossCatalog.FromData(data, enemies));
        }

        [Test]
        public void Sequence_BurstLongerThanCooldown_CompletesTailThenMovesToNextAttack()
        {
            var burst = new EnemyDefinition("FIXTURE-LONG-BURST", 10, 1, 0, 0, 1,
                attack: new EnemyAttackProfile(EnemyProjectilePattern.Burst, 1, .1f, 1, 1,
                    projectileCount: 3, burstIntervalSeconds: 1, telegraphSeconds: .2f));
            var final = Final();
            var combat = new BossCombatController(new BossEncounterDefinition(final.Id, "Long burst", final.Hook,
                final.Body, 8, 0, new[] { new BossPhaseDefinition("FIXTURE-SEQUENCE", 1, new[] { burst, final.Phases[0].Attacks[0] }) }));
            combat.Tick(0, true, 1, Vector2.right);
            Assert.AreEqual(1, combat.Tick(.2f, true, 1, Vector2.right).Length);
            Assert.AreEqual(1, combat.Tick(1, true, 1, Vector2.right).Length);
            Assert.AreEqual(burst.Id, combat.AttackDefinition.Id);
            Assert.AreEqual(1, combat.Tick(1, true, 1, Vector2.right).Length);
            Assert.IsEmpty(combat.Tick(0, true, 1, Vector2.right));
            Assert.AreEqual(EnemyProjectilePattern.Fan, combat.AttackDefinition.Attack.Pattern);
            Assert.AreEqual(EnemyAttackPhase.Telegraphing, combat.Attack.Phase);
        }

        [Test]
        public void Sequence_SpiralRepeatedCycle_PreservesPatternRotation()
        {
            var spiral = FixtureEnemyCatalog.Create().Single(e => e.Attack?.Pattern == EnemyProjectilePattern.Spiral);
            var final = Final();
            var combat = new BossCombatController(new BossEncounterDefinition(final.Id, "Spiral", final.Hook, final.Body,
                8, 0, new[] { new BossPhaseDefinition("FIXTURE-SPIRAL-PHASE", 1, new[] { spiral }) }));
            combat.Tick(0, true, 1, Vector2.right);
            var first = combat.Tick(spiral.Attack.TelegraphSeconds, true, 1, Vector2.right)[0].Direction;
            combat.Tick(spiral.Attack.CooldownSeconds, true, 1, Vector2.right);
            var second = combat.Tick(spiral.Attack.TelegraphSeconds, true, 1, Vector2.right)[0].Direction;
            Assert.AreNotEqual(first, second);
        }
    }
}
