using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Pooling
{
    public sealed class GameObjectPool<T> where T : Component
    {
        private readonly Func<T> _factory;
        private readonly Transform _root;
        private readonly Stack<T> _inactive = new Stack<T>();
        // Mirrors _inactive so a second Return() of an already-pooled instance is a no-op
        // instead of queueing it twice (which would hand the same object to two renters).
        private readonly HashSet<T> _inactiveSet = new HashSet<T>();

        public GameObjectPool(Func<T> factory, Transform root = null)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _root = root;
        }

        public int InactiveCount => _inactive.Count;

        public T Rent()
        {
            while (_inactive.Count > 0)
            {
                var instance = _inactive.Pop();
                _inactiveSet.Remove(instance);
                if (instance == null)
                    continue;

                instance.gameObject.SetActive(true);
                return instance;
            }

            return _factory();
        }

        public void Return(T instance)
        {
            if (instance == null || !_inactiveSet.Add(instance))
                return;

            instance.transform.SetParent(_root, worldPositionStays: false);
            instance.gameObject.SetActive(false);
            _inactive.Push(instance);
        }
    }
}
