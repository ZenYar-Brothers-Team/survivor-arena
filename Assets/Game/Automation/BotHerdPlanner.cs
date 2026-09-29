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
        private readonly bool _adaptive;
        private float _modeSeconds;
        private float _avoidSeconds;
        private Vector2 _bank;
        private Vector2 _avoidedBank;
        private Vector2 _lureStart;
        private Vector2 _lureGoal;
        private int _sweepSign;

        public BotHerdMode Mode { get; private set; }
        public Vector2? Goal { get; private set; }
        public int CrowdCount { get; private set; }
        public int RouteBlockers { get; private set; }
        public int BankRouteBlockers { get; private set; }
        public string TransitionReason { get; private set; } = "initial";

        public BotHerdPlanner(MovementPolicyData settings)
        {
            _crowdMinEnemies = settings.CrowdMinEnemies.Value;
            _crowdRadius = settings.CrowdRadius.Value;
            _arcOffset = settings.ArcOffsetWorldUnits.Value;
            _lureSeconds = settings.LureSeconds.Value;
            _sweepSeconds = settings.SweepSeconds.Value;
            _collectSeconds = settings.CollectSeconds.Value;
            _adaptive = settings.Id == "herdLoopAdaptive";
        }

        public void Clear()
        {
            Mode = BotHerdMode.Forage;
            Goal = null;
            _modeSeconds = 0f;
            _avoidSeconds = 0f;
            RouteBlockers = 0;
            BankRouteBlockers = 0;
            CrowdCount = 0;
            TransitionReason = "reset";
        }

        public void Update(BotObservation observation, float elapsedSimulationSeconds)
        {
            _modeSeconds += elapsedSimulationSeconds;
            _avoidSeconds = Mathf.Max(0f, _avoidSeconds - elapsedSimulationSeconds);
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
            CrowdCount = crowdCount;
            RouteBlockers = Goal.HasValue ? CountRouteBlockers(observation, Goal.Value) : 0;

            switch (Mode)
            {
                case BotHerdMode.Forage:
                    Goal = _adaptive ? PreferredXp(observation) :
                        NearestXp(observation, observation.Position, float.PositiveInfinity);
                    RouteBlockers = Goal.HasValue ? CountRouteBlockers(observation, Goal.Value) : 0;
                    if (crowdCount >= _crowdMinEnemies && Goal.HasValue &&
                        (Goal.Value - observation.Position).sqrMagnitude > 4f &&
                        RouteBlockers >= 3)
                    {
                        _bank = Goal.Value;
                        Transition(BotHerdMode.Lure, "xp-route-blocked");
                        BeginLure(observation, crowdCenter);
                    }
                    break;
                case BotHerdMode.Lure:
                    if (_modeSeconds >= _lureSeconds ||
                        (observation.Position - _lureStart).sqrMagnitude >= _arcOffset * _arcOffset * 0.64f)
                    {
                        Transition(BotHerdMode.Sweep, "lure-distance-or-time");
                        _sweepSign = ChooseSweepSide(observation, crowdCenter);
                    }
                    else Goal = _lureGoal;
                    break;
                case BotHerdMode.Sweep:
                    if (_adaptive && _modeSeconds >= _sweepSeconds * 2f &&
                        CountRouteBlockers(observation, _bank) >= 3)
                    {
                        AbandonBank("sweep-route-still-blocked");
                        break;
                    }
                    if (_modeSeconds >= _sweepSeconds &&
                        (CountRouteBlockers(observation, _bank) < 3 ||
                        !_adaptive && _modeSeconds >= _sweepSeconds * 2f))
                        Transition(BotHerdMode.Collect, "xp-route-open");
                    else Goal = SweepGoal(observation, crowdCenter);
                    break;
                case BotHerdMode.Collect:
                    var nearbyXp = NearestXp(observation, _bank, _arcOffset * 1.5f);
                    Goal = nearbyXp ?? _bank;
                    if (_adaptive && CountRouteBlockers(observation, Goal.Value) >= 3 &&
                        observation.HealthFraction < 0.5f)
                    {
                        AbandonBank("collect-route-unsafe");
                        break;
                    }
                    if (_modeSeconds >= _collectSeconds ||
                        crowdCount >= _crowdMinEnemies && observation.HealthFraction < 0.35f)
                        Transition(BotHerdMode.Forage, "collect-complete-or-unsafe");
                    break;
            }

            if (Mode == BotHerdMode.Collect)
                Goal = NearestXp(observation, _bank, _arcOffset * 1.5f) ?? _bank;
            else if (Mode == BotHerdMode.Sweep)
                Goal = SweepGoal(observation, crowdCenter);
            RouteBlockers = Goal.HasValue ? CountRouteBlockers(observation, Goal.Value) : 0;
            BankRouteBlockers = Mode == BotHerdMode.Forage ? 0 :
                CountRouteBlockers(observation, _bank);
        }

        private Vector2? PreferredXp(BotObservation observation)
        {
            Vector2? safe = null;
            Vector2? blocked = null;
            var safeDistance = float.PositiveInfinity;
            var blockedDistance = float.PositiveInfinity;
            foreach (var pickup in observation.Pickups)
            {
                if (!pickup.IsExperience || _avoidSeconds > 0f &&
                    (pickup.Position - _avoidedBank).sqrMagnitude < 2.25f) continue;
                var distance = (pickup.Position - observation.Position).sqrMagnitude;
                if (CountRouteBlockers(observation, pickup.Position) < 3)
                {
                    if (distance < safeDistance) { safe = pickup.Position; safeDistance = distance; }
                }
                else if (distance < blockedDistance) { blocked = pickup.Position; blockedDistance = distance; }
            }
            return safe ?? blocked;
        }

        private void AbandonBank(string reason)
        {
            _avoidedBank = _bank;
            _avoidSeconds = _collectSeconds;
            Transition(BotHerdMode.Forage, reason);
            Goal = null;
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

        private static int CountRouteBlockers(BotObservation observation, Vector2 goal)
        {
            var path = goal - observation.Position;
            var lengthSquared = path.sqrMagnitude;
            if (lengthSquared < 0.25f) return 0;
            var blockers = 0;
            foreach (var threat in observation.Threats)
            {
                if (!threat.IsEnemy) continue;
                var progress = Vector2.Dot(threat.Position - observation.Position, path) / lengthSquared;
                if (progress <= 0.05f || progress >= 1f) continue;
                var closest = observation.Position + path * progress;
                var clearance = observation.PlayerRadius + threat.Radius + 1.25f;
                if ((threat.Position - closest).sqrMagnitude < clearance * clearance) blockers++;
            }
            return blockers;
        }

        private static Vector2 ClampGoal(BotObservation observation, Vector2 goal)
        {
            var padding = observation.PlayerRadius + 0.2f;
            return new Vector2(
                Mathf.Clamp(goal.x, observation.ArenaBounds.xMin + padding, observation.ArenaBounds.xMax - padding),
                Mathf.Clamp(goal.y, observation.ArenaBounds.yMin + padding, observation.ArenaBounds.yMax - padding));
        }

        private void Transition(BotHerdMode next, string reason)
        {
            Mode = next;
            _modeSeconds = 0f;
            TransitionReason = reason;
        }
    }
}
