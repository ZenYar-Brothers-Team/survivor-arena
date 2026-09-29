using System;
using System.Collections.Generic;
using Game.ActiveSkill;
using Game.Diagnostics;
using Game.Enemy;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Game.Bootstrap.Automation
{
    /// <summary>AB-14 raw current-state sensor, shared by demonstration capture and future inference. Main thread only.</summary>
    public sealed class DemonstrationObservationAdapter
    {
        private readonly AutomationRuntimeBindings _bindings;
        private readonly PlayerActiveSkillSetRuntime _skills;
        private readonly Rigidbody2D _body;
        private readonly Camera _camera;
        private readonly float _radiusSquared;
        private readonly int _limit;
        private readonly List<EnemyRuntime> _enemies = new List<EnemyRuntime>();
        public int TruncatedEntities { get; private set; }
        public bool CoverageComplete { get; private set; }

        public DemonstrationObservationAdapter(AutomationRuntimeBindings bindings, float radius, int limit)
        {
            _bindings = bindings ?? throw new ArgumentNullException(nameof(bindings));
            _skills = bindings.Player.GetComponent<PlayerActiveSkillSetRuntime>();
            _body = bindings.Player.GetComponent<Rigidbody2D>();
            _camera = Camera.main;
            _radiusSquared = radius * radius;
            _limit = limit;
        }

        public JObject Capture()
        {
            using var guard = PerfGuard.Measure("DemonstrationObservationAdapter.Capture", 8f);
            var observation = _bindings.Observation.Capture();
            TruncatedEntities = 0;
            CoverageComplete = observation.CoverageComplete;
            var threats = new JArray();
            foreach (var threat in observation.Threats)
            {
                if (threats.Count >= _limit) { TruncatedEntities++; continue; }
                threats.Add(new JObject { ["position"] = Vector(threat.Position), ["velocity"] = Vector(threat.Velocity),
                    ["radius"] = threat.CollisionRadius, ["isEnemy"] = threat.IsEnemy,
                    ["forecastMotion"] = threat.Motion.ToString() });
            }
            var pickups = new JArray();
            foreach (var pickup in observation.Pickups)
            {
                if (pickups.Count >= _limit) { TruncatedEntities++; continue; }
                pickups.Add(new JObject { ["position"] = Vector(pickup.Position), ["isExperience"] = pickup.IsExperience,
                    ["contentId"] = pickup.ContentId, ["value"] = pickup.Value,
                    ["remainingSeconds"] = float.IsInfinity(pickup.RemainingSeconds) ? (float?)null : pickup.RemainingSeconds });
            }
            var obstacles = new JArray();
            foreach (var obstacle in observation.Obstacles)
            {
                if (obstacles.Count >= _limit) { TruncatedEntities++; continue; }
                obstacles.Add(Bounds(obstacle.Bounds));
            }
            var beams = new JArray();
            foreach (var beam in observation.Beams)
            {
                if (beams.Count >= _limit) { TruncatedEntities++; continue; }
                beams.Add(new JObject { ["start"] = Vector(beam.Start), ["end"] = Vector(beam.End), ["halfWidth"] = beam.HalfWidth });
            }
            var enemies = new JArray();
            EnemyRegistry.CopyAliveTo(_enemies);
            foreach (var enemy in _enemies)
            {
                if ((enemy.Position - observation.Position).sqrMagnitude > _radiusSquared) continue;
                if (enemies.Count >= _limit) { TruncatedEntities++; continue; }
                enemies.Add(new JObject { ["lifeId"] = enemy.LifeId.ToString("N"), ["contentId"] = enemy.ContentId.ToString(),
                    ["position"] = Vector(enemy.Position), ["hp"] = enemy.Health.CurrentHealth,
                    ["maxHp"] = enemy.Health.MaxHealth, ["movementKind"] = enemy.CurrentMovement?.Kind.ToString(),
                    ["movementPhase"] = enemy.MovementPhase.ToString(), ["attackPhase"] = enemy.AttackPhase?.ToString() });
            }
            var skills = new JArray();
            if (_skills != null)
                foreach (var skill in _skills.Skills)
                    skills.Add(new JObject { ["id"] = skill.Definition.Id.ToString(), ["level"] = skill.Level,
                        ["cooldownRemainingSeconds"] = skill.CooldownRemainingSeconds,
                        ["aimDirection"] = Vector(skill.LastAimDirection), ["activations"] = skill.TriggerCount });
            var build = new JArray();
            foreach (var entry in _bindings.Draft.Build.Entries)
                build.Add(new JObject { ["id"] = entry.Definition.Id.ToString(), ["level"] = entry.Level,
                    ["kind"] = entry.Definition.Kind.ToString() });
            var player = _bindings.Player;
            var stats = player.Stats;
            JArray viewport = null;
            if (_camera != null && _camera.orthographic)
            {
                var center = (Vector2)_camera.transform.position;
                var size = new Vector2(_camera.orthographicSize * _camera.aspect, _camera.orthographicSize);
                viewport = Bounds(Rect.MinMaxRect(center.x - size.x, center.y - size.y, center.x + size.x, center.y + size.y));
            }
            return new JObject
            {
                ["player"] = new JObject { ["position"] = Vector(_body != null ? _body.position : observation.Position),
                    ["velocity"] = Vector(_body != null ? _body.linearVelocity : Vector2.zero),
                    ["movementSpeed"] = observation.MovementSpeed, ["radius"] = observation.PlayerRadius,
                    ["pickupRadius"] = observation.PickupRadius, ["hp"] = player.Health.CurrentHealth,
                    ["maxHp"] = player.Health.MaxHealth, ["level"] = _bindings.Experience.Progression.Level,
                    ["experience"] = _bindings.Experience.Progression.CurrentExperience,
                    ["damageMultiplier"] = stats.ActiveSkillDamageMultiplier, ["actionSpeedBonus"] = stats.ActionSpeedBonus,
                    ["xpMultiplier"] = stats.PickedUpXpMultiplier },
                ["skills"] = skills, ["build"] = build,
                ["phaseId"] = _bindings.Spawner.Director.CurrentPhase.Id.ToString(),
                ["arenaBounds"] = Bounds(observation.ArenaBounds), ["viewportBounds"] = viewport,
                ["threats"] = threats, ["enemyStates"] = enemies, ["pickups"] = pickups,
                ["obstacles"] = obstacles, ["beams"] = beams,
                ["coverageComplete"] = CoverageComplete, ["truncatedEntities"] = TruncatedEntities
            };
        }

        private static JArray Vector(Vector2 value) => new JArray(value.x, value.y);
        private static JArray Bounds(Rect value) => new JArray(value.xMin, value.yMin, value.xMax, value.yMax);
    }
}
