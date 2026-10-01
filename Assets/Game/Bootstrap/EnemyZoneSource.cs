using System.Collections.Generic;
using Game.Combat;
using Game.Content;
using Game.Enemy;
using Game.Zones;
using UnityEngine;

namespace Game.Bootstrap
{
    /// <summary>
    /// Zone-effect view of the living enemies. Each tick snapshots the registry into a reused list; damage over time is
    /// gathered per enemy and applied a few times per second so a zone does not fire a hit reaction every frame.
    /// </summary>
    public sealed class EnemyZoneSource : IZoneEnemySource
    {
        private const float DamageIntervalSeconds = 0.2f;

        private readonly List<EnemyRuntime> _alive = new List<EnemyRuntime>();
        private readonly Dictionary<EnemyRuntime, float> _pending = new Dictionary<EnemyRuntime, float>();
        private float _lastFlushTime;

        public int Refresh()
        {
            EnemyRegistry.CopyAliveTo(_alive);
            if (_pending.Count > 0 && Time.time - _lastFlushTime >= DamageIntervalSeconds) Flush();
            return _alive.Count;
        }

        public Vector2 Position(int index) => _alive[index].Position;

        public void Slow(int index, float fraction, float seconds, ContentId source)
        {
            var enemy = _alive[index];
            if (!enemy.IsAlive) return;
            enemy.ApplyControl(new CombatDamageRequest(new CombatSource(default, source, CombatSourceOrigin.Unknown), 0f,
                new CombatControlProfile(slowFraction: fraction, slowSeconds: seconds, channel: "zone")));
        }

        public void Damage(int index, float amount, ContentId source)
        {
            var enemy = _alive[index];
            _pending[enemy] = _pending.TryGetValue(enemy, out var sum) ? sum + amount : amount;
        }

        private void Flush()
        {
            _lastFlushTime = Time.time;
            foreach (var pair in _pending)
                if (pair.Key != null && pair.Key.IsAlive) pair.Key.TakeDamage(pair.Value);
            _pending.Clear();
        }
    }
}
