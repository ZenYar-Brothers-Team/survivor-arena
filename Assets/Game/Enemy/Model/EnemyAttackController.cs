using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Content;

namespace Game.Enemy
{
    public sealed class EnemyAttackController
    {
        private readonly EnemyAttackProfile _profile;
        private float _cooldownRemaining;
        private float _burstRemaining;
        private int _burstShotsRemaining;
        private float _rotationDegrees;
        private float _telegraphRemaining;
        public EnemyAttackPhase Phase { get; private set; }
        public int BurstShotsRemaining => _burstShotsRemaining;
        public float PhaseRemaining => Phase == EnemyAttackPhase.Telegraphing ? _telegraphRemaining :
            Phase == EnemyAttackPhase.Bursting ? _burstRemaining : Mathf.Max(0f, _cooldownRemaining);
        public Vector2 AimDirection { get; private set; } = Vector2.right;

        public EnemyAttackController(EnemyAttackProfile profile)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        }

        public EnemyShotCommand[] Tick(float deltaTime, bool isSimulating, Vector2 aimDirection)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            if (!isSimulating)
                return Array.Empty<EnemyShotCommand>();

            if (aimDirection.sqrMagnitude > Mathf.Epsilon) AimDirection = aimDirection.normalized;
            var shots = new List<EnemyShotCommand>();
            if (Phase == EnemyAttackPhase.Telegraphing)
            {
                _telegraphRemaining = Mathf.Max(0f, _telegraphRemaining - deltaTime);
                if (_telegraphRemaining > 0f) return Array.Empty<EnemyShotCommand>();
                Fire(shots);
                return shots.ToArray();
            }
            _cooldownRemaining -= deltaTime;

            if (_burstShotsRemaining > 0)
            {
                _burstRemaining -= deltaTime;
                while (_burstShotsRemaining > 0 && _burstRemaining <= 0f)
                {
                    shots.AddRange(EnemyProjectilePatternGenerator.Create(_profile, AimDirection));
                    _burstShotsRemaining--;
                    _burstRemaining += _profile.BurstIntervalSeconds;
                }
            }

            Phase = _burstShotsRemaining > 0 ? EnemyAttackPhase.Bursting : EnemyAttackPhase.Cooldown;
            if (_cooldownRemaining <= 0f && _burstShotsRemaining == 0)
            {
                if (_profile.TelegraphSeconds > 0f)
                {
                    Phase = EnemyAttackPhase.Telegraphing;
                    _telegraphRemaining = _profile.TelegraphSeconds;
                }
                else Fire(shots);
            }
            return shots.ToArray();
        }

        private void Fire(List<EnemyShotCommand> shots)
        {
            shots.AddRange(EnemyProjectilePatternGenerator.Create(_profile, AimDirection, _rotationDegrees));
            if (_profile.Pattern == EnemyProjectilePattern.Burst)
            {
                _burstShotsRemaining = _profile.ProjectileCount - 1;
                _burstRemaining = _profile.BurstIntervalSeconds;
            }
            if (_profile.Pattern == EnemyProjectilePattern.Spiral)
                _rotationDegrees = Mathf.Repeat(_rotationDegrees + _profile.RotationStepDegrees, 360f);
            _cooldownRemaining = _profile.CooldownSeconds;
            Phase = _burstShotsRemaining > 0 ? EnemyAttackPhase.Bursting : EnemyAttackPhase.Cooldown;
        }
    }
}
