using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Enemy
{
    public sealed class EnemyAttackController
    {
        private readonly EnemyAttackProfile _profile;
        private float _cooldownRemaining;
        private float _burstRemaining;
        private int _burstShotsRemaining;
        private float _rotationDegrees;

        public EnemyAttackController(EnemyAttackProfile profile)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        }

        public EnemyShotCommand[] Tick(float deltaTime, bool isSimulating, Vector2 aimDirection)
        {
            if (deltaTime < 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (!isSimulating)
                return Array.Empty<EnemyShotCommand>();

            var shots = new List<EnemyShotCommand>();
            _cooldownRemaining -= deltaTime;

            if (_burstShotsRemaining > 0)
            {
                _burstRemaining -= deltaTime;
                while (_burstShotsRemaining > 0 && _burstRemaining <= 0f)
                {
                    shots.AddRange(EnemyProjectilePatternGenerator.Create(_profile, aimDirection));
                    _burstShotsRemaining--;
                    _burstRemaining += _profile.BurstIntervalSeconds;
                }
            }

            if (_cooldownRemaining <= 0f && _burstShotsRemaining == 0)
            {
                shots.AddRange(EnemyProjectilePatternGenerator.Create(_profile, aimDirection, _rotationDegrees));
                if (_profile.Pattern == EnemyProjectilePattern.Burst)
                {
                    _burstShotsRemaining = _profile.ProjectileCount - 1;
                    _burstRemaining = _profile.BurstIntervalSeconds;
                }
                if (_profile.Pattern == EnemyProjectilePattern.Spiral)
                    _rotationDegrees = Mathf.Repeat(_rotationDegrees + _profile.RotationStepDegrees, 360f);
                _cooldownRemaining += _profile.CooldownSeconds;
            }

            return shots.ToArray();
        }
    }
}
