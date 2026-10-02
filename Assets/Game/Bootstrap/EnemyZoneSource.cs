using System.Collections.Generic;
using Game.Combat;
using Game.Content;
using Game.Enemy;
using Game.Zones;
using Game.Presentation;
using UnityEngine;

namespace Game.Bootstrap
{
    /// <summary>
    /// Zone-effect view of the living enemies. Each tick snapshots the registry into a reused list; damage over time is
    /// gathered per enemy and applied a few times per second so a zone does not fire a hit reaction every frame.
    /// </summary>
    public sealed class EnemyZoneSource : IZoneEnemySource, System.IDisposable
    {
        private const float DamageIntervalSeconds = 0.2f;

        private readonly List<EnemyRuntime> _alive = new List<EnemyRuntime>();
        private readonly Dictionary<EnemyRuntime, float> _pending = new Dictionary<EnemyRuntime, float>();
        private float _lastFlushTime;
        private readonly System.Action<SpritePresentationRuntime> _damageFeedback;
        private readonly bool _suppressAreaUnitFeedback;
        private readonly HashSet<EnemyRuntime> _areaSlowed = new HashSet<EnemyRuntime>();

        public EnemyZoneSource(System.Action<SpritePresentationRuntime> damageFeedback = null, bool suppressAreaUnitFeedback = false)
        { _damageFeedback = damageFeedback; _suppressAreaUnitFeedback = suppressAreaUnitFeedback; }

        public int Refresh()
        {
            ClearAreaSlows();
            EnemyRegistry.CopyAliveTo(_alive);
            if (_pending.Count > 0 && Time.time - _lastFlushTime >= DamageIntervalSeconds) Flush();
            return _alive.Count;
        }

        public Vector2 Position(int index) => _alive[index].Position;

        public void Slow(int index, float fraction, float seconds, ContentId source)
        {
            var enemy = _alive[index];
            if (!enemy.IsAlive) return;
            if (seconds == 0f)
            {
                enemy.SetAreaSlowFraction(Mathf.Max(enemy.AreaSlowFraction, fraction));
                _areaSlowed.Add(enemy);
                return;
            }
            enemy.ApplyControl(new CombatDamageRequest(new CombatSource(default, source, CombatSourceOrigin.Unknown), 0f,
                new CombatControlProfile(slowFraction: fraction, slowSeconds: seconds, channel: "zone")));
        }

        public void Damage(int index, float amount, ContentId source)
        {
            var enemy = _alive[index];
            _pending[enemy] = _pending.TryGetValue(enemy, out var sum) ? sum + amount : amount;
        }

        public void Strike(int index, float amount, ContentId source)
        {
            var enemy = _alive[index];
            if (enemy != null && enemy.IsAlive) enemy.TakeDamage(amount);
        }

        private void Flush()
        {
            _lastFlushTime = Time.time;
            foreach (var pair in _pending)
            {
                if (pair.Key == null || !pair.Key.IsAlive) continue;
                var body = pair.Key.BodyPresentation;
                var previous = body != null && body.SuppressDamageFeedback;
                try
                {
                    if (body != null && _suppressAreaUnitFeedback) body.SuppressDamageFeedback = true;
                    if (pair.Key.TakeDamage(pair.Value) > 0f && !_suppressAreaUnitFeedback) _damageFeedback?.Invoke(body);
                }
                finally { if (body != null) body.SuppressDamageFeedback = previous; }
            }
            _pending.Clear();
        }

        private void ClearAreaSlows()
        {
            foreach (var enemy in _areaSlowed) if (enemy != null) enemy.SetAreaSlowFraction(0f);
            _areaSlowed.Clear();
        }

        public void Dispose() { ClearAreaSlows(); _pending.Clear(); _alive.Clear(); }
    }
}
