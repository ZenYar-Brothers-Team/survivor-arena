using System;
using UnityEngine;
using Game.Content;

namespace Game.Enemy
{
    public sealed class EnemyMovementController
    {
        private readonly EnemyMovementProfile _profile;
        private float _elapsed;
        private float _dashCooldownRemaining;
        private float _dashPhaseRemaining;
        private Vector2 _dashDirection = Vector2.right;
        public EnemyMovementPhase Phase { get; private set; } = EnemyMovementPhase.Seeking;
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
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            NumericValidation.ValidateNonNegative(movementSpeed, nameof(movementSpeed));
            if (!isSimulating)
                return new EnemyMovementFrame(Vector2.zero, Phase, _dashDirection);

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
                case EnemyMovementKind.DistanceReposition:
                    return CalculateReposition(toward, distance, movementSpeed);
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

        // ENEMY-005 (baseline v1): hold distance, then briefly move tangentially with distance correction;
        // the tangent side alternates every cycle and the result never exceeds movement speed.
        private EnemyMovementFrame CalculateReposition(Vector2 toward, float distance, float movementSpeed)
        {
            var cycle = Mathf.FloorToInt(_elapsed / _profile.CycleSeconds);
            var inCycle = _elapsed - cycle * _profile.CycleSeconds;
            var radial = Vector2.zero;
            if (distance > _profile.PreferredDistance + _profile.DistanceTolerance) radial = toward;
            else if (distance < Mathf.Max(0f, _profile.PreferredDistance - _profile.DistanceTolerance)) radial = -toward;
            if (inCycle < _profile.CycleSeconds - _profile.RepositionSeconds)
                return Frame(radial * movementSpeed, radial == Vector2.zero ? EnemyMovementPhase.HoldingDistance :
                    radial == toward ? EnemyMovementPhase.Approaching : EnemyMovementPhase.Retreating);
            var side = cycle % 2 == 0 ? 1f : -1f;
            var combined = radial + new Vector2(-toward.y, toward.x) * (_profile.LateralStrength * side);
            return Frame(combined.sqrMagnitude <= Mathf.Epsilon ? Vector2.zero : combined.normalized * movementSpeed,
                EnemyMovementPhase.Repositioning);
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

        private EnemyMovementFrame Frame(Vector2 velocity, EnemyMovementPhase phase, Vector2 telegraph = default)
        {
            Phase = phase;
            return new EnemyMovementFrame(velocity, phase, telegraph);
        }
    }
}
