using System;
using System.Collections.Generic;
using Game.Automation;
using Game.Character;
using Game.Diagnostics;
using Game.Enemy;
using Game.Pickup;
using Game.Progression;
using UnityEngine;

namespace Game.Bootstrap.Automation
{
    /// <summary>Reads only current runtime state. All buffers are owned by this main-thread adapter.</summary>
    public sealed class AutomationObservationAdapter
    {
        private readonly PlayerCharacterRuntime _player;
        private readonly PlayerExperienceRuntime _experience;
        private readonly WorldPickupRuntime _worldPickups;
        private readonly BossEncounterRuntime _bosses;
        private readonly IReadOnlyList<Collider2D> _obstacleColliders;
        private readonly Rect _arenaBounds;
        private readonly float _observationRadius;
        private readonly List<EnemyRuntime> _enemies = new List<EnemyRuntime>();
        private readonly List<EnemyProjectileRuntime> _projectiles = new List<EnemyProjectileRuntime>();
        private readonly List<ExperienceDropRuntime> _drops = new List<ExperienceDropRuntime>();
        private readonly List<WorldPickupVisual> _pickups = new List<WorldPickupVisual>();
        private readonly List<BossHazardVisual> _hazards = new List<BossHazardVisual>();
        private readonly List<BotThreat> _threats = new List<BotThreat>();
        private readonly List<BotPickup> _collectibles = new List<BotPickup>();
        private readonly List<BotObstacle> _obstacles = new List<BotObstacle>();
        private readonly List<BotBeam> _beams = new List<BotBeam>();

        public AutomationObservationAdapter(PlayerCharacterRuntime player, PlayerExperienceRuntime experience,
            WorldPickupRuntime worldPickups, BossEncounterRuntime bosses, IReadOnlyList<Collider2D> obstacleColliders,
            Rect arenaBounds, float observationRadius)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _experience = experience ?? throw new ArgumentNullException(nameof(experience));
            _worldPickups = worldPickups ?? throw new ArgumentNullException(nameof(worldPickups));
            _bosses = bosses;
            _obstacleColliders = obstacleColliders ?? throw new ArgumentNullException(nameof(obstacleColliders));
            _arenaBounds = arenaBounds;
            _observationRadius = observationRadius;
        }

