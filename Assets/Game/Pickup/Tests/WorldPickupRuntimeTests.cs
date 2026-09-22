using System;
using System.Collections.Generic;
using Game.Character;
using Game.Content;
using Game.Content.Json;
using Game.Enemy;
using Game.Progression;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Game.Pickup.Tests
{
    public sealed class WorldPickupRuntimeTests
    {
        private GameObject _owner;
        private RunController _run;
        private PlayerCharacterRuntime _player;
        private PlayerExperienceRuntime _xp;
        private LevelUpDraftRuntime _draft;
        private WorldPickupRuntime _pickups;
        private FixturePickupCatalog _catalog;
        private int _baseline, _rewards;
        private readonly List<PickupEvent> _events = new List<PickupEvent>();
        [SetUp]
        public void SetUp()
        {
            _baseline = EnemyRegistry.Count; _rewards = 0; _events.Clear();
            _owner = new GameObject("pickup test"); _run = _owner.AddComponent<RunController>(); _run.Initialize();
            var player = new GameObject("target"); player.transform.SetParent(_owner.transform);
            player.AddComponent<BoxCollider2D>().size = Vector2.one;
            _player = player.AddComponent<PlayerCharacterRuntime>();
            _player.Initialize(new CharacterBaseStats(100, 3, healthRestorationMultiplier: 2, pickupRadius: 100), _run);
            _xp = player.AddComponent<PlayerExperienceRuntime>(); _xp.Initialize(_player, _run, new ExperienceSettings(60, 5));
            _draft = player.AddComponent<LevelUpDraftRuntime>();
            var starting = new BuildEntryDefinition("FIXTURE-START", BuildEntryKind.ActiveSkill, "Start");
            _draft.Initialize(_xp, _run, new[] { starting, new BuildEntryDefinition("FIXTURE-OTHER", BuildEntryKind.PassiveItem, "Other") },
                starting, 3, new SeededDraftRandom(1), initialBanishes: 2, emptyBookCurrency: 7);
            _catalog = FixturePickupCatalog.FromJson(JsonContentFile.ReadText("Content/Pickups/FixturePickups").Replace("\"baseChance\": 0.08", "\"baseChance\": 1"));
            _pickups = _owner.AddComponent<WorldPickupRuntime>(); InitializePickups();
            _pickups.Resolved += _events.Add;
            _run.Model.Start(); Physics2D.SyncTransforms();
        }
        private void InitializePickups() => _pickups.Initialize(_catalog, _run.Model, _player,
            new PlayerPickupRewardTarget(_player, _run.Model, _draft, _ => _rewards++),
            new BoxPickupPlacement(new Rect(-10, -10, 20, 20), Array.Empty<Rect>(), Vector2.one * .5f, Vector2.zero, .01f), "FIXTURE-FIELD");
        [TearDown]
        public void TearDown()
        {
            _pickups.Shutdown(); _draft.Shutdown(); _xp.Shutdown(); _player.Shutdown(); _run.Shutdown();
            Object.DestroyImmediate(_owner); Assert.AreEqual(_baseline, EnemyRegistry.Count);
        }
        [Test]
        public void Potion_RestorationClampFullHealthAndDuplicate_HasOneRewardPerLife()
        {
            _player.Health.TakeDamage(25);
            var drop = _pickups.Spawn(_catalog.Potion, Vector2.zero); var id = drop.Life.Identity.DropId;
            Assert.IsTrue(_pickups.TryCollect(drop, id)); Assert.AreEqual(100, _player.Health.CurrentHealth);
            Assert.AreEqual(20, _events[0].Healing.Requested); Assert.AreEqual(40, _events[0].Healing.AfterMitigation); Assert.AreEqual(25, _events[0].Healing.Actual);
            Assert.IsFalse(_pickups.TryCollect(drop, id)); Assert.AreEqual(1, _rewards);
            var reused = _pickups.Spawn(_catalog.Potion, Vector2.zero);
            Assert.AreSame(drop, reused); Assert.AreNotEqual(id, reused.Life.Identity.DropId);
            Assert.IsFalse(_pickups.TryCollect(reused, id));
            _pickups.Tick(0); Assert.AreEqual(2, _rewards); Assert.AreEqual(0, _events[1].Healing.Actual);
        }
        [Test]
        public void Contact_HugeXpRadiusDoesNotCollectDistantPickup()
        {
            var drop = _pickups.Spawn(_catalog.Potion, Vector2.right * 2); _pickups.Tick(0);
            Assert.AreEqual(1, _pickups.Snapshot.Active); Assert.AreEqual(0, _rewards);
            _player.transform.position = Vector2.right * 1.4f; Physics2D.SyncTransforms(); _pickups.Tick(0);
            Assert.AreEqual(0, _pickups.Snapshot.Active); Assert.AreEqual(1, _rewards);
        }
        [Test]
        public void BookPause_DefersFollowingPotionUntilChoice_WithoutXpOrLevel()
        {
            _pickups.Spawn(_catalog.Book, Vector2.zero); _pickups.Spawn(_catalog.Potion, Vector2.zero);
            _pickups.Tick(0); Assert.IsTrue(_draft.IsDraftOpen); Assert.AreEqual(RunState.Paused, _run.Model.State);
            Assert.AreEqual(1, _pickups.Snapshot.Active); Assert.AreEqual(0, _rewards); Assert.AreEqual(1, _xp.Progression.Level);
            _pickups.Tick(10); Assert.AreEqual(0, _rewards);
            _draft.Select(_draft.CurrentDraft.Options[0].Definition.Id); _pickups.Tick(0);
            Assert.AreEqual(1, _rewards); Assert.AreEqual(2, _pickups.Snapshot.Collected);
        }
        [Test]
        public void Book_EmptyAtPickup_AwardsCurrencyOnceWithoutXpOrPause()
        {
            Assert.IsTrue(_draft.Controls.TryBanish("FIXTURE-START")); Assert.IsTrue(_draft.Controls.TryBanish("FIXTURE-OTHER"));
            var drop = _pickups.Spawn(_catalog.Book, Vector2.zero); var id = drop.Life.Identity.DropId;
            Assert.IsTrue(_pickups.TryCollect(drop, id)); Assert.IsFalse(_pickups.TryCollect(drop, id));
            Assert.AreEqual(7, _draft.BookCurrency); Assert.AreEqual(RunState.Running, _run.Model.State);
            Assert.AreEqual(1, _xp.Progression.Level); Assert.AreEqual(0, _xp.Progression.CurrentExperience);
        }
        [Test]
        public void Book_RejectedByDraft_RemainsOnGround()
        {
            var drop = _pickups.Spawn(_catalog.Book, Vector2.zero); var id = drop.Life.Identity.DropId;
            _draft.Shutdown(); Assert.IsFalse(_pickups.TryCollect(drop, id));
            Assert.AreEqual(PickupLifeState.Available, drop.Life.State); Assert.AreEqual(id, drop.Life.Identity.DropId);
            Assert.AreEqual(1, _pickups.Snapshot.Active); Assert.AreEqual(0, _pickups.Snapshot.Collected);
        }
        [Test]
        public void Collect_TerminalCallback_CancelsFollowingPickupWithoutFurtherReward()
        {
            _player.CombatResolved += result => { if (result.Health.IsHealing) _run.Model.Kill(); };
            _pickups.Spawn(_catalog.Potion, Vector2.zero); _pickups.Spawn(_catalog.Potion, Vector2.zero);
            _pickups.Tick(0);
            Assert.AreEqual(1, _pickups.Snapshot.Collected); Assert.AreEqual(1, _pickups.Snapshot.Cancelled);
            Assert.AreEqual(0, _pickups.Snapshot.Active); Assert.AreEqual(0, _rewards);
        }
        [Test]
        public void Collect_ShutdownInRewardCallback_DoesNotLeaveClaimedVisualOrStaleCounts()
        {
            _player.CombatResolved += result => { if (result.Health.IsHealing) _pickups.Shutdown(); };
            _pickups.Spawn(_catalog.Potion, Vector2.zero); _pickups.Tick(0);
            Assert.IsFalse(_pickups.IsInitialized); Assert.AreEqual(0, _pickups.Snapshot.Active);
            Assert.AreEqual(0, _pickups.Snapshot.Collected); Assert.AreEqual(1, _pickups.InactiveCount);
        }
        [Test]
        public void Lifetime_PauseFreezeAndExpiryBeforeContact_NoReward()
        {
            var timed = new PickupDefinition("TIMED", PickupRewardKind.Potion, 20, .2f, 1, "+", Color.green, .1f);
            _pickups.Spawn(timed, Vector2.zero); _run.Model.Pause(); _pickups.Tick(100);
            Assert.AreEqual(1, _pickups.Snapshot.Active); _run.Model.Resume(); _pickups.Tick(1);
            Assert.AreEqual(1, _pickups.Snapshot.Expired); Assert.AreEqual(0, _rewards);
        }
        [Test]
        public void Terminal_CancelsDrops_AndReinitResetsCountsAndVisualLife()
        {
            var drop = _pickups.Spawn(_catalog.Potion, Vector2.right * 2); var old = drop.Life;
            _run.Model.Kill(); Assert.AreEqual(0, _pickups.Snapshot.Active); Assert.AreEqual(PickupLifeState.Cancelled, old.State);
            Assert.IsNull(_pickups.Spawn(_catalog.Book, Vector2.zero));
            _run.Shutdown(); _run.Initialize(); InitializePickups(); _run.Model.Start();
            var reused = _pickups.Spawn(_catalog.Book, Vector2.right * 2);
            Assert.AreSame(drop, reused); Assert.AreEqual(1, _pickups.Snapshot.Spawned); Assert.AreEqual(0, _pickups.Snapshot.Cancelled);
            Assert.AreEqual(Vector3.one, reused.transform.localScale); Assert.AreEqual("BOOK", reused.GetComponentInChildren<TextMesh>().text);
            Assert.AreEqual(_run.Model.RunId, reused.Life.Identity.RunId);
        }
        [TestCase(EnemyCategory.Ordinary, true, 1)]
        [TestCase(EnemyCategory.Ordinary, false, 0)]
        [TestCase(EnemyCategory.Boss, true, 0)]
        [TestCase(EnemyCategory.Traveler, true, 0)]
        public void DeathDrop_OnlyOrdinaryDeath_NotDespawnOrOtherCategory(EnemyCategory category, bool killed, int expected)
        {
            var enemy = EnemyFactory.Spawn(FixtureEnemyCatalog.Create()[0], Vector2.right * 3, _player.transform, _run,
                _owner.transform, lifecycleSink: _pickups, category: category);
            enemy.LifeEvent += snapshot => { if (snapshot.Kind == EnemyLifeEventKind.Died) _pickups.OnEnemyLifeEvent(snapshot); };
            if (killed) enemy.TakeDamage(100000); else enemy.Despawn();
            Assert.AreEqual(expected, _pickups.Snapshot.Spawned);
        }
    }
}
