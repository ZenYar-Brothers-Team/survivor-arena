using System;
using Game.Combat;
using Game.Content;
using Game.Enemy;
using Game.Pooling;
using Game.Progression;
using Game.Run;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using Game.Character;
using Game.ActiveSkill;

namespace Game.Telemetry.Tests
{
    public sealed class PlaytestSessionTests
    {
        [Test]
        public void LethalPlayerHit_CompletesBeforeResult_ExportStillIncludesAppliedDamage()
        {
            using var fixture = new TelemetryRuntimeFixture();
            fixture.Run.Model.Tick(2);
            fixture.Player.TakeDamage(100);
            Assert.AreEqual(RunState.Lost, fixture.Run.Model.State);
            fixture.Session.Tick(); fixture.Session.PendingExport.GetAwaiter().GetResult(); fixture.Session.Tick();
            var data = JObject.Parse(fixture.Sink.Report.Json);
            Assert.AreEqual(10, (double)data["appliedDamageTaken"]);
            Assert.AreEqual(90, (double)data["combat"][0]["overkill"]);
            Assert.AreEqual("Defeat", (string)data["outcome"]["reason"]);
            Assert.IsNotNull(fixture.Run.Model.Outcome.Contributions["draft"].Sets);
        }
        [Test]
        public void PooledEnemyLethalResult_SurvivesSynchronousReturn_AndOldLifeIsNotReused()
        {
            var baseline = EnemyRegistry.Count;
            using (var fixture = new TelemetryRuntimeFixture())
            {
                var pool = new GameObjectPool<EnemyRuntime>(EnemyFactory.CreateInstance, fixture.Root.transform);
                var definition = new EnemyDefinition(new ContentId("FIXTURE-TEST-ENEMY"), 10, 1, 0, 0, 1);
                var enemy = EnemyFactory.Spawn(definition, Vector2.zero, fixture.Root.transform, fixture.Run, fixture.Root.transform, pool: pool);
                var life = enemy.LifeId;
                enemy.CombatResolved += fixture.Session.Recorder.Combat;
                enemy.ApplyDamage(new EnemyDamageRequest(new CombatDamageRequest(default, 100)));
                Assert.IsFalse(enemy.gameObject.activeSelf);
                var reused = EnemyFactory.Spawn(definition, Vector2.zero, fixture.Root.transform, fixture.Run, fixture.Root.transform, pool: pool);
                Assert.AreSame(enemy, reused); Assert.AreNotEqual(life, reused.LifeId);
                reused.CombatResolved += fixture.Session.Recorder.Combat;
                reused.ApplyDamage(new EnemyDamageRequest(new CombatDamageRequest(default, 2)));
                var data = JObject.Parse(fixture.Session.Recorder.Snapshot(null, false).Json);
                Assert.AreEqual(12, (double)data["appliedDamageDealt"]);
                Assert.AreEqual(2, (long)data["combat"][0]["results"]);
            }
            Assert.AreEqual(baseline, EnemyRegistry.Count);
        }
        [Test]
        public void XpDrop_CollectExpireRecoverAndGround_AreSeparateAndRepeatedPickupHasNoEffect()
        {
            using var fixture = new TelemetryRuntimeFixture();
            var first = ExperienceDropFactory.Spawn(4, Vector2.zero, 5, fixture.Xp, fixture.Run, fixture.Root.transform, fixture.Xp.DropPool);
            Assert.IsTrue(first.TryPickup()); Assert.IsFalse(first.TryPickup());
            var expired = ExperienceDropFactory.Spawn(8, new Vector2(20, 20), 5, fixture.Xp, fixture.Run, fixture.Root.transform, fixture.Xp.DropPool);
            Assert.IsTrue(expired.Tick(6));
            ExperienceDropFactory.Spawn(3, new Vector2(20, 20), 5, fixture.Xp, fixture.Run, fixture.Root.transform, fixture.Xp.DropPool);
            fixture.Run.Model.Stop(); fixture.Session.Tick(); fixture.Session.PendingExport.GetAwaiter().GetResult();
            var data = JObject.Parse(fixture.Sink.Report.Json);
            Assert.AreEqual(15, (double)data["producers"]["xp"]["droppedBase"]);
            Assert.AreEqual(3, (double)data["producers"]["xp"]["groundBase"]);
            Assert.AreEqual(4, (double)data["counters"]["xp.Collected.base"]);
            Assert.AreEqual(8, (double)data["counters"]["xp.Collected.awarded"]);
            Assert.AreEqual(8, (double)data["counters"]["xp.Expired.base"]);
            Assert.AreEqual(2, (double)data["counters"]["xp.Expired.awarded"]);
            Assert.AreEqual(10, fixture.Xp.TotalAwarded);
        }
        [Test]
        public void HealthRescale_IsNotHealing_ActualHealIsCapped()
        {
            using var fixture = new TelemetryRuntimeFixture();
            fixture.Player.TakeDamage(4); fixture.Player.Heal(100);
            fixture.Player.SetModifier("rescale", new CharacterStatModifier(maxHealthMultiplierBonus: 1));
            var data = JObject.Parse(fixture.Session.Recorder.Snapshot(null, false).Json);
            Assert.AreEqual(4, (double)data["actualHealing"]);
            Assert.AreEqual(4, (double)data["appliedDamageTaken"]);
            Assert.AreEqual(20, fixture.Player.Health.CurrentHealth);
        }
        [Test]
        public void Draft_OffersControlsResolutionAndOutcome_AreRecordedWithoutChangingRng()
        {
            using var fixture = new TelemetryRuntimeFixture();
            fixture.Xp.AddInterventionExperience(500);
            Assert.IsTrue(fixture.Draft.IsDraftOpen);
            var old = fixture.Draft.Revision;
            Assert.IsTrue(fixture.Draft.Reroll(old)); Assert.IsFalse(fixture.Draft.Reroll(old));
            Assert.IsTrue(fixture.Draft.Banish(fixture.Draft.CurrentDraft.Options[0].Definition.Id));
            var choice = fixture.Draft.CurrentDraft.Options[0].Definition.Id;
            Assert.IsTrue(fixture.Draft.Select(choice));
            fixture.Run.Model.Stop(); fixture.Session.Tick(); fixture.Session.PendingExport.GetAwaiter().GetResult();
            var data = JObject.Parse(fixture.Sink.Report.Json);
            Assert.AreEqual(1, (int)data["counters"]["draft.reroll.success"]);
            Assert.AreEqual(1, (int)data["counters"]["draft.reroll.failed"]);
            Assert.AreEqual(1, (int)data["counters"]["draft.banish.success"]);
            Assert.AreEqual(1, (int)data["counters"]["draft.Selected"]);
            StringAssert.Contains(choice.ToString(), fixture.Sink.Report.Json);
            Assert.AreEqual(2, fixture.Run.Model.Outcome.Contributions["experience"].Level);
        }
        [Test]
        public void DisabledRecorder_StillCapturesRequiredResults_AndDisposedSessionStopsObserving()
        {
            using (var fixture = new TelemetryRuntimeFixture(false))
            {
                var disabled = new DisabledPlaytestSession(); disabled.Export(); disabled.AddMarker("ignored");
                fixture.Run.Model.Tick(3); fixture.Run.Model.Stop();
                var outcome = fixture.Run.Model.Outcome;
                Assert.AreEqual(3, outcome.ElapsedSeconds); Assert.AreEqual(1, outcome.Contributions["experience"].Level);
                Assert.AreEqual(1, outcome.Contributions["draft"].Build.Count); Assert.AreEqual(0, outcome.Contributions["draft"].Sets.Count);
            }
            using (var fixture = new TelemetryRuntimeFixture())
            {
                fixture.Session.Dispose(); fixture.Player.TakeDamage(1);
                Assert.AreEqual(0, (double)JObject.Parse(fixture.Session.Recorder.Snapshot(null, false).Json)["appliedDamageTaken"]);
            }
        }
        [Test]
        public void ExportFailure_IsVisibleAndRetryable_AndDoesNotEndRun()
        {
            using var fixture = new TelemetryRuntimeFixture();
            fixture.Sink.Fail = true; fixture.Session.Export(); fixture.Session.Tick();
            Assert.Throws<InvalidOperationException>(() => fixture.Session.PendingExport.GetAwaiter().GetResult());
            fixture.Session.Tick(); StringAssert.Contains("Export error: Synthetic disk failure", fixture.Session.Summary);
            Assert.AreEqual(RunState.Running, fixture.Run.Model.State);
            fixture.Sink.Fail = false; fixture.Session.Export(); fixture.Session.Tick();
            fixture.Session.PendingExport.GetAwaiter().GetResult(); fixture.Session.Tick();
            StringAssert.Contains("Saved: fake/output", fixture.Session.Summary);
            Assert.AreEqual("incomplete", (string)JObject.Parse(fixture.Sink.Report.Json)["completionReason"]);
        }

