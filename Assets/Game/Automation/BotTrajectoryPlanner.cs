using System;
using System.Collections.Generic;
using System.Diagnostics;
using Game.Diagnostics;
using UnityEngine;

namespace Game.Automation
{
    /// <summary>
    /// AB-13 bounded shooting search: simulate whole waypoint trajectories against responsive enemies,
    /// keep the best route as a warm start and execute only its first direction.
    /// </summary>
    internal sealed class BotTrajectoryPlanner
    {
        private readonly float _horizon;
        private readonly float _step;
        private readonly int _candidateCount;
        private readonly float _contactPenalty;
        private readonly float _clearance;
        private readonly float _obstaclePadding;
        private readonly System.Random _random = new System.Random(13013);
        private readonly List<Vector2> _targets = new List<Vector2>(4);
        private readonly Vector2[] _scratchPath;
        private readonly Vector2[] _bestPath;
        private Vector2[] _threatPositions = Array.Empty<Vector2>();
        private bool[] _collected = Array.Empty<bool>();
        private BotTrajectoryRoute _warmRoute;
        private bool _hasWarmRoute;
        private BotTrajectoryRoute _bestRoute;
        private Vector2 _bestDirection;
        private Vector2 _previousDirection;
        private float _bestScore;
        private float _bestXp;
        private float _bestRisk;
        private int _bestPathCount;
        private int _evaluations;

        public BotTrajectoryPlanner(MovementPolicyData settings)
        {
            var data = settings.Trajectory ?? throw new ArgumentException("Trajectory settings required.");
            data.Validate();
            _horizon = data.HorizonSeconds.Value;
            _step = data.StepSeconds.Value;
            _candidateCount = data.CandidateCount.Value;
            _contactPenalty = data.ContactPenalty.Value;
            _clearance = data.Clearance.Value;
            _obstaclePadding = settings.ObstaclePadding.Value;
            _scratchPath = new Vector2[Mathf.CeilToInt(_horizon / _step) + 1];
            _bestPath = new Vector2[_scratchPath.Length];
        }

        public void Clear()
        {
            _hasWarmRoute = false;
            _previousDirection = Vector2.zero;
        }

