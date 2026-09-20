using System;
using UnityEngine;

namespace Game.Enemy
{
    public sealed class EnemyMovementController
    {
        private readonly EnemyMovementProfile _profile;
        private float _elapsed;
        private float _dashCooldownRemaining;
        private float _dashPhaseRemaining;
        private Vector2 _dashDirection = Vector2.right;
        private EnemyMovementPhase _dashPhase = EnemyMovementPhase.Seeking;

        public EnemyMovementController(EnemyMovementProfile profile)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _dashCooldownRemaining = profile.DashCooldownSeconds;
        }

        public EnemyMovementFrame Tick(
            Vector2 currentPosition,
            Vector2 targetPosition,
            float movementSpeed,
            float deltaTime,
            bool isSimulating)
        {
            if (!isSimulating)
                return new EnemyMovementFrame(Vector2.zero, CurrentPausedPhase(), _dashDirection);
            if (deltaTime < 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
                throw new ArgumentOutOfRangeException(nameof(deltaTime));

            _elapsed += deltaTime;
            var offset = targetPosition - currentPosition;
            var toward = offset.sqrMagnitude <= Mathf.Epsilon ? Vector2.zero : offset.normalized;
            var distance = offset.magnitude;

            switch (_profile.Kind)
            {
                case EnemyMovementKind.Seek:
                    return Frame(toward * movementSpeed, EnemyMovementPhase.Seeking);
                case EnemyMovementKind.KeepDistance:
                    if (distance > _profile.PreferredDistance + _profile.DistanceTolerance)
                        return Frame(toward * movementSpeed, EnemyMovementPhase.Approaching);
                    if (distance < Mathf.Max(0f, _profile.PreferredDistance - _profile.DistanceTolerance))
                        return Frame(-toward * movementSpeed, EnemyMovementPhase.Retreating);
                    return Frame(Vector2.zero, EnemyMovementPhase.HoldingDistance);
                case EnemyMovementKind.Orbit:
                    return CalculateOrbit(toward, distance, movementSpeed);
                case EnemyMovementKind.Zigzag:
                    return CalculateZigzag(toward, movementSpeed);
                case EnemyMovementKind.ApproachRetreat:
                    return CalculateApproachRetreat(toward, movementSpeed);
                case EnemyMovementKind.TelegraphedDash:
                    return CalculateDash(toward, movementSpeed, deltaTime);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private EnemyMovementFrame CalculateOrbit(Vector2 toward, float distance, float movementSpeed)
        {
            var radial = Vector2.zero;
            if (distance > _profile.PreferredDistance + _profile.DistanceTolerance)
                radial = toward;
            else if (distance < Mathf.Max(0f, _profile.PreferredDistance - _profile.DistanceTolerance))
                radial = -toward;
            var tangent = new Vector2(-toward.y, toward.x) * _profile.LateralStrength;
            var combined = radial + tangent;
            return Frame(combined.sqrMagnitude <= Mathf.Epsilon ? Vector2.zero : combined.normalized * movementSpeed,
                EnemyMovementPhase.Orbiting);
        }

        private EnemyMovementFrame CalculateZigzag(Vector2 toward, float movementSpeed)
        {
            var tangent = new Vector2(-toward.y, toward.x);
            var wave = Mathf.Sin(_elapsed * Mathf.PI * 2f / _profile.CycleSeconds) * _profile.LateralStrength;
            var combined = toward + tangent * wave;
            return Frame(combined.sqrMagnitude <= Mathf.Epsilon ? Vector2.zero : combined.normalized * movementSpeed,
                EnemyMovementPhase.Zigzagging);
        }

        private EnemyMovementFrame CalculateApproachRetreat(Vector2 toward, float movementSpeed)
        {
            var approach = Mathf.Repeat(_elapsed, _profile.CycleSeconds) < _profile.CycleSeconds * 0.5f;
            return Frame((approach ? toward : -toward) * movementSpeed,
                approach ? EnemyMovementPhase.Approaching : EnemyMovementPhase.Retreating);
        }

        private EnemyMovementFrame CalculateDash(Vector2 toward, float movementSpeed, float deltaTime)
        {
            if (_dashPhase == EnemyMovementPhase.TelegraphingDash)
            {
                _dashPhaseRemaining -= deltaTime;
                if (_dashPhaseRemaining > 0f)
                    return Frame(Vector2.zero, _dashPhase, _dashDirection);
                _dashPhase = EnemyMovementPhase.Dashing;
                _dashPhaseRemaining = _profile.DashDurationSeconds;
                return Frame(_dashDirection * movementSpeed * _profile.DashSpeedMultiplier, _dashPhase, _dashDirection);
            }

            if (_dashPhase == EnemyMovementPhase.Dashing)
            {
                _dashPhaseRemaining -= deltaTime;
                if (_dashPhaseRemaining > 0f)
                    return Frame(_dashDirection * movementSpeed * _profile.DashSpeedMultiplier, _dashPhase, _dashDirection);
                _dashPhase = EnemyMovementPhase.Seeking;
                _dashCooldownRemaining = _profile.DashCooldownSeconds;
            }

            _dashCooldownRemaining -= deltaTime;
            if (_dashCooldownRemaining <= 0f)
            {
                _dashDirection = toward.sqrMagnitude > Mathf.Epsilon ? toward : _dashDirection;
                _dashPhase = EnemyMovementPhase.TelegraphingDash;
                _dashPhaseRemaining = _profile.DashTelegraphSeconds;
                if (_dashPhaseRemaining > 0f)
                    return Frame(Vector2.zero, _dashPhase, _dashDirection);
                _dashPhase = EnemyMovementPhase.Dashing;
                _dashPhaseRemaining = _profile.DashDurationSeconds;
                return Frame(_dashDirection * movementSpeed * _profile.DashSpeedMultiplier, _dashPhase, _dashDirection);
            }

            return Frame(toward * movementSpeed, EnemyMovementPhase.Seeking);
        }

        private EnemyMovementPhase CurrentPausedPhase() =>
            _dashPhase == EnemyMovementPhase.TelegraphingDash || _dashPhase == EnemyMovementPhase.Dashing
                ? _dashPhase
                : EnemyMovementPhase.Seeking;

        private EnemyMovementFrame Frame(Vector2 velocity, EnemyMovementPhase phase, Vector2 telegraph = default) =>
            new EnemyMovementFrame(velocity, phase, telegraph);
    }
}
