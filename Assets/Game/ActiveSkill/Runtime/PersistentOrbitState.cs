using System;
using System.Collections.Generic;
using Game.Combat;
using Game.Content;
using Game.Enemy;
using Game.Pooling;
using Game.Presentation;
using UnityEngine;

namespace Game.ActiveSkill
{
    /// <summary>
    /// Continuous orbit (SKILL-003 contract): blades rotate around the owner while the skill keeps
    /// refreshing the state. Hits use the swept blade path between ticks (capsule overlap), so fast
    /// rotation does not skip targets. Each blade may hit the same enemy life at most once per
    /// <see cref="OrbitEffect.HitCooldownSeconds"/>. A refresh with a new level keeps the phase and
    /// blade history, so upgrades never create a blade-free window or a duplicate orbit.
    /// </summary>
    internal sealed class PersistentOrbitState : IDisposable
    {
        private const float MaxSweepDegreesPerStep = 15f;
        private readonly Transform _owner;
        private readonly GameObjectPool<SpriteRenderer> _bladePool;
        private readonly List<SpriteRenderer> _blades = new List<SpriteRenderer>();
        private readonly Dictionary<(int blade, EnemyTargetLife life), float> _hitCooldowns = new Dictionary<(int, EnemyTargetLife), float>();
        private readonly List<(int, EnemyTargetLife)> _expired = new List<(int, EnemyTargetLife)>();
        private readonly List<(int, EnemyTargetLife)> _keys = new List<(int, EnemyTargetLife)>();
        private readonly List<Collider2D> _colliders = new List<Collider2D>();
        private readonly HashSet<IEnemyDamageReceiver> _hitThisStep = new HashSet<IEnemyDamageReceiver>();
        private Vector2[] _previous = Array.Empty<Vector2>();
        private SpriteDefinition _visual;
        private float _phaseDegrees;
        private float _sinceRefresh;
        private EnemyDamageRequest _damage;

        public ContentId SourceId { get; }
        public Transform Owner => _owner;
        public OrbitEffect Effect { get; private set; }
        public float RangeMultiplier { get; private set; } = 1f;
        public float SizeMultiplier { get; private set; } = 1f;
        public float PhaseDegrees => _phaseDegrees;
        public int BladeCount => Effect?.BladeCount ?? 0;
        public int VisualBladeCount => _blades.Count;

        /// <summary>Running time without a refresh after which the orbit is considered removed.</summary>
        public float LeaseSeconds { get; private set; }

        public PersistentOrbitState(ContentId sourceId, Transform owner, GameObjectPool<SpriteRenderer> bladePool, float initialPhaseDegrees)
        {
            SourceId = sourceId;
            _owner = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
            _bladePool = bladePool ?? throw new ArgumentNullException(nameof(bladePool));
            _phaseDegrees = initialPhaseDegrees;
        }

        public void Refresh(OrbitEffect effect, EnemyDamageRequest damage, float rangeMultiplier, float sizeMultiplier,
            float leaseSeconds, SpriteDefinition visual)
        {
            var countChanged = Effect == null || Effect.BladeCount != effect.BladeCount;
            Effect = effect ?? throw new ArgumentNullException(nameof(effect));
            _damage = damage;
            RangeMultiplier = rangeMultiplier;
            SizeMultiplier = sizeMultiplier;
            LeaseSeconds = leaseSeconds;
            _sinceRefresh = 0f;
            if (countChanged)
            {
                // New blades start on their evenly spaced positions; history of removed indices is dropped.
                _previous = BladePositions();
                _hitCooldowns.Clear();
            }
            if (countChanged || _visual != visual)
            {
                _visual = visual;
                RebuildVisuals();
            }
            UpdateVisualPose();
        }

        /// <summary>Advances rotation and applies swept hits. Returns false when the lease has expired.</summary>
        public bool Tick(float deltaTime)
        {
            if (_owner == null || Effect == null) return false;
            _sinceRefresh += deltaTime;
            if (_sinceRefresh > LeaseSeconds) return false;
            TickCooldowns(deltaTime);
            var totalDegrees = Effect.AngularSpeedDegrees * deltaTime;
            var steps = Math.Max(1, Mathf.CeilToInt(totalDegrees / MaxSweepDegreesPerStep));
            for (var step = 0; step < steps; step++)
            {
                _phaseDegrees = Mathf.Repeat(_phaseDegrees + totalDegrees / steps, 360f);
                var current = BladePositions();
                for (var blade = 0; blade < current.Length; blade++)
                    Sweep(blade, blade < _previous.Length ? _previous[blade] : current[blade], current[blade]);
                _previous = current;
            }
            UpdateVisualPose();
            return true;
        }

