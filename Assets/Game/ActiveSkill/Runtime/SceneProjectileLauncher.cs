using System;
using System.Collections.Generic;
using Game.Pooling;
using Game.Run;
using UnityEngine;

namespace Game.ActiveSkill
{
    public sealed class SceneProjectileLauncher : IActiveSkillProjectileLauncher, IDisposable
    {
        private readonly RunController _runController;
        private readonly Game.Enemy.ICombatTargetQuery _retargetQuery;
        private readonly Transform _root;
        private readonly GameObjectPool<FixtureProjectileRuntime> _pool;
        private readonly HashSet<FixtureProjectileRuntime> _active = new HashSet<FixtureProjectileRuntime>();
        private bool _disposed;
        public int ActiveCount => _active.Count;
        public int InactiveCount => _pool.InactiveCount;

        /// <param name="retargetQuery">Ricochet target selection; the executor passes its on-screen query (DECISION-0058).</param>
        public SceneProjectileLauncher(RunController runController, Game.Enemy.ICombatTargetQuery retargetQuery = null)
        {
            _runController = runController != null ? runController : throw new ArgumentNullException(nameof(runController));
            _retargetQuery = retargetQuery;
            _root = new GameObject("Active Skill Projectile Pool").transform;
            _pool = new GameObjectPool<FixtureProjectileRuntime>(FixtureProjectileFactory.Create, _root);
        }
        public void Launch(ActiveSkillProjectile projectile)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SceneProjectileLauncher));
            var runtime = FixtureProjectileFactory.Spawn(projectile, _runController, _root, _pool, _retargetQuery);
            _active.Add(runtime);
            runtime.Returned += OnReturned;
        }
        private void OnReturned(FixtureProjectileRuntime runtime)
        {
            runtime.Returned -= OnReturned;
            _active.Remove(runtime);
        }
        public void Clear()
        {
            using var guard = Game.Diagnostics.PerfGuard.Measure("SceneProjectileLauncher.Clear", 2f);
            while (_active.Count > 0)
            {
                using var items = _active.GetEnumerator();
                items.MoveNext();
                var runtime = items.Current;
                if (runtime != null) runtime.Despawn();
                else _active.Remove(runtime);
            }
        }
        public void Dispose()
        {
            if (_disposed) return;
            Clear();
            _disposed = true;
            if (_root == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(_root.gameObject);
            else UnityEngine.Object.DestroyImmediate(_root.gameObject);
        }
    }
}
