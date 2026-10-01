using System;
using System.Collections.Generic;
using Game.Character;
using Game.Diagnostics;
using UnityEngine;

namespace Game.Zones
{
    /// <summary>
    /// Runs the zones of one field: relocates pulsing zones when a new cycle begins, and while a zone is active applies its
    /// effect to the player and (for the kinds that touch them) to enemies. It owns no scene objects and advances only
    /// when ticked, so the caller decides when time passes (pause-aware by construction).
    /// </summary>
    public sealed class ZoneRuntime : IDisposable
    {
        /// <summary>Key of the single stat modifier that carries every zone's effect on the player.</summary>
        public const string ModifierKey = "zones";
        private const float ModifierEpsilon = 1e-5f;

        private readonly List<ZonePlacement> _zones;
        private readonly ZonePlacementRules _rules;
        private readonly int _seed;
        private readonly IZonePlayerTarget _player;
        private readonly IZoneEnemySource _enemies;
        private float _portalCooldown;
        private bool _modifierSet;
        private float _move, _skill, _action, _regen;

        public IReadOnlyList<ZonePlacement> Zones => _zones;
        /// <summary>Seconds this runtime has been ticked (the zones' pulse clock).</summary>
        public float Time { get; private set; }
        /// <summary>How many active zones the player stood in on the last tick.</summary>
        public int PlayerActiveZoneCount { get; private set; }
        public float PortalCooldownRemaining => _portalCooldown;

        public ZoneRuntime(IReadOnlyList<ZonePlacement> zones, ZonePlacementRules rules, int seed, IZonePlayerTarget player,
            IZoneEnemySource enemies)
        {
            if (zones == null) throw new ArgumentNullException(nameof(zones));
            _zones = new List<ZonePlacement>(zones);
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _seed = seed;
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) return;
            Time += deltaTime;
            _portalCooldown = Mathf.Max(0f, _portalCooldown - deltaTime);
            Relocate();
            TickPlayer(deltaTime);
            TickEnemies(deltaTime);
        }

        // A pulsing zone is invisible when a new cycle starts, so it can jump to a fresh random spot unseen.
        private void Relocate()
        {
            foreach (var zone in _zones)
            {
                if (zone.Effect.Lifetime != ZoneLifetimeMode.Pulsing) continue;
                var cycle = zone.CycleAt(Time);
                if (cycle == zone.Cycle) continue;
                var random = new System.Random(unchecked(_seed * 31 + zone.Index * 7919 + cycle * 104729));
                var center = _rules.TryPick(zone.Effect, random, OthersOf(zone), null, out var picked) ? picked : zone.Center;
                zone.Relocate(center, cycle);
            }
        }

        private IEnumerable<ZonePlacement> OthersOf(ZonePlacement zone)
        {
            foreach (var other in _zones)
                if (other != zone) yield return other;
        }

        private void TickPlayer(float deltaTime)
        {
            PlayerActiveZoneCount = 0;
            if (!_player.IsAlive)
            {
                ApplyModifier(0f, 0f, 0f, 0f);
                return;
            }
            float move = 0f, skill = 0f, action = 0f, regen = 0f, damage = 0f;
            ZonePlacement portal = null;
            var position = _player.Position;
            foreach (var zone in _zones)
            {
                if (!zone.IsActive(Time) || !zone.Contains(position)) continue;
                PlayerActiveZoneCount++;
                var effect = zone.Effect;
                switch (effect.Kind)
                {
                    case ZoneEffectKind.Slow:
                    case ZoneEffectKind.Haste:
                        move += effect.PlayerMovementBonus;
                        break;
                    case ZoneEffectKind.Regeneration:
                        regen += effect.PlayerRegenerationPerSecond;
                        break;
                    case ZoneEffectKind.ArcanePower:
                        skill += effect.PlayerSkillDamageBonus;
                        action += effect.PlayerActionSpeedBonus;
                        break;
                    case ZoneEffectKind.Rift:
                        damage += effect.PlayerDamagePerSecond;
                        if (effect.PlayerDamagePerSecond > 0f) _player.Damage(effect.PlayerDamagePerSecond * deltaTime, effect.Id);
                        break;
                    default:
                        if (portal == null && _portalCooldown <= 0f) portal = zone;
                        break;
                }
            }
            ApplyModifier(move, skill, action, regen);
            if (portal != null) Teleport(portal);
        }

        private void Teleport(ZonePlacement from)
        {
            if (from.PartnerIndex < 0 || from.PartnerIndex >= _zones.Count) return;
            var partner = _zones[from.PartnerIndex];
            var toArenaCenter = -partner.Center;
            var direction = toArenaCenter.sqrMagnitude > 1e-6f ? toArenaCenter.normalized : Vector2.up;
            _player.TeleportTo(partner.Center + direction * (partner.Effect.Radius + partner.Effect.PortalExitDistance));
            _portalCooldown = from.Effect.PortalCooldownSeconds;
        }

        private void ApplyModifier(float move, float skill, float action, float regen)
        {
            var zero = Mathf.Abs(move) < ModifierEpsilon && Mathf.Abs(skill) < ModifierEpsilon &&
                       Mathf.Abs(action) < ModifierEpsilon && Mathf.Abs(regen) < ModifierEpsilon;
            if (zero)
            {
                if (_modifierSet) _player.RemoveStatModifier(ModifierKey);
                _modifierSet = false;
                _move = _skill = _action = _regen = 0f;
                return;
            }
            if (_modifierSet && Mathf.Approximately(move, _move) && Mathf.Approximately(skill, _skill) &&
                Mathf.Approximately(action, _action) && Mathf.Approximately(regen, _regen)) return;
            _player.SetStatModifier(ModifierKey, new CharacterStatModifier(movementSpeedMultiplierBonus: move,
                activeSkillDamageMultiplierBonus: skill, actionSpeedBonus: action, healthRegenerationPerSecondBonus: regen));
            _modifierSet = true;
            _move = move; _skill = skill; _action = action; _regen = regen;
        }

        private void TickEnemies(float deltaTime)
        {
            var touchesEnemies = false;
            foreach (var zone in _zones)
                if (zone.IsActive(Time) && AffectsEnemies(zone.Effect)) { touchesEnemies = true; break; }
            if (!touchesEnemies) return;
            // Cost scales with living enemies times zones (DECISION-0008).
            using var guard = PerfGuard.Measure("Zones.TickEnemies", 2f);
            var count = _enemies.Refresh();
            for (var i = 0; i < count; i++)
            {
                var position = _enemies.Position(i);
                foreach (var zone in _zones)
                {
                    if (!zone.IsActive(Time) || !AffectsEnemies(zone.Effect) || !zone.Contains(position)) continue;
                    var effect = zone.Effect;
                    if (effect.Kind == ZoneEffectKind.Slow)
                        _enemies.Slow(i, effect.EnemySlowFraction, effect.EnemySlowSeconds, effect.Id);
                    else
                        _enemies.Damage(i, effect.EnemyDamagePerSecond * deltaTime, effect.Id);
                }
            }
        }

        private static bool AffectsEnemies(ZoneEffectDefinition effect) =>
            (effect.Kind == ZoneEffectKind.Slow && effect.EnemySlowFraction > 0f) ||
            (effect.Kind == ZoneEffectKind.Rift && effect.EnemyDamagePerSecond > 0f);

        /// <summary>Removes the zone modifier from the player; call when the run's field is torn down.</summary>
        public void Dispose()
        {
            if (_modifierSet) _player.RemoveStatModifier(ModifierKey);
            _modifierSet = false;
        }
    }
}
