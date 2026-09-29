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
        private int _dashesLeftInSequence;
        private readonly System.Random _offsetRandom;
        private Vector2 _offsetDirection;
        private float _offsetCycleTime;
        private Vector2 _committedDirection;
        private float _courseRemaining;
        private Vector2 _lastSamplePosition;
        private Vector2 _lastDesiredVelocity;
        private bool _hasLastSample;
        private float _blockedTime;
        private float _sidestepNearTime;
        private float _sidestepRemaining;
        private float _sidestepCooldown;
        private float _sidestepSide;
        private float _arcCycleTime;
        private float _arcSide;
        private Vector2 _inertialDirection;
        private float _inertialTurnSide;
        public EnemyMovementPhase Phase { get; private set; } = EnemyMovementPhase.Seeking;
        private EnemyMovementPhase _dashPhase = EnemyMovementPhase.Seeking;

        public EnemyMovementController(EnemyMovementProfile profile, System.Random random = null)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _dashCooldownRemaining = profile.DashCooldownSeconds;
            if (profile.Kind == EnemyMovementKind.OffsetPursuit)
            {
                _offsetRandom = random ?? new System.Random(0);
                SelectOffsetDirection();
                _offsetCycleTime = (float)_offsetRandom.NextDouble() * profile.CycleSeconds;
            }
            if (profile.Kind == EnemyMovementKind.CommittedPursuit)
                _courseRemaining = profile.CycleSeconds * (0.5f + 0.5f * (float)(random?.NextDouble() ?? 1d));
            if (profile.Kind == EnemyMovementKind.BlockedSidestep || profile.Kind == EnemyMovementKind.ArcPassPursuit)
            {
                var seededRandom = random ?? new System.Random(0);
                if (profile.Kind == EnemyMovementKind.BlockedSidestep)
                    _sidestepSide = seededRandom.Next(2) == 0 ? -1f : 1f;
                else
                {
                    _arcSide = seededRandom.Next(2) == 0 ? -1f : 1f;
                    _arcCycleTime = (float)seededRandom.NextDouble() * profile.CycleSeconds;
                }
            }
            if (profile.Kind == EnemyMovementKind.InertialPursuit)
                _inertialTurnSide = (random ?? new System.Random(0)).Next(2) == 0 ? -1f : 1f;
        }

        private void SelectOffsetDirection()
        {
            var angle = (float)(_offsetRandom.NextDouble() * Math.PI * 2d);
            _offsetDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
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
            {
                _hasLastSample = false;
                return new EnemyMovementFrame(Vector2.zero, Phase, _dashDirection);
            }

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
                case EnemyMovementKind.OffsetPursuit:
                    return CalculateOffsetPursuit(currentPosition, targetPosition, toward, movementSpeed, deltaTime);
                case EnemyMovementKind.CommittedPursuit:
                    return CalculateCommittedPursuit(toward, movementSpeed, deltaTime);
                case EnemyMovementKind.BlockedSidestep:
                    return CalculateBlockedSidestep(currentPosition, toward, distance, movementSpeed, deltaTime);
                case EnemyMovementKind.ArcPassPursuit:
                    return CalculateArcPass(toward, distance, movementSpeed, deltaTime);
                case EnemyMovementKind.InertialPursuit:
                    return CalculateInertialPursuit(toward, movementSpeed, deltaTime);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private EnemyMovementFrame CalculateOffsetPursuit(
            Vector2 currentPosition,
            Vector2 targetPosition,
            Vector2 toward,
            float movementSpeed,
            float deltaTime)
        {
            _offsetCycleTime += deltaTime;
            if (_offsetCycleTime >= _profile.CycleSeconds)
            {
                _offsetCycleTime %= _profile.CycleSeconds;
                SelectOffsetDirection();
            }
            if (_offsetCycleTime >= _profile.CycleSeconds - _profile.DirectPursuitSeconds)
                return Frame(toward * movementSpeed, EnemyMovementPhase.Seeking);

            var offsetTarget = targetPosition + _offsetDirection * _profile.PreferredDistance;
            var delta = offsetTarget - currentPosition;
            var distance = delta.magnitude;
            if (distance <= Mathf.Epsilon)
                return Frame(Vector2.zero, EnemyMovementPhase.OffsetPursuit);
            var arrival = Mathf.Clamp01(distance / _profile.DistanceTolerance);
            return Frame(delta / distance * (movementSpeed * arrival), EnemyMovementPhase.OffsetPursuit);
        }

        private EnemyMovementFrame CalculateCommittedPursuit(Vector2 toward, float movementSpeed, float deltaTime)
        {
            if (_committedDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                if (toward.sqrMagnitude > Mathf.Epsilon) _committedDirection = toward;
                if (_courseRemaining <= 0f) _courseRemaining = _profile.CycleSeconds;
            }
            else if (_courseRemaining <= 0f)
            {
                if (toward.sqrMagnitude > Mathf.Epsilon) _committedDirection = toward;
                _courseRemaining = _profile.CycleSeconds;
            }
            _courseRemaining -= deltaTime;
            return Frame(_committedDirection * movementSpeed, EnemyMovementPhase.CommittedPursuit);
        }

        // Compare last frame's commanded motion with displacement already produced by physics.
        // No neighbour search or extra physics query is needed.
        private EnemyMovementFrame CalculateBlockedSidestep(
            Vector2 currentPosition, Vector2 toward, float distance, float movementSpeed, float deltaTime)
        {
            _sidestepCooldown = Mathf.Max(0f, _sidestepCooldown - deltaTime);
            if (distance <= _profile.PreferredDistance)
            {
                _blockedTime = 0f;
                _sidestepNearTime = 0f;
                _sidestepRemaining = 0f;
            }
            else if (_sidestepRemaining > 0f)
            {
                _sidestepRemaining -= deltaTime;
                if (_sidestepRemaining <= 0f)
                {
                    _sidestepCooldown = _profile.SidestepCooldownSeconds;
                    _sidestepSide = -_sidestepSide;
                }
            }
            else if (_sidestepCooldown <= 0f && movementSpeed > Mathf.Epsilon)
            {
                _sidestepNearTime = distance <= _profile.SidestepNearDistance
                    ? _sidestepNearTime + deltaTime : 0f;
                if (_hasLastSample && deltaTime > 0f && _lastDesiredVelocity.sqrMagnitude > Mathf.Epsilon)
                {
                    var desiredSquare = _lastDesiredVelocity.sqrMagnitude;
                    var progressFraction = Vector2.Dot(currentPosition - _lastSamplePosition, _lastDesiredVelocity) /
                                           (desiredSquare * deltaTime);
                    _blockedTime = progressFraction < _profile.BlockedProgressFraction
                        ? _blockedTime + deltaTime : 0f;
                }
                else _blockedTime = 0f;
                if (_blockedTime >= _profile.BlockedTriggerSeconds ||
                    _sidestepNearTime >= _profile.SidestepNearSeconds)
                {
                    _sidestepRemaining = _profile.SidestepSeconds;
                    _blockedTime = 0f;
                    _sidestepNearTime = 0f;
                }
            }
            else
            {
                _blockedTime = 0f;
                _sidestepNearTime = 0f;
            }

            var direction = toward;
            var phase = EnemyMovementPhase.Seeking;
            if (_sidestepRemaining > 0f && toward.sqrMagnitude > Mathf.Epsilon)
            {
                var tangent = new Vector2(-toward.y, toward.x) * (_profile.LateralStrength * _sidestepSide);
                direction = (toward + tangent).normalized;
                phase = EnemyMovementPhase.Sidestepping;
            }
            var frame = Frame(direction * movementSpeed, phase);
            _lastSamplePosition = currentPosition;
            _lastDesiredVelocity = frame.Velocity;
            _hasLastSample = true;
            return frame;
        }

        private EnemyMovementFrame CalculateArcPass(Vector2 toward, float distance, float movementSpeed, float deltaTime)
        {
            _arcCycleTime = Mathf.Repeat(_arcCycleTime + deltaTime, _profile.CycleSeconds);
            if (distance > _profile.PreferredDistance ||
                _arcCycleTime >= _profile.CycleSeconds - _profile.DirectPursuitSeconds)
                return Frame(toward * movementSpeed, EnemyMovementPhase.Seeking);

            var tangent = new Vector2(-toward.y, toward.x) * (_profile.LateralStrength * _arcSide);
            var combined = toward + tangent;
            return Frame(combined.sqrMagnitude <= Mathf.Epsilon ? Vector2.zero : combined.normalized * movementSpeed,
                EnemyMovementPhase.ArcPassing);
        }

        private EnemyMovementFrame CalculateInertialPursuit(Vector2 toward, float movementSpeed, float deltaTime)
        {
            if (_inertialDirection.sqrMagnitude <= Mathf.Epsilon)
                _inertialDirection = toward;
            else if (toward.sqrMagnitude > Mathf.Epsilon)
            {
                var cross = _inertialDirection.x * toward.y - _inertialDirection.y * toward.x;
                var dot = Vector2.Dot(_inertialDirection, toward);
                var turnStep = Mathf.Clamp01(deltaTime / _profile.TurnResponseSeconds);
                if (dot >= 0f && Mathf.Abs(cross) <= turnStep)
                    _inertialDirection = toward;
                else
                {
                    var side = Mathf.Abs(cross) > Mathf.Epsilon ? Mathf.Sign(cross) : _inertialTurnSide;
                    var tangent = new Vector2(-_inertialDirection.y, _inertialDirection.x) * side;
                    _inertialDirection = (_inertialDirection + tangent * turnStep).normalized;
                }
            }
            return Frame(_inertialDirection * movementSpeed, EnemyMovementPhase.InertialPursuit);
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
                if (--_dashesLeftInSequence > 0)
                {
                    // Follow-up dash: fresh direction snapshot, shorter telegraph, no pursuit in between (MIDBOSS-001).
                    _dashDirection = toward.sqrMagnitude > Mathf.Epsilon ? toward : _dashDirection;
                    _dashPhase = EnemyMovementPhase.TelegraphingDash;
                    _dashPhaseRemaining = _profile.FollowUpTelegraphSeconds;
                    if (_dashPhaseRemaining > 0f)
                        return Frame(Vector2.zero, _dashPhase, _dashDirection);
                    _dashPhase = EnemyMovementPhase.Dashing;
                    _dashPhaseRemaining = _profile.DashDurationSeconds;
                    return Frame(_dashDirection * movementSpeed * _profile.DashSpeedMultiplier, _dashPhase, _dashDirection);
                }
                _dashPhase = EnemyMovementPhase.Seeking;
                _dashCooldownRemaining = _profile.DashCooldownSeconds;
            }

            _dashCooldownRemaining -= deltaTime;
            if (_dashCooldownRemaining <= 0f)
            {
                _dashesLeftInSequence = _profile.DashCount;
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
