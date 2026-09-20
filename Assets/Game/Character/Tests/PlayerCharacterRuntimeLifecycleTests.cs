using System.Reflection;
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