        public BotTrajectoryPlan Plan(BotObservation observation)
        {
            using var guard = PerfGuard.Measure("BotTrajectoryPlanner.Plan", 25f);
            var timer = Stopwatch.StartNew();
            if (_threatPositions.Length < observation.Threats.Count)
                _threatPositions = new Vector2[observation.Threats.Count];
            if (_collected.Length < observation.Pickups.Count) _collected = new bool[observation.Pickups.Count];
            SelectTargets(observation);
            _bestScore = float.NegativeInfinity;
            _bestPathCount = 1;
            _bestPath[0] = observation.Position;
            _bestDirection = Vector2.zero;
            _bestRoute = new BotTrajectoryRoute(observation.Position, observation.Position, observation.Position, 2, false);
            _bestXp = _bestRisk = 0f;
            _evaluations = 0;
            // Staying is evaluated with the same moving enemies as every other candidate.
            Consider(new BotTrajectoryRoute(observation.Position, observation.Position,
                observation.Position, 2, false), observation);
            if (_hasWarmRoute && (!_warmRoute.HasXpTarget || IsVisibleXp(observation, _warmRoute.Target)))
            {
                AdvanceStage(ref _warmRoute, observation.Position);
                Consider(_warmRoute, observation);
            }
            foreach (var target in _targets)
            {
                var exit = Clamp(observation, target + (target - observation.Position).normalized *
                    observation.MovementSpeed * _horizon);
                Consider(new BotTrajectoryRoute(target, target, target, 2, true, exit), observation);
            }
            // Escape trajectories also give meaningful actions when no XP is visible.
            for (var i = 0; i < 8 && _evaluations < _candidateCount; i++)
            {
                var angle = i * Mathf.PI / 4f;
                var target = Clamp(observation, observation.Position +
                    new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * observation.MovementSpeed * _horizon * 0.5f);
                Consider(new BotTrajectoryRoute(target, target, target, 2, false), observation);
            }
            var sample = 0;
            while (_evaluations < _candidateCount)
            {
                var hasXp = _targets.Count > 0;
                var target = hasXp ? _targets[(sample / 2) % _targets.Count] : observation.Position;
                var forward = (target - observation.Position).normalized;
                if (forward.sqrMagnitude < 0.01f)
                {
                    var angle = sample * 2.3999632f;
                    forward = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                }
                var side = new Vector2(-forward.y, forward.x) * (sample % 2 == 0 ? 1f : -1f);
                var reach = observation.MovementSpeed * _horizon;
                var lateral = reach * (0.04f + 0.24f * (float)_random.NextDouble());
                var along = (float)_random.NextDouble() * 1.2f - 0.4f;
                var first = Vector2.LerpUnclamped(observation.Position, target, along) + side * lateral;
                var second = Vector2.Lerp(observation.Position, target, 0.7f) + side * lateral;
                if (!hasXp)
                {
                    first = observation.Position + forward * lateral;
                    second = observation.Position + side * lateral;
                }
                var exitAngle = (float)_random.NextDouble() * Mathf.PI * 2f;
                var exit = Clamp(observation, target + new Vector2(Mathf.Cos(exitAngle), Mathf.Sin(exitAngle)) * reach);
                if (sample % 3 == 2 && _bestRoute.HasXpTarget && _bestRoute.Stage < 2)
                {
                    // Refine the current best, while the other samples keep exploring both sides.
                    var spread = reach * 0.08f;
                    first = _bestRoute.First + new Vector2(Noise(), Noise()) * spread;
                    second = _bestRoute.Second + new Vector2(Noise(), Noise()) * spread;
                    target = _bestRoute.Target;
                    exit = _bestRoute.Exit;
                }
                Consider(new BotTrajectoryRoute(Clamp(observation, first), Clamp(observation, second),
                    target, 0, hasXp, hasXp ? exit : target), observation);
                sample++;
            }
            _warmRoute = _bestRoute;
            _hasWarmRoute = !float.IsNegativeInfinity(_bestScore);
            _previousDirection = _bestDirection;
            var path = new Vector2[_bestPathCount];
            Array.Copy(_bestPath, path, path.Length);
            var linearEnemies = 0;
            foreach (var threat in observation.Threats)
                if (threat.IsEnemy && threat.Motion == BotThreatMotion.Linear) linearEnemies++;
            timer.Stop();
            return new BotTrajectoryPlan(_bestDirection, _bestRoute.HasXpTarget ? _bestRoute.Target : (Vector2?)null,
                path, _hasWarmRoute ? _bestScore : 0f, _bestXp, _bestRisk, _evaluations, linearEnemies,
                timer.Elapsed.TotalMilliseconds);
        }

        private void Consider(BotTrajectoryRoute route, BotObservation observation)
        {
            _evaluations++;
            var score = Evaluate(route, observation, out var xp, out var risk, out var pathCount, out var direction);
            score += Vector2.Dot(direction, _previousDirection) * 0.05f;
            if (score <= _bestScore) return;
            _bestScore = score;
            _bestRoute = route;
            _bestDirection = direction;
            _bestXp = xp;
            _bestRisk = risk;
            _bestPathCount = pathCount;
            Array.Copy(_scratchPath, _bestPath, pathCount);
        }

