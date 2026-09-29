using System;
using Game.Diagnostics;
using UnityEngine;

namespace Game.Automation
{
    /// <summary>Bounded visible-state steering heuristic, independent of gameplay RNG (IP-34 AB-02).</summary>
    public sealed class BotMovementPolicy
    {
        private static readonly Vector2[] Directions =
        {
            Vector2.zero, Vector2.right, Vector2.up, Vector2.left, Vector2.down,
            new Vector2(0.70710678f, 0.70710678f), new Vector2(-0.70710678f, 0.70710678f),
            new Vector2(-0.70710678f, -0.70710678f), new Vector2(0.70710678f, -0.70710678f)
        };

        private readonly float _predictionSeconds;
        private readonly float _obstaclePadding;
        private readonly float _stuckSeconds;
        private Vector2 _previousDirection;
        private Vector2 _lastPosition;
        private bool _hasLastPosition;
        private float _stationarySeconds;
        private int _recoveryAttempts;

        public BotMovementPolicy(MovementPolicyData settings)
        {
            if (settings == null || settings.PredictionSeconds == null || settings.ObstaclePadding == null ||
                settings.StuckSeconds == null) throw new ArgumentException("Validated movement policy required.", nameof(settings));
            _predictionSeconds = settings.PredictionSeconds.Value;
            _obstaclePadding = settings.ObstaclePadding.Value;
            _stuckSeconds = settings.StuckSeconds.Value;
        }

        public BotMovementDecision Decide(BotObservation observation, float elapsedSimulationSeconds)
        {
            using var guard = PerfGuard.Measure("BotMovementPolicy.Decide", 3f);
            if (observation == null) throw new ArgumentNullException(nameof(observation));
            if (elapsedSimulationSeconds < 0 || float.IsNaN(elapsedSimulationSeconds))
                throw new ArgumentOutOfRangeException(nameof(elapsedSimulationSeconds));
            if (!observation.CoverageComplete)
            {
                _previousDirection = Vector2.zero;
                return new BotMovementDecision(Vector2.zero, coverageIncomplete: true);
            }

            if (_hasLastPosition && (_previousDirection != Vector2.zero || _recoveryAttempts > 0) &&
                (observation.Position - _lastPosition).sqrMagnitude < 0.0025f)
                _stationarySeconds += elapsedSimulationSeconds;
            else
            {
                _stationarySeconds = 0f;
                _recoveryAttempts = 0;
            }
            _lastPosition = observation.Position;
            _hasLastPosition = true;

            var best = 0;
            var bestScore = float.NegativeInfinity;
            for (var i = 0; i < Directions.Length; i++)
            {
                var direction = Directions[i];
                if (_stationarySeconds >= _stuckSeconds && (direction == _previousDirection ||
                    _recoveryAttempts > 0 && direction == Vector2.zero)) continue;
                if (!IsReachable(observation, direction)) continue;
                var score = Score(observation, direction);
                if (direction == _previousDirection) score += 0.1f;
                if (score > bestScore) { bestScore = score; best = i; }
            }
            if (_stationarySeconds >= _stuckSeconds)
            {
                _recoveryAttempts++;
                _stationarySeconds = 0f;
            }
            _previousDirection = Directions[best];
            return new BotMovementDecision(_previousDirection, stuck: _recoveryAttempts >= 8);
        }

        private bool IsReachable(BotObservation observation, Vector2 direction)
        {
            var step = direction * observation.MovementSpeed * _predictionSeconds;
            var destination = observation.Position + step;
            var padding = observation.PlayerRadius + _obstaclePadding;
            if (destination.x < observation.ArenaBounds.xMin + padding || destination.x > observation.ArenaBounds.xMax - padding ||
                destination.y < observation.ArenaBounds.yMin + padding || destination.y > observation.ArenaBounds.yMax - padding)
                return false;
            if (direction == Vector2.zero) return true;
            foreach (var obstacle in observation.Obstacles)
            {
                var bounds = obstacle.Bounds;
                bounds.xMin -= padding; bounds.xMax += padding;
                bounds.yMin -= padding; bounds.yMax += padding;
                if (SegmentIntersectsRect(observation.Position, destination, bounds)) return false;
            }
            return true;
        }