        public BotObservation Capture()
        {
            using var guard = PerfGuard.Measure("AutomationObservationAdapter.Capture", 3f);
            var position = (Vector2)_player.transform.position;
            var radiusSquared = _observationRadius * _observationRadius;
            _threats.Clear(); _collectibles.Clear(); _obstacles.Clear(); _beams.Clear();
            var playerCollider = _player.GetComponent<CircleCollider2D>();
            var playerRadius = playerCollider != null ? playerCollider.radius *
                Mathf.Max(Mathf.Abs(_player.transform.lossyScale.x), Mathf.Abs(_player.transform.lossyScale.y)) : 0f;

            EnemyRegistry.CopyAliveTo(_enemies);
            foreach (var enemy in _enemies)
            {
                if ((enemy.Position - position).sqrMagnitude > radiusSquared) continue;
                var body = enemy.GetComponent<Rigidbody2D>();
                var collider = enemy.GetComponent<CircleCollider2D>();
                var movement = enemy.CurrentMovement;
                var motion = movement?.Kind == EnemyMovementKind.Seek ? BotThreatMotion.Seek :
                    movement?.Kind == EnemyMovementKind.KeepDistance ? BotThreatMotion.KeepDistance : BotThreatMotion.Linear;
                if (enemy.Category != EnemyCategory.Ordinary) motion = BotThreatMotion.Linear;
                var enemyRadius = collider != null ? collider.radius *
                    Mathf.Max(Mathf.Abs(enemy.transform.lossyScale.x), Mathf.Abs(enemy.transform.lossyScale.y)) : 0.5f;
                _threats.Add(new BotThreat(enemy.Position, body != null ? body.linearVelocity : Vector2.zero,
                    collider != null ? collider.radius : 0.5f, 3f, isEnemy: true, motion: motion,
                    movementSpeed: enemy.Definition.MovementSpeed * enemy.Controls.MovementMultiplier,
                    preferredDistance: movement?.PreferredDistance ?? 0f,
                    distanceTolerance: movement?.DistanceTolerance ?? 0f, collisionRadius: enemyRadius));
            }
            EnemyProjectileRegistry.CopyActiveTo(_projectiles);
            foreach (var projectile in _projectiles)
            {
                if ((projectile.Position - position).sqrMagnitude > radiusSquared) continue;
                _threats.Add(new BotThreat(projectile.Position, projectile.Velocity,
                    Mathf.Max(projectile.Profile.ProjectileRadius, projectile.Profile.ExplosionRadius), 10f));
            }

            _experience.CopyActiveDropsTo(_drops);
            foreach (var drop in _drops)
                if (((Vector2)drop.transform.position - position).sqrMagnitude <= radiusSquared)
                    _collectibles.Add(new BotPickup(drop.transform.position, 1f, value: drop.Amount,
                        remainingSeconds: drop.RemainingSeconds));
            _worldPickups.CopyActiveTo(_pickups);
            foreach (var pickup in _pickups)
                if (((Vector2)pickup.transform.position - position).sqrMagnitude <= radiusSquared)
                    _collectibles.Add(new BotPickup(pickup.transform.position, 2f, isExperience: false,
                        contentId: pickup.Life.Definition.Id.ToString()));

            foreach (var collider in _obstacleColliders)
            {
                if (collider == null || !collider.enabled) continue;
                var bounds = collider.bounds;
                if (((Vector2)bounds.center - position).sqrMagnitude >
                    (_observationRadius + bounds.extents.magnitude) * (_observationRadius + bounds.extents.magnitude)) continue;
                _obstacles.Add(new BotObstacle(Rect.MinMaxRect(bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y)));
            }

            var coverageComplete = true;
            _bosses?.CopyHazardVisualsTo(_hazards);
            foreach (var hazard in _hazards)
            {
                switch (hazard.Kind)
                {
                    case BossHazardVisualKind.ZoneEdge:
                    case BossHazardVisualKind.ZoneFill:
                    case BossHazardVisualKind.Burning:
                        _threats.Add(new BotThreat(hazard.Center, Vector2.zero, hazard.Radius, 12f));
                        break;
                    case BossHazardVisualKind.BeamTelegraph:
                    case BossHazardVisualKind.BeamActive:
                        var angle = hazard.AngleDegrees * Mathf.Deg2Rad;
                        var end = hazard.Center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * hazard.Length;
                        _beams.Add(new BotBeam(hazard.Center, end, hazard.Width * 0.5f,
                            hazard.Kind == BossHazardVisualKind.BeamActive ? 12f : 4f));
                        break;
                    case BossHazardVisualKind.SummonMarker:
                        _threats.Add(new BotThreat(hazard.Center, Vector2.zero, hazard.Radius, 1f));
                        break;
                    case BossHazardVisualKind.DangerWash:
                        // The safe-circle inverse area cannot be reduced to a circle threat.
                        coverageComplete = false;
                        break;
                    case BossHazardVisualKind.SafeCircle:
                    case BossHazardVisualKind.ZoneImpact:
                        break;
                    default:
                        coverageComplete = false;
                        break;
                }
            }
            var health = _player.Health;
            var healthFraction = health != null && health.MaxHealth > 0f ?
                health.CurrentHealth / health.MaxHealth : 0f;
            return new BotObservation(position, _player.MovementSpeed, playerRadius, _arenaBounds,
                _threats.ToArray(), _collectibles.ToArray(), _obstacles.ToArray(), _beams.ToArray(),
                coverageComplete, healthFraction, _experience.PickupRadius);
        }
    }
}
