using System.Collections.Generic;
using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill
{
    /// <summary>
    /// Expanding wave (SKILL-004 contract): the damage front grows from 0 to the full radius over
    /// the expansion time around a center fixed at wave start. Targets take damage only when the
    /// front reaches them, and each target life at most once per wave. Knockback is radial from
    /// the center. Pause freezes the front because the executor ticks only while running.
    /// </summary>
    internal sealed class ExpandingAreaState
    {
        private readonly HashSet<EnemyTargetLife> _hit = new HashSet<EnemyTargetLife>();
        private readonly List<Collider2D> _colliders = new List<Collider2D>();
        private readonly EnemyDamageRequest _damage;
        private float _elapsed;

        public Vector2 Center { get; }
        public float MaxRadius { get; }
        public float ExpansionSeconds { get; }
        public float CurrentRadius => MaxRadius * Mathf.Clamp01(_elapsed / ExpansionSeconds);
        public bool IsComplete => _elapsed >= ExpansionSeconds;
        public object VisualHandle { get; set; }
        public Game.Presentation.SkillWorldEffectProfile VisualProfile { get; set; }
        public int HitCount => _hit.Count;

        public ExpandingAreaState(Vector2 center, float maxRadius, float expansionSeconds, EnemyDamageRequest damage)
        {
            Center = center;
            MaxRadius = maxRadius;
            ExpansionSeconds = expansionSeconds;
            _damage = damage;
        }

        /// <summary>Advances the front and damages newly reached targets. Returns true once complete.</summary>
        public bool Tick(float deltaTime)
        {
            _elapsed = Mathf.Min(ExpansionSeconds, _elapsed + deltaTime);
            var radius = CurrentRadius;
            if (radius <= 0f) return IsComplete;
            _colliders.Clear();
            var filter = new ContactFilter2D();
            filter.NoFilter();
            Physics2D.OverlapCircle(Center, radius, filter, _colliders);
            for (var i = 0; i < _colliders.Count; i++)
            {
                if (_colliders[i] == null) continue;
                var receiver = _colliders[i].GetComponentInParent<IEnemyDamageReceiver>();
                var life = new EnemyTargetLife(receiver);
                if (!life.IsAlive || !_hit.Add(life)) continue;
                var radial = receiver.Position - Center;
                receiver.ApplyDamage(_damage.WithDirection(radial.x, radial.y));
            }
            return IsComplete;
        }
    }
}
