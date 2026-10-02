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
        private readonly HashSet<EnemyRuntime> _areaModified = new HashSet<EnemyRuntime>();
        private readonly Dictionary<EnemyRuntime, (System.Guid life, float until)> _portalCooldowns = new Dictionary<EnemyRuntime, (System.Guid, float)>();
        private readonly List<EnemyRuntime> _expiredPortals = new List<EnemyRuntime>();
        private readonly HashSet<PortalTransitRuntime> _transits = new HashSet<PortalTransitRuntime>();
        private readonly Game.Run.RunController _run;
        private readonly ZoneSealPresentationProfile _portalProfile;
        private SlowStatusPresentationProfile _speedProfile;

        public EnemyZoneSource(System.Action<SpritePresentationRuntime> damageFeedback = null, bool suppressAreaUnitFeedback = false,
            Game.Run.RunController run = null, ZoneSealPresentationProfile portalProfile = null)
        { _damageFeedback = damageFeedback; _suppressAreaUnitFeedback = suppressAreaUnitFeedback; _run = run; _portalProfile = portalProfile; }

        public int Refresh()
        {
            ClearAreaSlows();
            foreach (var enemy in _areaModified) if (enemy != null) enemy.ZoneInfluence.SetArea(0f, 0f, 0f);
            _areaModified.Clear();
            EnemyRegistry.CopyAliveTo(_alive);
            if (_pending.Count > 0 && Time.time - _lastFlushTime >= DamageIntervalSeconds) Flush();
            return _alive.Count;
        }

        public Vector2 Position(int index) => _alive[index].Position;

        public void SetArea(int index, float movementBonus, float actionBonus, float regeneration, float defense, float deltaTime)
        {
            var enemy = _alive[index];
            if (!enemy.IsAlive) return;
            enemy.ZoneInfluence.SetArea(movementBonus, actionBonus, defense);
            _areaModified.Add(enemy);
            var transit = enemy.GetComponent<PortalTransitRuntime>();
            if (transit != null && transit.IsActive) enemy.ZoneInfluence.Tick(deltaTime, true);
            if (transit == null || !transit.IsActive) enemy.Health.Heal(regeneration * deltaTime);
            var body = enemy.BodyPresentation;
            if (body == null) return;
            var speed = body.GetComponent<SpeedStatusPresentationRuntime>();
            if (enemy.ZoneInfluence.BuffRemaining > 0f)
            {
                _speedProfile ??= FixtureSlowStatusPresentationCatalog.Create();
                speed ??= body.gameObject.AddComponent<SpeedStatusPresentationRuntime>();
                speed.Apply(body, _speedProfile, true, enemy.ZoneInfluence.BuffRemaining01, false, _run?.Model?.Elapsed ?? Time.time);
            }
            else if (speed != null) speed.Clear();
        }
        public void SpeedBurst(int index, float bonus, float seconds)
        { if (_alive[index].IsAlive) _alive[index].ZoneInfluence.ApplyBurst(bonus, seconds); }
        public bool Teleport(int index, Vector2 destination, float cooldownSeconds, float runSeconds)
        {
            var enemy = _alive[index];
            if (!enemy.IsAlive) return false;
            _expiredPortals.Clear();
            foreach (var pair in _portalCooldowns)
                if (pair.Key == null || !pair.Key.IsAlive || pair.Key.LifeId != pair.Value.life || pair.Value.until <= runSeconds)
                    _expiredPortals.Add(pair.Key);
            foreach (var expired in _expiredPortals) _portalCooldowns.Remove(expired);
            if (_portalCooldowns.ContainsKey(enemy)) return false;
            if (_portalProfile != null)
            {
                var transit = enemy.GetComponent<PortalTransitRuntime>() ?? enemy.gameObject.AddComponent<PortalTransitRuntime>();
                if (!transit.Begin(destination, _run, _portalProfile, enemy.Health, enemy.BodyPresentation)) return false;
                _transits.Add(transit);
            }
            else
            {
                enemy.transform.position = destination;
                var body = enemy.GetComponent<Rigidbody2D>(); body.position = destination; body.linearVelocity = Vector2.zero;
            }
            _portalCooldowns[enemy] = (enemy.LifeId, runSeconds + cooldownSeconds);
            return true;
        }

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

        public void Dispose()
        {
            ClearAreaSlows();
            foreach (var enemy in _areaModified) if (enemy != null) enemy.ZoneInfluence.Reset();
            foreach (var enemy in _alive) if (enemy != null) enemy.ZoneInfluence.Reset();
            foreach (var transit in _transits) if (transit != null) transit.Shutdown();
            _areaModified.Clear(); _transits.Clear(); _portalCooldowns.Clear(); _pending.Clear(); _alive.Clear();
        }
    }
}