        private float Evaluate(BotTrajectoryRoute route, BotObservation observation, out float xp,
            out float risk, out int pathCount, out Vector2 direction)
        {
            for (var i = 0; i < observation.Threats.Count; i++) _threatPositions[i] = observation.Threats[i].Position;
            Array.Clear(_collected, 0, observation.Pickups.Count);
            xp = risk = 0f;
            var weightedXp = 0f;
            pathCount = 1;
            direction = Vector2.zero;
            var player = observation.Position;
            _scratchPath[0] = player;
            var originalTarget = route.Target;
            var reachedTarget = false;
            var elapsed = 0f;
            for (var stepIndex = 1; stepIndex < _scratchPath.Length; stepIndex++)
            {
                var dt = Mathf.Min(_step, _horizon - elapsed);
                if (dt <= 0f) break;
                AdvanceStage(ref route, player);
                var next = Vector2.MoveTowards(player, route.Waypoint, observation.MovementSpeed * dt);
                if (!Reachable(observation, player, next)) return float.NegativeInfinity;
                if (stepIndex == 1) direction = (next - player).normalized;
                var stepRisk = 0f;
                for (var i = 0; i < observation.Threats.Count; i++)
                {
                    var threat = observation.Threats[i];
                    var old = _threatPositions[i];
                    var predicted = BotThreatForecast.Advance(threat, old, player, dt);
                    var radius = observation.PlayerRadius + threat.CollisionRadius;
                    var distanceSquared = OriginSegmentDistanceSquared(old - player, predicted - next);
                    if (distanceSquared < (radius + _clearance) * (radius + _clearance))
                        stepRisk = Mathf.Max(stepRisk, Exposure(Mathf.Sqrt(distanceSquared), radius) * threat.Weight / 3f);
                    _threatPositions[i] = predicted;
                }
                foreach (var beam in observation.Beams)
                {
                    var distance = SegmentDistance(player, next, beam.Start, beam.End);
                    stepRisk = Mathf.Max(stepRisk,
                        Exposure(distance, observation.PlayerRadius + beam.HalfWidth) * beam.Weight / 3f);
                }
                risk += stepRisk * dt;
                elapsed += dt;
                for (var i = 0; i < observation.Pickups.Count; i++)
                {
                    var pickup = observation.Pickups[i];
                    if (_collected[i] || !pickup.IsExperience || elapsed > pickup.RemainingSeconds) continue;
                    if (BotMovementPolicy.DistanceToSegment(pickup.Position, player, next) > observation.PickupRadius) continue;
                    _collected[i] = true;
                    xp += pickup.Value;
                    weightedXp += pickup.Value * (1f - 0.25f * elapsed / _horizon);
                }
                player = next;
                _scratchPath[pathCount++] = player;
                if (route.HasXpTarget && Vector2.Distance(player, route.Target) <= observation.PickupRadius)
                    reachedTarget = true;
            }
            var targetProgress = reachedTarget ? 1f : route.HasXpTarget ? Mathf.Clamp(
                (Vector2.Distance(observation.Position, originalTarget) - Vector2.Distance(player, originalTarget)) /
                Mathf.Max(1f, Vector2.Distance(observation.Position, originalTarget)), -1f, 1f) : 0f;
            var exitRisk = BestExitRisk(observation, player);
            // Technical objective: collect XP along the whole route, avoid contact and leave an exit.
            var driftCost = _targets.Count == 0 ? Vector2.Distance(observation.Position, player) * 0.05f : 0f;
            return 5f * weightedXp + 1.5f * targetProgress - driftCost -
                _contactPenalty * Mathf.Lerp(2f, 1f, observation.HealthFraction) * (risk + exitRisk * 0.5f);
        }

        private float BestExitRisk(BotObservation observation, Vector2 player)
        {
            var best = float.PositiveInfinity;
            for (var direction = 0; direction < 8; direction++)
            {
                var angle = direction * Mathf.PI / 4f;
                var next = player + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * observation.MovementSpeed * 0.5f;
                if (!Reachable(observation, player, next)) continue;
                var risk = 0f;
                for (var i = 0; i < observation.Threats.Count; i++)
                {
                    var threat = observation.Threats[i];
                    var old = _threatPositions[i];
                    var predicted = BotThreatForecast.Advance(threat, old, player, 0.5f);
                    var radius = observation.PlayerRadius + threat.CollisionRadius;
                    var distanceSquared = OriginSegmentDistanceSquared(old - player, predicted - next);
                    if (distanceSquared < (radius + _clearance) * (radius + _clearance))
                        risk = Mathf.Max(risk, Exposure(Mathf.Sqrt(distanceSquared), radius) * threat.Weight / 3f);
                }
                foreach (var beam in observation.Beams)
                    risk = Mathf.Max(risk, Exposure(SegmentDistance(player, next, beam.Start, beam.End),
                        observation.PlayerRadius + beam.HalfWidth) * beam.Weight / 3f);
                best = Mathf.Min(best, risk);
            }
            return float.IsPositiveInfinity(best) ? 1f : best;
        }

