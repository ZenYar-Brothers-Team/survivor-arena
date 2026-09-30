using System;
using System.Collections.Generic;
using Game.Diagnostics;
using Game.Enemy;
using UnityEngine;
namespace Game.Traveler
{
    public sealed class TravelerMovementDriver : IEnemyMovementDriver
    {
        private const int GroupCandidateCount = 4;
        private const float RetargetSeconds = 10f;
        private readonly TravelerDefinition _definition;
        private readonly TravelerPlacement _placement;
        private readonly Guid _runId;
        private readonly System.Random _random;
        private readonly List<EnemyRuntime> _ordinary = new List<EnemyRuntime>();
        private readonly List<EnemyRuntime> _groupCandidates = new List<EnemyRuntime>(GroupCandidateCount);
        private EnemyRuntime _groupTarget;
        private Vector2 _direction;
        private float _remaining, _avoidRemaining, _retargetRemaining;
        private bool _resting;
        private float _zigzagRemaining, _dashRemaining, _dashCooldown, _teleportRemaining;
        private float _zigzagSign = 1f, _orbitSign = 1f;
        private Vector2 _dashDirection, _teleportTarget;
        private bool _teleportPending;
        public EnemyMovementPhase Phase { get; private set; }
        public TravelerMovementDriver(TravelerDefinition definition, TravelerPlacement placement, Guid runId, int seed)
        {
            _definition = definition; _placement = placement; _runId = runId; _random = new System.Random(seed);
            if (definition.MovementStyle == TravelerMovementStyle.Orbit)
            {
                _orbitSign = _random.Next(2) == 0 ? -1f : 1f;
                _teleportRemaining = NextTeleportDelay();
            }
        }
        /// <summary>DECISION-0120: the orbiting Traveler asked to jump; the encounter moves the body and plays the effect.</summary>
        public bool TryTakeTeleport(out Vector2 target)
        { target = _teleportTarget; var pending = _teleportPending; _teleportPending = false; return pending; }
        private float NextTeleportDelay() =>
            Mathf.Lerp(_definition.TeleportMinSeconds, _definition.TeleportMaxSeconds, (float)_random.NextDouble());
        public EnemyMovementFrame Tick(Vector2 position, Vector2 player, float speed, float deltaTime, bool running)
        {
            if (!running) return new EnemyMovementFrame(Vector2.zero, Phase);
            using var guard = PerfGuard.Measure("Traveler.Movement", 3f);
            Vector2 destination;
            if (_definition.Role == TravelerRole.Protector && TryGroup(position, player, deltaTime, out destination))
                Phase = EnemyMovementPhase.Seeking;
            else if (_definition.MovementStyle == TravelerMovementStyle.Orbit)
                destination = OrbitDestination(position, player, deltaTime);
            else
            {
                _remaining -= deltaTime;
                if (_remaining <= 0)
                {
                    _resting = !_resting && _definition.RestSeconds > 0;
                    _remaining = _resting ? _definition.RestSeconds : _definition.WanderSeconds;
                    var angle = (float)_random.NextDouble() * Mathf.PI * 2;
                    _direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                }
                _dashCooldown = Mathf.Max(0, _dashCooldown - deltaTime);
                var closeToPlayer = (position - player).sqrMagnitude < _definition.AvoidRadius * _definition.AvoidRadius;
                if (closeToPlayer) { _avoidRemaining = _definition.AvoidSeconds; _direction = (position - player).normalized; }
                if (closeToPlayer && _dashRemaining <= 0 && _dashCooldown <= 0 && _definition.MovementStyle == TravelerMovementStyle.DashEscape)
                    StartEscapeDash(position - player);
                _avoidRemaining = Mathf.Max(0, _avoidRemaining - deltaTime);
                if (_dashRemaining > 0)
                {
                    _dashRemaining -= deltaTime;
                    Phase = EnemyMovementPhase.Retreating;
                    var dashStep = _dashDirection * (_definition.EscapeDashDistance / _definition.EscapeDashSeconds) * deltaTime;
                    return Finish(position, position + dashStep, dashStep.magnitude, deltaTime);
                }
                if (_avoidRemaining > 0 && _definition.MovementStyle == TravelerMovementStyle.ZigzagEscape)
                {
                    _zigzagRemaining -= deltaTime;
                    if (_zigzagRemaining <= 0) { _zigzagSign = -_zigzagSign; _zigzagRemaining = _definition.ZigzagSeconds; }
                    var away = position == player ? _direction : (position - player).normalized;
                    _direction = Rotate(away, _zigzagSign * _definition.ZigzagAngleDegrees);
                    Phase = EnemyMovementPhase.Zigzagging;
                }
                else Phase = _avoidRemaining > 0 ? EnemyMovementPhase.Retreating : EnemyMovementPhase.HoldingDistance;
                destination = position + (_resting && _avoidRemaining == 0 ? Vector2.zero : _direction) * speed * deltaTime;
            }
            return Finish(position, destination, speed * deltaTime, deltaTime);
        }
        private EnemyMovementFrame Finish(Vector2 position, Vector2 destination, float maxStep, float deltaTime)
        {
            var desired = Vector2.MoveTowards(position, destination, maxStep);
            desired = _placement.ClampToBounds(desired);
            if ((desired - position).sqrMagnitude < .000001f) _remaining = 0;
            return new EnemyMovementFrame(deltaTime > 0 ? (desired - position) / deltaTime : Vector2.zero, Phase);
        }
        private void StartEscapeDash(Vector2 away)
        {
            // Dash away with a slight random swing so the escape is not a straight line.
            var swing = ((float)_random.NextDouble() * 2f - 1f) * 30f;
            _dashDirection = Rotate(away.sqrMagnitude < 1e-6f ? _direction : away.normalized, swing);
            _dashRemaining = _definition.EscapeDashSeconds;
            _dashCooldown = _definition.EscapeDashCooldownSeconds;
        }

