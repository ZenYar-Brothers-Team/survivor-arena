using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Content;

namespace Game.Enemy
{
    public sealed class EnemyAttackController
    {
        private readonly EnemyAttackProfile _profile;
        private readonly bool _repeat;
        private float _cooldownRemaining;
        private float _burstRemaining;
        private int _burstShotsRemaining;
        private float _rotationDegrees;
        private float _telegraphRemaining;
        private bool _hasFired;
        // Boss sequence owner advances only after the whole burst and its cooldown, never during wind-up.
        public bool CycleCompletesWithin(float deltaTime) => _hasFired && Phase == EnemyAttackPhase.Cooldown &&
            _burstShotsRemaining == 0 && _cooldownRemaining <= deltaTime;
        public EnemyAttackPhase Phase { get; private set; }
        public int BurstShotsRemaining => _burstShotsRemaining;
        public float PhaseRemaining => Phase == EnemyAttackPhase.Telegraphing ? _telegraphRemaining :
            Phase == EnemyAttackPhase.Bursting ? _burstRemaining : Mathf.Max(0f, _cooldownRemaining);
        public Vector2 AimDirection { get; private set; } = Vector2.right;

        public EnemyAttackController(EnemyAttackProfile profile, bool repeat = true)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _repeat = repeat;
            if (WindupCadence) _cooldownRemaining = profile.CooldownSeconds;
        }

        private bool WindupCadence => _profile.Cadence == EnemyAttackCadence.WindupStartToStart;

        // Sequence re-entry starts a full wind-up, while retaining this pattern's accumulated spiral rotation.
        public void RestartCycle()
        {
            _hasFired = false;
            _cooldownRemaining = 0;
            _burstRemaining = 0;
            _burstShotsRemaining = 0;
            _telegraphRemaining = 0;
            Phase = EnemyAttackPhase.Cooldown;
        }

        public EnemyShotCommand[] Tick(float deltaTime, bool isSimulating, Vector2 aimDirection)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            if (!isSimulating)
                return Array.Empty<EnemyShotCommand>();

            // Wind-up cadence keeps the aim snapshotted at wind-up start until the shot.
            if (aimDirection.sqrMagnitude > Mathf.Epsilon && !(WindupCadence && Phase == EnemyAttackPhase.Telegraphing))
                AimDirection = aimDirection.normalized;
            var shots = new List<EnemyShotCommand>();
            if (Phase == EnemyAttackPhase.Telegraphing)
            {
                if (WindupCadence) _cooldownRemaining -= deltaTime;
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
            if (_cooldownRemaining <= 0f && _burstShotsRemaining == 0 && (!_hasFired || _repeat))
            {
                // Start-to-start: the next interval begins with this wind-up, not with the shot.
                if (WindupCadence) _cooldownRemaining += _profile.CooldownSeconds;
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
            _hasFired = true;
            shots.AddRange(EnemyProjectilePatternGenerator.Create(_profile, AimDirection, _rotationDegrees));
            if (_profile.Pattern == EnemyProjectilePattern.Burst)
            {
                _burstShotsRemaining = _profile.ProjectileCount - 1;
                _burstRemaining = _profile.BurstIntervalSeconds;
            }
            if (_profile.Pattern == EnemyProjectilePattern.Spiral)
                _rotationDegrees = Mathf.Repeat(_rotationDegrees + _profile.RotationStepDegrees, 360f);
            if (!WindupCadence) _cooldownRemaining = _profile.CooldownSeconds;
            Phase = _burstShotsRemaining > 0 ? EnemyAttackPhase.Bursting : EnemyAttackPhase.Cooldown;
        }
    }
}
