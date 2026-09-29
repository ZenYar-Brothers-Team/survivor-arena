using UnityEngine;

namespace Game.Automation
{
    /// <summary>Visible-state lure/sweep/return plan; never reads future spawns or gameplay RNG (IP-34 AB-11).</summary>
    internal sealed class BotHerdPlanner
    {
        private readonly int _crowdMinEnemies;
        private readonly float _crowdRadius;
        private readonly float _arcOffset;
        private readonly float _lureSeconds;
        private readonly float _sweepSeconds;
        private readonly float _collectSeconds;
        private float _modeSeconds;
        private Vector2 _bank;
        private Vector2 _lureStart;
        private Vector2 _lureGoal;
        private int _sweepSign;

        public BotHerdMode Mode { get; private set; }
        public Vector2? Goal { get; private set; }

        public BotHerdPlanner(MovementPolicyData settings)
        {
            _crowdMinEnemies = settings.CrowdMinEnemies.Value;
            _crowdRadius = settings.CrowdRadius.Value;
            _arcOffset = settings.ArcOffsetWorldUnits.Value;
            _lureSeconds = settings.LureSeconds.Value;
            _sweepSeconds = settings.SweepSeconds.Value;
            _collectSeconds = settings.CollectSeconds.Value;
        }

        public void Clear()
        {
            Mode = BotHerdMode.Forage;
            Goal = null;
            _modeSeconds = 0f;
        }

        public void Update(BotObservation observation, float elapsedSimulationSeconds)
        {
            _modeSeconds += elapsedSimulationSeconds;
            var crowdCenter = Vector2.zero;
            var crowdCount = 0;
            var radiusSquared = _crowdRadius * _crowdRadius;
            foreach (var threat in observation.Threats)
            {
                if (!threat.IsEnemy || (threat.Position - observation.Position).sqrMagnitude > radiusSquared) continue;
                crowdCenter += threat.Position;
                crowdCount++;
            }
            if (crowdCount > 0) crowdCenter /= crowdCount;

            switch (Mode)
            {
                case BotHerdMode.Forage:
                    Goal = NearestXp(observation, observation.Position, float.PositiveInfinity);
                    if (crowdCount >= _crowdMinEnemies && Goal.HasValue &&
                        (Goal.Value - observation.Position).sqrMagnitude > 4f &&
                        EnemyBlocksRoute(observation, Goal.Value))
                    {
                        _bank = Goal.Value;
                        Transition(BotHerdMode.Lure);
                        BeginLure(observation, crowdCenter);
                    }
                    break;
                case BotHerdMode.Lure:
                    if (_modeSeconds >= _lureSeconds ||
                        (observation.Position - _lureStart).sqrMagnitude >= _arcOffset * _arcOffset * 0.64f)
                    {
                        Transition(BotHerdMode.Sweep);
                        _sweepSign = ChooseSweepSide(observation, crowdCenter);
                    }
                    else Goal = _lureGoal;
                    break;
                case BotHerdMode.Sweep:
                    if (_modeSeconds >= _sweepSeconds &&
                        (!EnemyBlocksRoute(observation, _bank) || _modeSeconds >= _sweepSeconds * 2f))
                        Transition(BotHerdMode.Collect);
                    else Goal = SweepGoal(observation, crowdCenter);
                    break;
                case BotHerdMode.Collect:
                    var nearbyXp = NearestXp(observation, _bank, _arcOffset * 1.5f);
                    Goal = nearbyXp ?? _bank;
                    if (_modeSeconds >= _collectSeconds ||
                        crowdCount >= _crowdMinEnemies && observation.HealthFraction < 0.35f)
                        Transition(BotHerdMode.Forage);
                    break;
            }

            if (Mode == BotHerdMode.Collect)
                Goal = NearestXp(observation, _bank, _arcOffset * 1.5f) ?? _bank;
            else if (Mode == BotHerdMode.Sweep)
                Goal = SweepGoal(observation, crowdCenter);
        }