        private Vector2 OrbitDestination(Vector2 position, Vector2 player, float deltaTime)
        {
            Phase = EnemyMovementPhase.Orbiting;
            var radiusX = _definition.OrbitRadiusX;
            var radiusY = _definition.OrbitRadiusY;
            var relative = position - player;
            if (relative.sqrMagnitude < 1e-6f) relative = Vector2.right;
            // Angle on the ellipse (x/rx, y/ry); the goal is always a fixed lead ahead, so the body spirals onto the oval.
            var angle = Mathf.Atan2(relative.y / radiusY, relative.x / radiusX);
            _teleportRemaining -= deltaTime;
            if (_teleportRemaining <= 0f)
            {
                _teleportRemaining = NextTeleportDelay();
                TrySelectTeleport(player, angle);
            }
            var ahead = angle + _orbitSign * _definition.OrbitLeadDegrees * Mathf.Deg2Rad;
            return player + new Vector2(Mathf.Cos(ahead) * radiusX, Mathf.Sin(ahead) * radiusY);
        }

        // Jump to the opposite point of the oval or a quarter turn either way; points outside the arena or inside obstacles are skipped.
        private void TrySelectTeleport(Vector2 player, float angle)
        {
            var first = _random.Next(3);
            for (var i = 0; i < 3; i++)
            {
                var turn = (first + i) % 3 == 0 ? Mathf.PI : ((first + i) % 3 == 1 ? Mathf.PI / 2 : -Mathf.PI / 2);
                var target = player + new Vector2(Mathf.Cos(angle + turn) * _definition.OrbitRadiusX, Mathf.Sin(angle + turn) * _definition.OrbitRadiusY);
                if (!_placement.Contains(target)) continue;
                _teleportTarget = target; _teleportPending = true; return;
            }
        }

        private static Vector2 Rotate(Vector2 vector, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(radians); var sin = Mathf.Sin(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
        }

        private bool TryGroup(Vector2 position, Vector2 player, float deltaTime, out Vector2 destination)
        {
            _retargetRemaining -= deltaTime;
            if (_retargetRemaining <= 0f || !ValidOrdinary(_groupTarget))
            {
                SelectGroupTarget(position, player);
                _retargetRemaining = RetargetSeconds;
            }
            destination = _groupTarget == null
                ? position
                : _groupTarget.Position + (player - _groupTarget.Position).normalized * _definition.GuardOffset;
            return _groupTarget != null;
        }

        private void SelectGroupTarget(Vector2 position, Vector2 player)
        {
            EnemyRegistry.CopyAliveTo(_ordinary);
            _groupCandidates.Clear();
            foreach (var candidate in _ordinary)
            {
                if (!ValidOrdinary(candidate)) continue;
                var insertAt = 0;
                while (insertAt < _groupCandidates.Count && CompareDistance(_groupCandidates[insertAt], candidate, position) <= 0)
                    insertAt++;
                if (insertAt >= GroupCandidateCount) continue;
                _groupCandidates.Insert(insertAt, candidate);
                if (_groupCandidates.Count > GroupCandidateCount) _groupCandidates.RemoveAt(GroupCandidateCount);
            }

            _groupTarget = null;
            var bestCount = -1;
            var bestPlayerDistance = float.PositiveInfinity;
            var radiusSquared = _definition.SupportRadius * _definition.SupportRadius;
            foreach (var candidate in _groupCandidates)
            {
                var count = 0;
                foreach (var other in _ordinary)
                    if (ValidOrdinary(other) && (other.Position - candidate.Position).sqrMagnitude <= radiusSquared) count++;
                var playerDistance = (candidate.Position - player).sqrMagnitude;
                if (count < bestCount || (count == bestCount && playerDistance > bestPlayerDistance) ||
                    (count == bestCount && playerDistance == bestPlayerDistance && _groupTarget != null &&
                     candidate.LifeId.CompareTo(_groupTarget.LifeId) >= 0)) continue;
                _groupTarget = candidate;
                bestCount = count;
                bestPlayerDistance = playerDistance;
            }
        }

        private bool ValidOrdinary(EnemyRuntime enemy) => enemy != null && enemy.IsAlive &&
            enemy.Category == EnemyCategory.Ordinary && enemy.Identity.RunId == _runId;

        private static int CompareDistance(EnemyRuntime left, EnemyRuntime right, Vector2 position)
        {
            var comparison = ((left.Position - position).sqrMagnitude).CompareTo((right.Position - position).sqrMagnitude);
            return comparison != 0 ? comparison : left.LifeId.CompareTo(right.LifeId);
        }
    }
}
