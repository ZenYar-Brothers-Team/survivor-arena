using UnityEngine;

namespace Game.Automation
{
    /// <summary>Persistent, visible-state waypoint for the wide XP detour (IP-34 AB-10).</summary>
    internal sealed class BotOrbitPlanner
    {
        private readonly float _arcOffset;
        private readonly float _predictionSeconds;
        private readonly float _obstaclePadding;
        private readonly Vector2[] _candidatePositions = new Vector2[6];
        private readonly float[] _candidateDistances = new float[6];
        private Vector2 _target;
        private Vector2 _waypoint;
        private bool _hasTarget;
        private bool _hasWaypoint;

        public BotOrbitPlanner(float arcOffset, float predictionSeconds, float obstaclePadding)
        {
            _arcOffset = arcOffset;
            _predictionSeconds = predictionSeconds;
            _obstaclePadding = obstaclePadding;
        }

        public void Clear()
        {
            _hasTarget = false;
            _hasWaypoint = false;
        }

        public Vector2? NextGoal(BotObservation observation)
        {
            if (_hasTarget)
            {
                var stillVisible = false;
                foreach (var pickup in observation.Pickups)
                {
                    if (!pickup.IsExperience || (pickup.Position - _target).sqrMagnitude > 0.0625f) continue;
                    stillVisible = true;
                    break;
                }
                if (!stillVisible) Clear();
            }

            if (!_hasTarget)
            {
                if (!SelectReachableTarget(observation)) return null;
            }

            if (_hasWaypoint && (observation.Position - _waypoint).sqrMagnitude <= 1f)
                _hasWaypoint = false;
            return _hasWaypoint ? _waypoint : _target;
        }

        private bool SelectReachableTarget(BotObservation observation)
        {
            var count = 0;
            foreach (var pickup in observation.Pickups)
            {
                if (!pickup.IsExperience) continue;
                var duplicate = false;
                for (var existing = 0; existing < count; existing++)
                    if ((_candidatePositions[existing] - pickup.Position).sqrMagnitude < 0.25f)
                    {
                        duplicate = true;
                        break;
                    }
                if (duplicate) continue;
                var distance = (pickup.Position - observation.Position).sqrMagnitude;
                var index = 0;
                while (index < count && _candidateDistances[index] <= distance) index++;
                if (index >= _candidatePositions.Length) continue;
                var last = Mathf.Min(count, _candidatePositions.Length - 1);
                for (var shift = last; shift > index; shift--)
                {
                    _candidatePositions[shift] = _candidatePositions[shift - 1];
                    _candidateDistances[shift] = _candidateDistances[shift - 1];
                }
                _candidatePositions[index] = pickup.Position;
                _candidateDistances[index] = distance;
                count = Mathf.Min(count + 1, _candidatePositions.Length);
            }
            for (var index = 0; index < count; index++)
            {
                _target = _candidatePositions[index];
                _hasTarget = true;
                _hasWaypoint = false;
                if (RouteRisk(observation, observation.Position, _target) < 0.75f || TryPlanArc(observation))
                    return true;
            }
            Clear();
            return false;
        }

        private bool TryPlanArc(BotObservation observation)
        {
            var delta = _target - observation.Position;
            if (delta.sqrMagnitude < 0.0001f) return false;
            var normal = new Vector2(-delta.y, delta.x).normalized;
            var midpoint = (observation.Position + _target) * 0.5f;
            var directRisk = RouteRisk(observation, observation.Position, _target);
            var bestCost = directRisk - 0.25f;
            var found = false;
            for (var side = 1; side >= -1; side -= 2)
            {
                var candidate = midpoint + normal * (_arcOffset * side);
                if (!RouteClear(observation, observation.Position, candidate) ||
                    !RouteClear(observation, candidate, _target)) continue;
                var risk = Mathf.Max(RouteRisk(observation, observation.Position, candidate),
                    RouteRisk(observation, candidate, _target));
                var extraLength = Vector2.Distance(observation.Position, candidate) +
                    Vector2.Distance(candidate, _target) - delta.magnitude;
                var cost = risk + 0.05f * extraLength;
                if (cost >= bestCost) continue;
                bestCost = cost;
                _waypoint = candidate;
                found = true;
            }
            _hasWaypoint = found;
            return found;
        }

        private bool RouteClear(BotObservation observation, Vector2 start, Vector2 end)
        {
            var padding = observation.PlayerRadius + _obstaclePadding;
            if (end.x < observation.ArenaBounds.xMin + padding || end.x > observation.ArenaBounds.xMax - padding ||
                end.y < observation.ArenaBounds.yMin + padding || end.y > observation.ArenaBounds.yMax - padding)
                return false;
            foreach (var obstacle in observation.Obstacles)
            {
                var bounds = obstacle.Bounds;
                bounds.xMin -= padding; bounds.xMax += padding;
                bounds.yMin -= padding; bounds.yMax += padding;
                if (BotMovementPolicy.SegmentIntersectsRect(start, end, bounds)) return false;
            }
            return true;
        }

        private float RouteRisk(BotObservation observation, Vector2 start, Vector2 end)
        {
            var maximum = 0f;
            for (var sample = 1; sample <= 8; sample++)
            {
                var point = Vector2.Lerp(start, end, sample / 8f);
                var danger = 0f;
                foreach (var threat in observation.Threats)
                {
                    var predicted = threat.Position + threat.Velocity * _predictionSeconds;
                    var distance = Mathf.Min(Vector2.Distance(point, threat.Position),
                        Vector2.Distance(point, predicted));
                    var margin = observation.PlayerRadius + threat.Radius + 1.5f;
                    var exposure = Mathf.Max(0f, margin - distance) / margin;
                    danger += threat.Weight * exposure * exposure;
                }
                foreach (var beam in observation.Beams)
                {
                    var distance = BotMovementPolicy.DistanceToSegment(point, beam.Start, beam.End);
                    var margin = observation.PlayerRadius + beam.HalfWidth + 1.5f;
                    var exposure = Mathf.Max(0f, margin - distance) / margin;
                    danger += beam.Weight * exposure * exposure;
                }
                maximum = Mathf.Max(maximum, danger);
            }
            return maximum;
        }
    }
}
