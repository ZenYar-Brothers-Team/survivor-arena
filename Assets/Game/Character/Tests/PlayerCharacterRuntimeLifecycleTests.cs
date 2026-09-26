using System.Collections.Generic;
using System.Reflection;
using Game.Combat;
using Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace Game.Character.Tests
{
    public class PlayerCharacterRuntimeLifecycleTests
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
            Invoke(_runController, "Awake");
            _playerObject = new GameObject("Player");
            _player = _playerObject.AddComponent<PlayerCharacterRuntime>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
                Object.DestroyImmediate(_playerObject);
            if (_runObject != null)
                Object.DestroyImmediate(_runObject);
        }

        [Test]
        public void Update_AfterShutdown_DoesNotThrow()
        {
            _player.Initialize(FixtureCharacterCatalog.CreateDefault(), _runController);
            _player.Shutdown();

            Assert.IsNull(_player.Health);
            Assert.DoesNotThrow(() => Invoke(_player, "Update"));
        }

        [Test]
        public void Update_BeforeInitialize_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => Invoke(_player, "Update"));
        }

        [Test]
        public void Initialize_AfterShutdown_RebuildsHealthAndBinding()
        {
            var stats = FixtureCharacterCatalog.CreateDefault();
            _player.Initialize(stats, _runController);
            var firstHealth = _player.Health;
            _player.Shutdown();

            _player.Initialize(stats, _runController);

            Assert.IsNotNull(_player.Health);
            Assert.AreNotSame(firstHealth, _player.Health);
            Assert.DoesNotThrow(() => Invoke(_player, "Update"));
        }

        [Test]
        public void Update_WhileRunning_ReportsRegenerationWithItsOwnSource()
        {
            // Playtest 794c2696: regeneration healing was reported without attribution ("unknown").
            _player.Initialize(new CharacterBaseStats(100f, 3f, healthRegenerationPerSecond: 1f), _runController);
            _runController.Model.Start();
            var results = new List<CombatResult>();
            _player.CombatResolved += results.Add;

            Invoke(_player, "Update");

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(CombatSourceOrigin.Regeneration, results[0].Source.Origin);
            Assert.IsNull(results[0].Source.ContentId);
            Assert.AreEqual(_player.Identity, results[0].Source.Owner);
            Assert.IsTrue(results[0].Health.IsHealing);
        }

        private static void Invoke(MonoBehaviour behaviour, string method)
        {
            var target = behaviour.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic);
            try
            {
                target.Invoke(behaviour, null);
            }
            catch (TargetInvocationException exception)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            }
        }
    }
}