        private float Score(BotObservation observation, Vector2 direction)
        {
            var destination = observation.Position + direction * observation.MovementSpeed * _predictionSeconds;
            var danger = 0f;
            foreach (var threat in observation.Threats)
            {
                var predicted = threat.Position + threat.Velocity * _predictionSeconds;
                var distance = Vector2.Distance(destination, predicted);
                var margin = observation.PlayerRadius + threat.Radius + 2f;
                var exposure = Mathf.Max(0f, margin - distance) / margin;
                danger += threat.Weight * exposure * exposure;
            }
            foreach (var beam in observation.Beams)
            {
                var distance = DistanceToSegment(destination, beam.Start, beam.End);
                var margin = observation.PlayerRadius + beam.HalfWidth + 2f;
                var exposure = Mathf.Max(0f, margin - distance) / margin;
                danger += beam.Weight * exposure * exposure;
            }
            var attraction = 0f;
            foreach (var pickup in observation.Pickups)
            {
                var target = GuidanceTarget(observation, pickup.Position);
                var oldDistance = Vector2.Distance(observation.Position, target);
                var newDistance = Vector2.Distance(destination, target);
                attraction += pickup.Priority * (oldDistance - newDistance) / Mathf.Max(1f, oldDistance);
            }
            return attraction - 10f * danger;
        }

        private Vector2 GuidanceTarget(BotObservation observation, Vector2 pickup)
        {
            foreach (var obstacle in observation.Obstacles)
            {
                var bounds = obstacle.Bounds;
                var padding = observation.PlayerRadius + _obstaclePadding + 0.1f;
                bounds.xMin -= padding; bounds.xMax += padding;
                bounds.yMin -= padding; bounds.yMax += padding;
                if (!SegmentIntersectsRect(observation.Position, pickup, bounds)) continue;
                var corners = new[]
                {
                    new Vector2(bounds.xMin, bounds.yMin), new Vector2(bounds.xMin, bounds.yMax),
                    new Vector2(bounds.xMax, bounds.yMin), new Vector2(bounds.xMax, bounds.yMax)
                };
                var best = pickup;
                var bestLength = float.PositiveInfinity;
                foreach (var corner in corners)
                {
                    if (!observation.ArenaBounds.Contains(corner)) continue;
                    // The first waypoint must be on the player's near side; a far-side
                    // corner would invite movement straight into the blocking collider.
                    var length = Vector2.Distance(observation.Position, corner);
                    if (length < bestLength) { bestLength = length; best = corner; }
                }
                return best;
            }
            return pickup;
        }

        private static bool SegmentIntersectsRect(Vector2 start, Vector2 end, Rect rect)
        {
            var delta = end - start;
            var minimum = 0f;
            var maximum = 1f;
            for (var axis = 0; axis < 2; axis++)
            {
                var origin = axis == 0 ? start.x : start.y;
                var change = axis == 0 ? delta.x : delta.y;
                var low = axis == 0 ? rect.xMin : rect.yMin;
                var high = axis == 0 ? rect.xMax : rect.yMax;
                if (Mathf.Abs(change) < 0.00001f)
                {
                    if (origin < low || origin > high) return false;
                    continue;
                }
                var near = (low - origin) / change;
                var far = (high - origin) / change;
                if (near > far) { var swap = near; near = far; far = swap; }
                minimum = Mathf.Max(minimum, near);
                maximum = Mathf.Min(maximum, far);
                if (minimum > maximum) return false;
            }
            return true;
        }

        private static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            var segment = end - start;
            var lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= 0.00001f) return Vector2.Distance(point, start);
            var progress = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared);
            return Vector2.Distance(point, start + segment * progress);
        }
    }
}
