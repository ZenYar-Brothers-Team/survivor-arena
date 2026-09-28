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
        public EnemyMovementPhase Phase { get; private set; }
        public TravelerMovementDriver(TravelerDefinition definition, TravelerPlacement placement, Guid runId, int seed)
        { _definition = definition; _placement = placement; _runId = runId; _random = new System.Random(seed); }
        public EnemyMovementFrame Tick(Vector2 position, Vector2 player, float speed, float deltaTime, bool running)
        {
            if (!running) return new EnemyMovementFrame(Vector2.zero, Phase);
            using var guard = PerfGuard.Measure("Traveler.Movement", 3f);
            Vector2 destination;
            if (_definition.Role == TravelerRole.Protector && TryGroup(position, player, deltaTime, out destination))
                Phase = EnemyMovementPhase.Seeking;
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
                if ((position - player).sqrMagnitude < _definition.AvoidRadius * _definition.AvoidRadius)
                { _avoidRemaining = _definition.AvoidSeconds; _direction = (position - player).normalized; }
                _avoidRemaining = Mathf.Max(0, _avoidRemaining - deltaTime);
                Phase = _avoidRemaining > 0 ? EnemyMovementPhase.Retreating : EnemyMovementPhase.HoldingDistance;
                destination = position + (_resting && _avoidRemaining == 0 ? Vector2.zero : _direction) * speed * deltaTime;
            }
            var desired = Vector2.MoveTowards(position, destination, speed * deltaTime);
            desired = _placement.ClampToBounds(desired);
            if ((desired - position).sqrMagnitude < .000001f) _remaining = 0;
            return new EnemyMovementFrame(deltaTime > 0 ? (desired - position) / deltaTime : Vector2.zero, Phase);
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
