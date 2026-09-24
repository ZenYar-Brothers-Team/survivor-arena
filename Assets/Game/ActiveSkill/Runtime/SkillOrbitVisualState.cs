using System;
using Game.Pooling;
using Game.Presentation;
using UnityEngine;

namespace Game.ActiveSkill
{
    internal sealed class SkillOrbitVisualState : IDisposable
    {
        private readonly Transform _owner;
        private readonly GameObjectPool<SpriteRenderer> _pool;
        private readonly SpriteRenderer[] _blades;
        private readonly float _radius;
        private readonly float _angularSpeedDegrees;
        private readonly float _durationSeconds;
        private readonly float _baseRotationDegrees;
        private float _elapsed;

        public int BladeCount => _blades.Length;

        public SkillOrbitVisualState(Transform owner, SpriteDefinition visual, OrbitEffect effect,
            float rangeMultiplier, float sizeMultiplier, float baseRotationDegrees,
            GameObjectPool<SpriteRenderer> pool)
        {
            _owner = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
            if (visual == null) throw new ArgumentNullException(nameof(visual));
            visual.RequireRole(SpriteRole.Projectile);
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _radius = effect.Radius * rangeMultiplier;
            _angularSpeedDegrees = effect.AngularSpeedDegrees;
            _durationSeconds = effect.DurationSeconds;
            _baseRotationDegrees = baseRotationDegrees;
            _blades = new SpriteRenderer[effect.BladeCount];
            var spriteSize = visual.Sprite.bounds.size;
            var diameter = effect.BladeHitboxRadius * 2f * sizeMultiplier *
                           visual.ProjectilePresentation.VisualScale;
            var scale = diameter / Mathf.Max(spriteSize.x, spriteSize.y);
            for (var i = 0; i < _blades.Length; i++)
            {
                var blade = _pool.Rent();
                blade.transform.SetParent(_owner, false);
                blade.sprite = visual.Sprite;
                blade.color = Color.white;
                blade.transform.localScale = Vector3.one * scale;
                _blades[i] = blade;
            }
            UpdatePose();
        }

        public bool Tick(float deltaTime, bool isRunning)
        {
            if (!isRunning) return false;
            _elapsed = Mathf.Min(_durationSeconds, _elapsed + deltaTime);
            UpdatePose();
            return _elapsed >= _durationSeconds;
        }

        public void Dispose()
        {
            for (var i = 0; i < _blades.Length; i++)
            {
                var blade = _blades[i];
                if (blade == null) continue;
                blade.sprite = null;
                blade.color = Color.white;
                blade.transform.localPosition = Vector3.zero;
                blade.transform.localRotation = Quaternion.identity;
                blade.transform.localScale = Vector3.one;
                _pool.Return(blade);
                _blades[i] = null;
            }
        }

        private void UpdatePose()
        {
            for (var i = 0; i < _blades.Length; i++)
            {
                var angle = _baseRotationDegrees + _elapsed * _angularSpeedDegrees +
                            i * 360f / _blades.Length;
                var radians = angle * Mathf.Deg2Rad;
                _blades[i].transform.localPosition = new Vector3(
                    Mathf.Cos(radians) * _radius, Mathf.Sin(radians) * _radius, 0f);
                _blades[i].transform.localRotation = Quaternion.Euler(0f, 0f, angle + 90f);
            }
        }
    }
}
