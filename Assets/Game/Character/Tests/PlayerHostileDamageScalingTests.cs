using System;
using System.Reflection;
using Game.Combat;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Character.Tests
{
    // DECISION-0107: the run-wide hostile coefficient scales hostile hits, not the character's protection stat.
    public sealed class PlayerHostileDamageScalingTests
    {
        private GameObject _runObject;
        private GameObject _playerObject;
        private RunController _runController;
        private PlayerCharacterRuntime _player;

        [SetUp]
        public void SetUp()
        {
            _runObject = new GameObject("RunController");
            _runController = _runObject.AddComponent<RunController>();
            typeof(RunController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_runController, null);
            _playerObject = new GameObject("Player");
            _player = _playerObject.AddComponent<PlayerCharacterRuntime>();
            _player.Initialize(new CharacterBaseStats(100f, 3f), _runController,
                permanentModifier: new CharacterStatModifier(incomingDamageReductionBonus: 0.03f), hostileDamageMultiplier: 0.7f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_playerObject);
            Object.DestroyImmediate(_runObject);
        }

        [Test]
        public void Initialize_WithHostileMultiplier_LeavesProtectionStatToOwnReductionOnly()
        {
            Assert.AreEqual(0.97f, _player.Stats.IncomingDamageMultiplier, 1e-5f);
        }

        [TestCase(CombatEntityCategory.OrdinaryEnemy)]
        [TestCase(CombatEntityCategory.Boss)]
        [TestCase(CombatEntityCategory.Traveler)]
        public void ApplyDamage_FromHostileSource_ScalesHitBeforeOwnReduction(CombatEntityCategory category)
        {
            var result = _player.ApplyDamage(new CombatDamageRequest(Source(category), 10f));

            Assert.AreEqual(7f, result.Health.Requested, 1e-4f);
            Assert.AreEqual(6.79f, result.Health.Actual, 1e-4f);
        }

        [TestCase(CombatEntityCategory.Unknown)]
        [TestCase(CombatEntityCategory.Player)]
        public void ApplyDamage_FromNonHostileSource_IgnoresHostileMultiplier(CombatEntityCategory category)
        {
            var result = _player.ApplyDamage(new CombatDamageRequest(Source(category), 10f));

            Assert.AreEqual(10f, result.Health.Requested, 1e-4f);
            Assert.AreEqual(9.7f, result.Health.Actual, 1e-4f);
        }

        private static CombatSource Source(CombatEntityCategory category) =>
            new CombatSource(new CombatIdentity(Guid.NewGuid(), null, null, category), null, CombatSourceOrigin.EnemyContact);
    }
}
