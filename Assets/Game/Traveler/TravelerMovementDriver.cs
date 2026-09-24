using System;
using System.Collections.Generic;
using Game.Diagnostics;
using Game.Enemy;
using UnityEngine;
namespace Game.Traveler
{
    public sealed class TravelerMovementDriver : IEnemyMovementDriver
    {
        private readonly TravelerDefinition _definition;
        private readonly TravelerPlacement _placement;
        private readonly Guid _runId;
        private readonly System.Random _random;
        private readonly List<EnemyRuntime> _ordinary = new List<EnemyRuntime>();
        private Vector2 _direction;
        private Vector2 _lastReachable;
        private bool _hasReachable;
        private float _remaining, _avoidRemaining;
        private bool _resting;
        public EnemyMovementPhase Phase { get; private set; }
        public TravelerMovementDriver(TravelerDefinition definition, TravelerPlacement placement, Guid runId, int seed)
        { _definition = definition; _placement = placement; _runId = runId; _random = new System.Random(seed); }
        public EnemyMovementFrame Tick(Vector2 position, Vector2 player, float speed, float deltaTime, bool running)
        {
            if (!running) return new EnemyMovementFrame(Vector2.zero, Phase);
            using var guard = PerfGuard.Measure("Traveler.Movement", 3f);
            Vector2 destination;
            if (_definition.Role == TravelerRole.Protector && TryGroup(position, player, out destination))
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
            desired = _hasReachable ? _placement.ProjectFrom(_lastReachable, desired) : _placement.Project(desired);
            _lastReachable = desired;
            _hasReachable = true;
            if ((desired - position).sqrMagnitude < .000001f) _remaining = 0;
            return new EnemyMovementFrame(deltaTime > 0 ? (desired - position) / deltaTime : Vector2.zero, Phase);
        }
        private bool TryGroup(Vector2 position, Vector2 player, out Vector2 destination)
        {
            EnemyRegistry.CopyAliveTo(_ordinary);
            _ordinary.RemoveAll(enemy => enemy.Category != EnemyCategory.Ordinary || enemy.Identity.RunId != _runId);
            _ordinary.Sort((a, b) => a.LifeId.CompareTo(b.LifeId));
            var bestCount = 0; var bestDistance = float.PositiveInfinity; destination = position;
            foreach (var candidate in _ordinary)
            {
                var count = 0;
                foreach (var other in _ordinary) if ((other.Position - candidate.Position).sqrMagnitude <= _definition.SupportRadius * _definition.SupportRadius) count++;
                var distance = (candidate.Position - position).sqrMagnitude;
                if (count < bestCount || (count == bestCount && distance >= bestDistance)) continue;
                bestCount = count; bestDistance = distance;
                destination = candidate.Position + (player - candidate.Position).normalized * _definition.GuardOffset;
            }
            return bestCount > 0;
        }
    }
}