        public void Dispose()
        {
            ReturnBlades();
            _hitCooldowns.Clear();
        }

        private void ReturnBlades()
        {
            for (var i = 0; i < _blades.Count; i++)
            {
                var blade = _blades[i];
                if (blade == null) continue;
                blade.sprite = null;
                blade.transform.localPosition = Vector3.zero;
                blade.transform.localRotation = Quaternion.identity;
                blade.transform.localScale = Vector3.one;
                _bladePool.Return(blade);
            }
            _blades.Clear();
        }

        private Vector2[] BladePositions()
        {
            var center = (Vector2)_owner.position;
            var positions = new Vector2[Effect.BladeCount];
            var radius = Effect.Radius * RangeMultiplier;
            for (var i = 0; i < positions.Length; i++)
            {
                var radians = (_phaseDegrees + i * 360f / positions.Length) * Mathf.Deg2Rad;
                positions[i] = center + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
            }
            return positions;
        }

        private void Sweep(int blade, Vector2 from, Vector2 to)
        {
            var hitbox = Effect.BladeHitboxRadius * SizeMultiplier;
            var offset = to - from;
            var length = offset.magnitude;
            _colliders.Clear();
            var filter = new ContactFilter2D();
            filter.NoFilter();
            if (length <= Mathf.Epsilon)
                Physics2D.OverlapCircle(to, hitbox, filter, _colliders);
            else
                Physics2D.OverlapCapsule((from + to) * .5f, new Vector2(length + hitbox * 2f, hitbox * 2f),
                    CapsuleDirection2D.Horizontal, Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg, filter, _colliders);
            _hitThisStep.Clear();
            var center = (Vector2)_owner.position;
            for (var i = 0; i < _colliders.Count; i++)
            {
                if (_colliders[i] == null) continue;
                var receiver = _colliders[i].GetComponentInParent<IEnemyDamageReceiver>();
                if (receiver == null || !_hitThisStep.Add(receiver)) continue;
                var life = new EnemyTargetLife(receiver);
                if (!life.IsAlive || _hitCooldowns.ContainsKey((blade, life))) continue;
                _hitCooldowns[(blade, life)] = Effect.HitCooldownSeconds;
                var radial = receiver.Position - center;
                receiver.ApplyDamage(_damage.WithDirection(radial.x, radial.y));
                if (_owner == null) return;
            }
        }

        private void TickCooldowns(float deltaTime)
        {
            _expired.Clear();
            _keys.Clear();
            _keys.AddRange(_hitCooldowns.Keys);
            foreach (var key in _keys)
            {
                var remaining = _hitCooldowns[key] - deltaTime;
                if (remaining <= 0f || !key.Item2.IsAlive) _expired.Add(key);
                else _hitCooldowns[key] = remaining;
            }
            foreach (var key in _expired) _hitCooldowns.Remove(key);
        }

        private void RebuildVisuals()
        {
            ReturnBlades();
            if (_visual == null) return;
            var size = _visual.Sprite.bounds.size;
            var diameter = Effect.BladeHitboxRadius * 2f * SizeMultiplier * _visual.ProjectilePresentation.VisualScale;
            var scale = diameter / Mathf.Max(size.x, size.y);
            for (var i = 0; i < Effect.BladeCount; i++)
            {
                var blade = _bladePool.Rent();
                blade.transform.SetParent(_owner, false);
                blade.sprite = _visual.Sprite;
                blade.color = Color.white;
                blade.transform.localScale = Vector3.one * scale;
                _blades.Add(blade);
            }
        }

        private void UpdateVisualPose()
        {
            if (_blades.Count == 0) return;
            var radius = Effect.Radius * RangeMultiplier;
            var size = _visual.Sprite.bounds.size;
            var scale = Effect.BladeHitboxRadius * 2f * SizeMultiplier * _visual.ProjectilePresentation.VisualScale / Mathf.Max(size.x, size.y);
            for (var i = 0; i < _blades.Count; i++)
            {
                var angle = _phaseDegrees + i * 360f / _blades.Count;
                var radians = angle * Mathf.Deg2Rad;
                _blades[i].transform.localPosition = new Vector3(Mathf.Cos(radians) * radius, Mathf.Sin(radians) * radius, 0f);
                _blades[i].transform.localRotation = Quaternion.Euler(0f, 0f, angle + 90f);
                _blades[i].transform.localScale = Vector3.one * scale;
            }
        }
    }
}