        private void BeginLure(BotObservation observation, Vector2 crowdCenter)
        {
            _lureStart = observation.Position;
            var away = observation.Position - crowdCenter;
            if (away.sqrMagnitude < 0.25f) away = observation.Position - _bank;
            if (away.sqrMagnitude < 0.25f) away = Vector2.right;
            away.Normalize();
            var left = new Vector2(-away.y, away.x);
            var right = -left;
            var first = ClampGoal(observation, observation.Position + away * _arcOffset);
            var second = ClampGoal(observation, observation.Position + left * _arcOffset);
            var third = ClampGoal(observation, observation.Position + right * _arcOffset);
            _lureGoal = first;
            if ((second - observation.Position).sqrMagnitude > (_lureGoal - observation.Position).sqrMagnitude)
                _lureGoal = second;
            if ((third - observation.Position).sqrMagnitude > (_lureGoal - observation.Position).sqrMagnitude)
                _lureGoal = third;
            Goal = _lureGoal;
        }

        private int ChooseSweepSide(BotObservation observation, Vector2 crowdCenter)
        {
            var radial = observation.Position - crowdCenter;
            if (radial.sqrMagnitude < 0.25f) radial = observation.Position - _bank;
            if (radial.sqrMagnitude < 0.25f) radial = Vector2.right;
            radial.Normalize();
            var tangent = new Vector2(-radial.y, radial.x);
            var left = ClampGoal(observation, observation.Position + tangent * _arcOffset);
            var right = ClampGoal(observation, observation.Position - tangent * _arcOffset);
            return (left - observation.Position).sqrMagnitude >= (right - observation.Position).sqrMagnitude ? 1 : -1;
        }

        private Vector2 SweepGoal(BotObservation observation, Vector2 crowdCenter)
        {
            var radial = observation.Position - crowdCenter;
            if (radial.sqrMagnitude < 0.25f) radial = observation.Position - _bank;
            if (radial.sqrMagnitude < 0.25f) radial = Vector2.right;
            radial.Normalize();
            var tangent = new Vector2(-radial.y, radial.x) * _sweepSign;
            return ClampGoal(observation, observation.Position + tangent * _arcOffset + radial * (_arcOffset * 0.35f));
        }

        private static Vector2? NearestXp(BotObservation observation, Vector2 origin, float radius)
        {
            Vector2? nearest = null;
            var best = radius * radius;
            foreach (var pickup in observation.Pickups)
            {
                if (!pickup.IsExperience) continue;
                var distance = (pickup.Position - origin).sqrMagnitude;
                if (distance >= best) continue;
                nearest = pickup.Position;
                best = distance;
            }
            return nearest;
        }

        private static bool EnemyBlocksRoute(BotObservation observation, Vector2 goal)
        {
            var path = goal - observation.Position;
            var lengthSquared = path.sqrMagnitude;
            if (lengthSquared < 0.25f) return false;
            var blockers = 0;
            foreach (var threat in observation.Threats)
            {
                if (!threat.IsEnemy) continue;
                var progress = Vector2.Dot(threat.Position - observation.Position, path) / lengthSquared;
                if (progress <= 0.05f || progress >= 1f) continue;
                var closest = observation.Position + path * progress;
                var clearance = observation.PlayerRadius + threat.Radius + 1.25f;
                if ((threat.Position - closest).sqrMagnitude < clearance * clearance && ++blockers >= 3)
                    return true;
            }
            return false;
        }

        private static Vector2 ClampGoal(BotObservation observation, Vector2 goal)
        {
            var padding = observation.PlayerRadius + 0.2f;
            return new Vector2(
                Mathf.Clamp(goal.x, observation.ArenaBounds.xMin + padding, observation.ArenaBounds.xMax - padding),
                Mathf.Clamp(goal.y, observation.ArenaBounds.yMin + padding, observation.ArenaBounds.yMax - padding));
        }

        private void Transition(BotHerdMode next)
        {
            Mode = next;
            _modeSeconds = 0f;
        }
    }
}
