using System;
using System.Collections.Generic;
using Game.Diagnostics;
using Game.Pooling;
using UnityEngine;

namespace Game.Presentation
{
    // Dedicated diagnostic kit, deliberately not a production enemy/pickup catalog.
    public sealed class PresentationFixtureKit : IDisposable
    {
        private readonly GameObjectPool<PresentationFixtureActor> _pool;
        private readonly GameObject _root;
        private readonly List<PresentationFixtureActor> _actors = new List<PresentationFixtureActor>();
        private readonly IReadOnlyList<PresentationFixtureDefinition> _definitions;
        private readonly PresentationFeedbackProfile _feedback;
        private readonly SpriteMotionProfile _motion;
        public GameObject Root => _root;
        public IReadOnlyList<PresentationFixtureActor> Actors => _actors;
        public bool IsRunning { get; private set; } = true;
        public PresentationFixtureKit()
        {
            _definitions = FixturePresentationKitCatalog.Create();
            _feedback = FixturePresentationFeedbackCatalog.Create();
            _motion = FixtureSpriteMotionProfileCatalog.Create()[0];
            _root = new GameObject("Presentation fixture kit");
            _pool = new GameObjectPool<PresentationFixtureActor>(() =>
            {
                var go = new GameObject("Fixture actor");
                go.transform.SetParent(_root.transform, false);
                return go.AddComponent<PresentationFixtureActor>();
            }, _root.transform);
            Reset(1);
        }
        public void Reset(int copies)
        {
            if (copies < 1 || copies > 4) throw new ArgumentOutOfRangeException(nameof(copies));
            using (PerfGuard.Measure("PresentationFixtureKit.Reset", 10))
            {
                foreach (var actor in _actors) { actor.Shutdown(); _pool.Return(actor); }
                _actors.Clear();
                for (var copy = 0; copy < copies; copy++)
                foreach (var definition in _definitions)
                {
                    var actor = _pool.Rent();
                    actor.name = definition.Id.ToString();
                    actor.transform.localPosition = new Vector3(definition.Position.x, definition.Position.y - copy, 0);
                    actor.Initialize(definition, _feedback, _motion);
                    actor.IsRunning = IsRunning;
                    _actors.Add(actor);
                }
            }
        }
        public void SetRunning(bool running)
        {
            IsRunning = running;
            foreach (var actor in _actors) actor.IsRunning = running;
        }
        public void SetVelocity(Vector2 velocity)
        {
            foreach (var actor in _actors) actor.Velocity = velocity;
        }
        public void Signal(PresentationSignal signal)
        {
            foreach (var actor in _actors) actor.Signal(signal);
        }
        public void Tick(float deltaTime)
        {
            using (PerfGuard.Measure("PresentationFixtureKit.Tick", 5))
                foreach (var actor in _actors) actor.Tick(deltaTime);
        }
        public void Dispose()
        {
            foreach (var actor in _actors) actor.Shutdown();
            _actors.Clear();
            if (_root == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(_root);
            else UnityEngine.Object.DestroyImmediate(_root);
        }
    }
}
