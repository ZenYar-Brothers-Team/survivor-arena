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

        public GameObjectPool(Func<T> factory, Transform root = null)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _root = root;
        }

        public T Rent()
        {
            while (_inactive.Count > 0)
            {
                var instance = _inactive.Pop();
                if (instance == null)
                    continue;

                instance.gameObject.SetActive(true);
                return instance;
            }

            return _factory();
        }

        public void Return(T instance)
        {
            if (instance == null)
                return;

            instance.transform.SetParent(_root, worldPositionStays: false);
            instance.gameObject.SetActive(false);
            _inactive.Push(instance);
        }
    }
}
