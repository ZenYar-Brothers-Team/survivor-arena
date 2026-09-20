using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Game.Pooling.Tests
{
    public class GameObjectPoolTests
    {
        private readonly List<GameObject> _created = new List<GameObject>();
        private GameObject _rootObject;
        private GameObjectPool<SpriteRenderer> _pool;
        private int _factoryCalls;

        [SetUp]
        public void SetUp()
        {
            _factoryCalls = 0;
            _rootObject = new GameObject("Pool Root");
            _pool = new GameObjectPool<SpriteRenderer>(CreateInstance, _rootObject.transform);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var created in _created)
            {
                if (created != null)
                    UnityEngine.Object.DestroyImmediate(created);
            }
            _created.Clear();
            if (_rootObject != null)
                UnityEngine.Object.DestroyImmediate(_rootObject);
        }

        private SpriteRenderer CreateInstance()
        {
            _factoryCalls++;
            var instance = new GameObject("Pooled");
            _created.Add(instance);
            return instance.AddComponent<SpriteRenderer>();
        }

        [Test]
        public void Constructor_RequiresFactory()
        {
            Assert.Throws<ArgumentNullException>(() => new GameObjectPool<SpriteRenderer>(null));
        }

        [Test]
        public void Rent_WhenEmpty_CreatesThroughFactory()
        {
            var rented = _pool.Rent();

            Assert.IsNotNull(rented);
            Assert.AreEqual(1, _factoryCalls);
            Assert.IsTrue(rented.gameObject.activeSelf);
        }

        [Test]
        public void Return_DeactivatesAndReparentsUnderPoolRoot()
        {
            var rented = _pool.Rent();
            rented.transform.SetParent(null);

            _pool.Return(rented);

            Assert.IsFalse(rented.gameObject.activeSelf);
            Assert.AreSame(_rootObject.transform, rented.transform.parent);
            Assert.AreEqual(1, _pool.InactiveCount);
        }

        [Test]
        public void Rent_AfterReturn_ReusesTheSameInstanceAndReactivatesIt()
        {
            var first = _pool.Rent();
            _pool.Return(first);

            var second = _pool.Rent();

            Assert.AreSame(first, second);
            Assert.AreEqual(1, _factoryCalls, "The factory must not run when a pooled instance is available.");
            Assert.IsTrue(second.gameObject.activeSelf);
            Assert.AreEqual(0, _pool.InactiveCount);
        }

        [Test]
        public void Return_SameInstanceTwice_IsIgnoredSoItIsNeverRentedTwice()
        {
            var instance = _pool.Rent();

            _pool.Return(instance);
            _pool.Return(instance);

            Assert.AreEqual(1, _pool.InactiveCount);
            var first = _pool.Rent();
            var second = _pool.Rent();
            Assert.AreSame(instance, first);
            Assert.AreNotSame(first, second, "A double Return must not hand the same object to two renters.");
        }

        [Test]
        public void Return_Null_IsIgnored()
        {
            Assert.DoesNotThrow(() => _pool.Return(null));
            Assert.AreEqual(0, _pool.InactiveCount);
        }

        [Test]
        public void Rent_SkipsInstancesDestroyedWhilePooled()
        {
            var doomed = _pool.Rent();
            var survivor = _pool.Rent();
            _pool.Return(survivor);
            _pool.Return(doomed);
            UnityEngine.Object.DestroyImmediate(doomed.gameObject);

            var rented = _pool.Rent();

            Assert.AreSame(survivor, rented);
            Assert.AreEqual(2, _factoryCalls, "The destroyed instance must be skipped, not recreated.");
        }

        [Test]
        public void Return_WithoutRoot_UnparentsAndDeactivates()
        {
            var pool = new GameObjectPool<SpriteRenderer>(CreateInstance);
            var instance = pool.Rent();
            instance.transform.SetParent(_rootObject.transform);

            pool.Return(instance);

            Assert.IsNull(instance.transform.parent);
            Assert.IsFalse(instance.gameObject.activeSelf);
        }
    }
}