        [Test]
        public void DelayedProjectile_AfterSourceDespawnAndLevelUp_RetainsOriginalAttribution()
        {
            var baseline = EnemyRegistry.Count;
            using (var fixture = new TelemetryRuntimeFixture())
            {
                var definition = new EnemyDefinition("FIXTURE-SOURCE", 10, 1, 0, 0, 1);
                var source = EnemyFactory.Spawn(definition, Vector2.zero, fixture.Root.transform, fixture.Run, fixture.Root.transform);
                var skill = new ActiveSkillInstance(Game.ActiveSkill.FixtureActiveSkillCatalog.Create()[0]);
                var identity = source.Identity;
                var shot = new ActiveSkillProjectile(Vector2.zero, Vector2.right, 1, 10, .1f, 0,
                    new EnemyDamageRequest(new CombatDamageRequest(new CombatSource(identity, skill.Definition.Id, CombatSourceOrigin.ActiveSkill, skill.Level), 3)));
                var projectile = FixtureProjectileFactory.Spawn(shot, fixture.Run, fixture.Root.transform);
                skill.SetLevel(2); source.Despawn();
                var target = EnemyFactory.Spawn(definition, Vector2.right, fixture.Root.transform, fixture.Run, fixture.Root.transform);
                CombatResult observed = default;
                target.CombatResolved += result => { observed = result; fixture.Session.Recorder.Combat(result); };
                Assert.IsTrue(projectile.TryImpact(target, target.Position));
                Assert.AreEqual(identity.LifeId, observed.Source.Owner.LifeId);
                Assert.AreEqual(1, observed.Source.SkillLevel);
                var data = JObject.Parse(fixture.Session.Recorder.Snapshot(null, false).Json);
                Assert.AreEqual(1, (int)data["combat"][0]["level"]); Assert.AreEqual(3, (double)data["appliedDamageDealt"]);
            }
            Assert.AreEqual(baseline, EnemyRegistry.Count);
        }

        [Test]
        public void Shutdown_DuringLiveExport_PublishesFinalSnapshotAfterEarlierPacket()
        {
            using var fixture = new TelemetryRuntimeFixture();
            fixture.Session.Export(); fixture.Session.Tick();
            fixture.Run.Model.Stop(); fixture.Session.Dispose();
            fixture.Session.PendingExport.GetAwaiter().GetResult();
            Assert.AreEqual("aborted", (string)JObject.Parse(fixture.Sink.Report.Json)["completionReason"]);
        }
    }
}