        private float Exposure(float distance, float radius) =>
            Mathf.Clamp01((radius + _clearance - distance) / Mathf.Max(0.05f, _clearance));

        private void SelectTargets(BotObservation observation)
        {
            _targets.Clear();
            for (var slot = 0; slot < 4; slot++)
            {
                Vector2? best = null;
                var distance = float.PositiveInfinity;
                foreach (var pickup in observation.Pickups)
                {
                    if (!pickup.IsExperience || pickup.RemainingSeconds <= 0f) continue;
                    var represented = false;
                    foreach (var target in _targets)
                        if ((pickup.Position - target).sqrMagnitude < 1f) { represented = true; break; }
                    if (represented) continue;
                    var candidateDistance = (pickup.Position - observation.Position).sqrMagnitude;
                    if (candidateDistance >= distance) continue;
                    distance = candidateDistance;
                    best = pickup.Position;
                }
                if (!best.HasValue) break;
                _targets.Add(best.Value);
            }
        }

        private static bool IsVisibleXp(BotObservation observation, Vector2 target)
        {
            foreach (var pickup in observation.Pickups)
                if (pickup.IsExperience && pickup.RemainingSeconds > 0f &&
                    (pickup.Position - target).sqrMagnitude < 0.04f) return true;
            return false;
        }

        private static void AdvanceStage(ref BotTrajectoryRoute route, Vector2 player)
        {
            while (route.Stage < 3 && (player - route.Waypoint).sqrMagnitude < 0.04f) route.Stage++;
        }

        private bool Reachable(BotObservation observation, Vector2 start, Vector2 end)
        {
            var padding = observation.PlayerRadius + _obstaclePadding;
            var arena = observation.ArenaBounds;
            if (end.x < arena.xMin + padding || end.x > arena.xMax - padding ||
                end.y < arena.yMin + padding || end.y > arena.yMax - padding) return false;
            foreach (var obstacle in observation.Obstacles)
            {
                var bounds = obstacle.Bounds;
                bounds.xMin -= padding; bounds.xMax += padding;
                bounds.yMin -= padding; bounds.yMax += padding;
                if (BotMovementPolicy.SegmentIntersectsRect(start, end, bounds)) return false;
            }
            return true;
        }

        private Vector2 Clamp(BotObservation observation, Vector2 point)
        {
            var padding = observation.PlayerRadius + _obstaclePadding + 0.01f;
            return new Vector2(Mathf.Clamp(point.x, observation.ArenaBounds.xMin + padding, observation.ArenaBounds.xMax - padding),
                Mathf.Clamp(point.y, observation.ArenaBounds.yMin + padding, observation.ArenaBounds.yMax - padding));
        }

        private float Noise() => (float)_random.NextDouble() * 2f - 1f;

        private static float OriginSegmentDistanceSquared(Vector2 start, Vector2 end)
        {
            var dx = end.x - start.x;
            var dy = end.y - start.y;
            var lengthSquared = dx * dx + dy * dy;
            var t = lengthSquared > 0.00001f ? Mathf.Clamp01(-(start.x * dx + start.y * dy) / lengthSquared) : 0f;
            var x = start.x + dx * t;
            var y = start.y + dy * t;
            return x * x + y * y;
        }

        private static float SegmentDistance(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            var ab = b - a;
            var cd = d - c;
            var denominator = ab.x * cd.y - ab.y * cd.x;
            if (Mathf.Abs(denominator) > 0.00001f)
            {
                var offset = c - a;
                var t = (offset.x * cd.y - offset.y * cd.x) / denominator;
                var u = (offset.x * ab.y - offset.y * ab.x) / denominator;
                if (t >= 0f && t <= 1f && u >= 0f && u <= 1f) return 0f;
            }
            return Mathf.Min(Mathf.Min(BotMovementPolicy.DistanceToSegment(a, c, d),
                    BotMovementPolicy.DistanceToSegment(b, c, d)),
                Mathf.Min(BotMovementPolicy.DistanceToSegment(c, a, b), BotMovementPolicy.DistanceToSegment(d, a, b)));
        }
    }
}
