using System;
using System.Collections.Generic;
using Game.Character;
using Game.Enemy;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Progression.Tests
{
    public sealed class ExperienceAccountingTests
    {
        private GameObject _root;
        private RunController _run;
        private PlayerCharacterRuntime _player;
        private PlayerExperienceRuntime _xp;
        private readonly List<ExperienceAwardEvent> _events = new List<ExperienceAwardEvent>();

        [SetUp]
        public void SetUp()
        {
            _events.Clear();
            _root = new GameObject("xp-accounting");
            _run = _root.AddComponent<RunController>();
            if (!_run.IsInitialized) _run.Initialize();
            _run.Model.Start();
            var playerObject = new GameObject("player");
            playerObject.transform.SetParent(_root.transform);
            _player = playerObject.AddComponent<PlayerCharacterRuntime>();
            _player.Initialize(new CharacterBaseStats(100f, 3f, pickupRadius: 1f), _run);
            _xp = playerObject.AddComponent<PlayerExperienceRuntime>();
            _xp.Initialize(_player, _run, new ExperienceSettings(1f, 1000f));
            _xp.ExperienceResolved += _events.Add;
        }

        [TearDown]
        public void TearDown()
        {
            _xp.Shutdown();
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void RadiusChange_AppliesToExistingAndNewDrops_InWorldUnitsDespiteSpriteScale()
        {
            var source = Guid.NewGuid();
            var old = Drop(4f, new Vector2(1.5f, 0f), source);
            old.transform.localScale = Vector3.one * 0.35f;
            var identity = old.Identity;
            Assert.IsFalse(old.Tick(0f));
            _player.SetModifier("magnet", new CharacterStatModifier(pickupRadiusMultiplierBonus: 1f));
            Assert.IsTrue(old.Tick(0f));
            Assert.AreEqual(4f, _xp.CollectedBase);
            Assert.AreEqual(identity.LifeId, _events[0].Drop.Value.LifeId);
            Assert.AreEqual(source, _events[0].Drop.Value.SourceLifeId);
            Assert.IsFalse(old.TryPickup());
            var reused = Drop(2f, new Vector2(1.5f, 0f));
            Assert.AreSame(old, reused);
            Assert.AreNotEqual(identity.LifeId, reused.Identity.LifeId);
            Assert.IsNull(reused.Identity.SourceLifeId);
            Assert.IsTrue(reused.Tick(0f));
            var outside = Drop(3f, new Vector2(1.5f, 0f));
            _player.RemoveModifier("magnet");
            Assert.IsFalse(outside.Tick(0f));
            Assert.AreEqual(6f, _xp.CollectedBase);
            Assert.AreEqual(6f, _xp.TotalAwarded);
        }

        [Test]
        public void ExpiryRecoveryAndPickup_RecordBaseAndAwardedSeparately_ExactlyOnce()
        {
            var expired = Drop(4f, Vector2.one * 100f);
            Assert.IsTrue(expired.Tick(1f));
            Assert.AreEqual(ExperienceEventKind.Expired, _events[0].Kind);
            Assert.AreEqual(0f, _events[0].AwardedAmount);
            Assert.AreEqual(4f, _xp.ExpiredBase);
            _player.SetModifier("recovery", new CharacterStatModifier(disappearingXpRecoveryBonus: 0.5f, pickedUpXpMultiplierBonus: 1f));
            var recovered = Drop(8f, Vector2.one * 100f);
            var snapshot = recovered.Identity;
            Assert.IsTrue(recovered.Tick(1f));
            Assert.IsFalse(recovered.Tick(1f));
            Assert.IsFalse(recovered.TryPickup());
            Assert.AreEqual(4f, _events[1].AwardedAmount, "Recovery must not also apply the physical pickup multiplier.");
            Assert.AreEqual(12f, _xp.ExpiredBase);
            Assert.AreEqual(4f, _xp.RecoveredAwarded);
            Assert.IsTrue(Drop(2f, Vector2.zero).TryPickupWithinRadius());
            Assert.AreEqual(2f, _xp.CollectedBase);
            Assert.AreEqual(4f, _xp.CollectedAwarded);
            Assert.AreEqual(8f, _xp.TotalAwarded);
            Assert.AreEqual(3, _events.Count);
            Assert.AreEqual(snapshot.LifeId, _events[1].Drop.Value.LifeId);
        }

        [Test]
        public void MultipleLevels_PreserveThresholdsAndLifetimeAward_OutcomeDoesNotUseRemainderAsTotal()
        {
            _xp.Shutdown();
            _xp.Initialize(_player, _run, new ExperienceSettings(1f, 5f, 10f, 20f));
            var levels = new List<int>();
            _xp.LevelUp += levels.Add;
            _xp.AddPickedUpExperience(17f);
            CollectionAssert.AreEqual(new[] { 2, 3 }, levels);
            Assert.AreEqual(3, _xp.Progression.Level);
            Assert.AreEqual(2f, _xp.Progression.CurrentExperience);
            Assert.AreEqual(17f, _xp.TotalAwarded);
            var result = _run.Model.Stop().Contributions[_xp.Key];
            Assert.AreEqual(3, result.Level);
            Assert.AreEqual(2f, result.Experience);
            Assert.AreEqual(17f, result.ExperienceTotals.TotalAwarded);
            Assert.AreEqual(0f, _xp.AddPickedUpExperience(20f));
            Assert.AreEqual(17f, result.ExperienceTotals.TotalAwarded);
        }

        [Test]
        public void CompletionDuringLevelNotification_CapturesTheWholeAward()
        {
            _xp.Shutdown();
            _xp.Initialize(_player, _run, new ExperienceSettings(1f, 5f, 10f, 20f));
            _xp.LevelUp += _ => _run.Model.Stop();
            _xp.AddPickedUpExperience(17f);
            var result = _run.Model.Outcome.Contributions[_xp.Key];
            Assert.AreEqual(3, result.Level);
            Assert.AreEqual(2f, result.Experience);
            Assert.AreEqual(17f, result.ExperienceTotals.TotalAwarded);
        }

        [Test]
        public void PausedAndEndedDrops_CannotExpireOrBePickedUp_ShutdownRemovesActiveDropsAndContributor()
        {
            var drop = Drop(4f, Vector2.zero);
            _run.TogglePause();
            Assert.IsFalse(drop.Tick(5f));
            Assert.IsFalse(drop.TryPickup());
            _run.TogglePause();
            Assert.IsTrue(drop.Tick(0f));
            var active = Drop(6f, Vector2.one * 100f);
            var oldProgression = _xp.Progression;
            _xp.Shutdown();
            Assert.IsTrue(active == null);
            oldProgression.AddExperience(1000f);
            Assert.AreEqual(RunState.Running, _run.Model.State);
            _xp.Initialize(_player, _run, new ExperienceSettings(1f, 1000f));
            Assert.AreEqual(0f, _xp.TotalAwarded);
            var endedDrop = Drop(4f, Vector2.zero);
            _run.Model.Stop();
            Assert.IsFalse(endedDrop.Tick(10f));
            Assert.IsFalse(endedDrop.TryPickup());
        }

        [Test]
        public void DevelopmentAward_IsAnExplicitIntervention_NotPhysicalCollection()
        {
            _player.SetModifier("xp", new CharacterStatModifier(pickedUpXpMultiplierBonus: 0.5f));
            _xp.AddInterventionExperience(4f);
            Assert.AreEqual(ExperienceEventKind.DevelopmentIntervention, _events[0].Kind);
            Assert.IsNull(_events[0].Drop);
            Assert.AreEqual(0f, _xp.CollectedBase);
            Assert.AreEqual(4f, _xp.InterventionBase);
            Assert.AreEqual(6f, _xp.InterventionAwarded);
        }

        [Test]
        public void DeathSink_DoesNotDuplicateAReward_WhenTheSameLifeEventIsDeliveredTwice()
        {
            var sink = new EnemyExperienceDropSink(_xp, _run);
            var enemy = EnemyFactory.Spawn(new EnemyDefinition("FIXTURE-XP-ENEMY", 1f, 1f, 0f, 0f, 1f, 4f),
                Vector2.right * 10f, _player.transform, _run, _root.transform, lifecycleSink: sink);
            EnemyLifeEvent death = null;
            enemy.LifeEvent += value => { if (value.Kind == EnemyLifeEventKind.Died) death = value; };
            enemy.TakeDamage(1f);
            sink.OnEnemyLifeEvent(death);
            var drops = Object.FindObjectsByType<ExperienceDropRuntime>();
            Assert.AreEqual(1, drops.Length);
            Assert.AreEqual(death.LifeId, drops[0].Identity.SourceLifeId);
        }

        [Test]
        public void Shutdown_UnregistersOutcomeProducer()
        {
            _xp.Shutdown();
            Assert.IsFalse(_run.Model.Stop().Contributions.ContainsKey(_xp.Key));
        }

        [Test]
        public void InvalidAward_DoesNotPoisonCountersOrProgression()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _xp.AddPickedUpExperience(float.MaxValue));
            Assert.AreEqual(0f, _xp.TotalAwarded);
            Assert.AreEqual(1, _xp.Progression.Level);
        }

        private ExperienceDropRuntime Drop(float amount, Vector2 position, Guid? source = null) =>
            ExperienceDropFactory.Spawn(amount, position, 1f, _xp, _run, pool: _xp.DropPool,
                sourceLifeId: source, sourceContentId: source.HasValue ? new Game.Content.ContentId("FIXTURE-XP-SOURCE") : (Game.Content.ContentId?)null);
    }
}
