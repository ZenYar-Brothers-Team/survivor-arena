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
        private readonly Func<Rect> _view;
        private float _portalCooldown;
        private bool _modifierSet;
        private float _move, _skill, _action, _regen, _defense;
        // Timed buffs that outlast their zone (one per effect; a new burst refreshes it instead of stacking).
        private readonly Dictionary<ZoneEffectDefinition, float> _buffs = new Dictionary<ZoneEffectDefinition, float>();
        private readonly List<ZoneEffectDefinition> _expiredBuffs = new List<ZoneEffectDefinition>();
        // Timed shields a shrine leaves on the player (incoming-damage reduction), one per shrine effect.
        private readonly Dictionary<ZoneEffectDefinition, float> _shields = new Dictionary<ZoneEffectDefinition, float>();

        public IReadOnlyList<ZonePlacement> Zones => _zones;
        /// <summary>Seconds this runtime has been ticked (the zones' pulse clock).</summary>
        public float Time { get; private set; }
        /// <summary>How many active zones the player stood in on the last tick.</summary>
        public int PlayerActiveZoneCount { get; private set; }
        public float PortalCooldownRemaining => _portalCooldown;
        /// <summary>Remaining fraction of the longest timed movement buff; read-only presentation projection.</summary>
        public float SpeedBuffRemaining01
        {
            get
            {
                float longest = 0f, fraction = 0f;
                foreach (var pair in _buffs)
                    if (pair.Key.TimedBuffMovementBonus > 0f && pair.Value > longest)
                    { longest = pair.Value; fraction = pair.Value / pair.Key.TimedBuffSeconds; }
                return Mathf.Clamp01(fraction);
            }
        }
        /// <summary>The player's screen widened by the layout's margin on every side: only zones touching it are simulated.</summary>
        public Rect ActiveWindow { get; private set; }
        /// <summary>Zones currently inside the active window.</summary>
        public int NearZoneCount { get; private set; }

        /// <param name="view">World rectangle the player's camera shows; the active window is derived from it each tick.</param>
        public ZoneRuntime(IReadOnlyList<ZonePlacement> zones, ZonePlacementRules rules, int seed, IZonePlayerTarget player,
            IZoneEnemySource enemies, Func<Rect> view)
        {
            if (zones == null) throw new ArgumentNullException(nameof(zones));
            _zones = new List<ZonePlacement>(zones);
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _seed = seed;
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            RefreshWindow();
        }

        /// <summary>The active window for a screen rectangle and margin; an empty screen means no limit.</summary>
        public static Rect WindowOf(Rect view, float margin)
        {
            if (view.width <= 0f || view.height <= 0f) return new Rect(-1e6f, -1e6f, 2e6f, 2e6f);
            var dx = view.width * margin;
            var dy = view.height * margin;
            return new Rect(view.xMin - dx, view.yMin - dy, view.width + 2f * dx, view.height + 2f * dy);
        }

        private void RefreshWindow()
        {
            ActiveWindow = WindowOf(_view(), _rules.ActiveScreenMargin);
            NearZoneCount = 0;
            foreach (var zone in _zones)
            {
                var near = TouchesWindow(zone, ActiveWindow);
                zone.SetNear(near);
                if (near) NearZoneCount++;
            }
        }

        private static bool TouchesWindow(ZonePlacement zone, Rect window)
        {
            var dx = Mathf.Max(window.xMin - zone.Center.x, 0f, zone.Center.x - window.xMax);
            var dy = Mathf.Max(window.yMin - zone.Center.y, 0f, zone.Center.y - window.yMax);
            return dx * dx + dy * dy <= zone.Effect.Radius * zone.Effect.Radius;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) return;
            var previous = Time;
            Time += deltaTime;
            _portalCooldown = Mathf.Max(0f, _portalCooldown - deltaTime);
            RefreshWindow();
            Relocate();
            TickBursts(previous, deltaTime);
            TickShrines(deltaTime);
            TickStrikes(previous);
            TickPlayer(deltaTime);
            TickEnemies(deltaTime);
        }

        // A new cycle starts while inactive: legacy pulses are invisible; prepared seals are dim before lighting up.
        private void Relocate()
        {
            foreach (var zone in _zones)
            {
                // Altars (cycling) and permanent zones never move; pulsing and burst zones jump when a new cycle begins.
                if (!zone.Effect.RelocatesBetweenCycles) continue;
                var cycle = zone.CycleAt(Time);
                if (cycle == zone.Cycle) continue;
                var random = new System.Random(unchecked(_seed * 31 + zone.Index * 7919 + cycle * 104729));
                // It reappears near the player (inside the active window) so the zones the player can see are the ones in play.
                var center = _rules.TryPick(zone.Effect, random, OthersOf(zone), null, out var picked, ActiveWindow) ? picked : zone.Center;
                zone.Relocate(center, cycle);
                zone.SetNear(TouchesWindow(zone, ActiveWindow));
            }
        }

        private IEnumerable<ZonePlacement> OthersOf(ZonePlacement zone)
        {
            foreach (var other in _zones)
                if (other != zone) yield return other;
        }

        /// <summary>Remaining seconds of the timed buff a burst zone gave the player (0 when none).</summary>
        public float BuffRemaining(ZoneEffectDefinition effect) => _buffs.TryGetValue(effect, out var remaining) ? remaining : 0f;

        /// <summary>Remaining seconds of the timed shield a shrine gave the player (0 when none).</summary>
        public float ShieldRemaining(ZoneEffectDefinition effect) => _shields.TryGetValue(effect, out var remaining) ? remaining : 0f;

        // A burst zone goes off the instant its telegraph ends: a player inside the radius then gets the timed buff.
        private void TickBursts(float previousTime, float deltaTime)
        {
            DecayTimers(_buffs, deltaTime);
            DecayTimers(_shields, deltaTime);
            if (!_player.IsAlive) return;
            foreach (var zone in _zones)
                if (zone.IsNear && zone.BurstFiresBetween(previousTime, Time) && zone.Contains(_player.Position))
                    _buffs[zone.Effect] = zone.Effect.PlayerBuffSeconds;
        }

        private void DecayTimers(Dictionary<ZoneEffectDefinition, float> timers, float deltaTime)
        {
            if (timers.Count == 0) return;
            _expiredBuffs.Clear();
            _expiredBuffs.AddRange(timers.Keys);
            foreach (var effect in _expiredBuffs)
            {
                var left = timers[effect] - deltaTime;
                if (left <= 0f) timers.Remove(effect); else timers[effect] = left;
            }
        }

        // A shrine fills while the player stands inside it, drains otherwise, and fires its reward once when full; then it
        // rests for its cooldown (which keeps running while the shrine is far away or the altar is off).
        private void TickShrines(float deltaTime)
        {
            foreach (var zone in _zones)
            {
                if (zone.Effect.Kind != ZoneEffectKind.Shrine) continue;
                var effect = zone.Effect;
                if (zone.ShrineCooldownRemaining > 0f)
                {
                    zone.SetShrineCooldown(zone.ShrineCooldownRemaining - deltaTime);
                    zone.SetCharge(0f);
                    continue;
                }
                if (!zone.IsNear || !_player.IsAlive) { zone.SetCharge(0f); continue; }
                var inside = zone.IsActive(Time) && zone.Contains(_player.Position);
                var next = zone.Charge + (inside ? deltaTime / effect.ShrineChargeSeconds : -deltaTime / effect.ShrineDecaySeconds);
                if (next < 1f) { zone.SetCharge(next); continue; }
                zone.SetCharge(0f);
                zone.SetShrineCooldown(effect.ShrineCooldownSeconds);
                FireShrine(zone);
            }
        }

        private void FireShrine(ZonePlacement zone)
        {
            var effect = zone.Effect;
            if (effect.RewardBuffSeconds > 0f) _buffs[effect] = effect.RewardBuffSeconds;
            if (effect.RewardShieldSeconds > 0f) _shields[effect] = effect.RewardShieldSeconds;
            if (effect.RewardHealFraction > 0f) _player.HealFraction(effect.RewardHealFraction);
            if (effect.RewardBlastDamage <= 0f) return;
            using var guard = PerfGuard.Measure("Zones.ShrineBlast", 2f);
            var count = _enemies.Refresh();
            var radiusSquared = effect.RewardBlastRadius * effect.RewardBlastRadius;
            for (var i = 0; i < count; i++)
                if ((_enemies.Position(i) - zone.Center).sqrMagnitude <= radiusSquared)
                    _enemies.Strike(i, effect.RewardBlastDamage, effect.Id);
        }

        // A strike altar's circles hit the instant their warning ends (only while the altar is on and near the player).
        private void TickStrikes(float previousTime)
        {
            foreach (var zone in _zones)
            {
                var effect = zone.Effect;
                if (effect.Kind != ZoneEffectKind.Strike || !zone.IsNear || !zone.IsActive(Time)) continue;
                if (!effect.StrikeFiresBetween(zone.PhaseSeconds, previousTime, Time, out var volley)) continue;
                var enemyCount = effect.StrikeEnemyDamage > 0f ? _enemies.Refresh() : 0;
                var radiusSquared = effect.StrikeRadius * effect.StrikeRadius;
                for (var circle = 0; circle < effect.StrikeCount; circle++)
                {
                    var center = effect.StrikeCenter(zone.Center, _seed, zone.Index, volley, circle);
                    if (effect.StrikePlayerDamage > 0f && _player.IsAlive && (_player.Position - center).sqrMagnitude <= radiusSquared)
                        _player.Hit(effect.StrikePlayerDamage, effect.Id);
                    for (var i = 0; i < enemyCount; i++)
                        if ((_enemies.Position(i) - center).sqrMagnitude <= radiusSquared)
                            _enemies.Strike(i, effect.StrikeEnemyDamage, effect.Id);
                }
            }
        }

        /// <summary>Fills <paramref name="circles"/> with the strike warnings and flashes to draw right now (cleared first).</summary>
        public void CollectStrikeCircles(List<StrikeCircle> circles)
        {
            circles.Clear();
            foreach (var zone in _zones)
            {
                var effect = zone.Effect;
                if (effect.Kind != ZoneEffectKind.Strike || !zone.IsNear || !zone.IsActive(Time)) continue;
                if (!effect.StrikeState(zone.PhaseSeconds, Time, out var telegraph, out var flash)) continue;
                var volley = Mathf.FloorToInt((Time + zone.PhaseSeconds) / effect.StrikePeriodSeconds);
                for (var circle = 0; circle < effect.StrikeCount; circle++)
                    circles.Add(new StrikeCircle(zone, effect.StrikeCenter(zone.Center, _seed, zone.Index, volley, circle),
                        effect.StrikeRadius, telegraph, flash));
            }
        }

        private void TickPlayer(float deltaTime)
        {
            PlayerActiveZoneCount = 0;
            if (!_player.IsAlive)
            {
                _buffs.Clear();
                _shields.Clear();
                foreach (var zone in _zones) zone.SetCharge(0f);
                ApplyModifier(0f, 0f, 0f, 0f, 0f);
                return;
            }
            float move = 0f, skill = 0f, action = 0f, regen = 0f, defense = 0f, damage = 0f;
            foreach (var pair in _buffs)
            {
                move += pair.Key.TimedBuffMovementBonus;
                skill += pair.Key.RewardSkillDamageBonus;
                action += pair.Key.RewardActionSpeedBonus;
            }
            foreach (var pair in _shields) defense += pair.Key.RewardIncomingDamageReduction;
            ZonePlacement portal = null;
            var position = _player.Position;
            // Charging altars fill while the player stands inside an active one and drain otherwise; their bonus is the charge.
            foreach (var zone in _zones)
            {
                if (zone.Effect.Kind != ZoneEffectKind.Charge) continue;
                var effect = zone.Effect;
                if (!zone.IsNear) { zone.SetCharge(0f); continue; }
                var inside = zone.IsActive(Time) && zone.Contains(position);
                zone.SetCharge(zone.Charge + (inside ? deltaTime / effect.ChargeSecondsToMax : -deltaTime / effect.ChargeDecaySeconds));
                if (inside) PlayerActiveZoneCount++;
                skill += effect.ChargeSkillDamageBonus * zone.Charge;
                action += effect.ChargeActionSpeedBonus * zone.Charge;
            }
            foreach (var zone in _zones)
            {
                // Bursts, charging altars, strike altars and shrines have their own ticks.
                var own = zone.Effect.Kind;
                if (own == ZoneEffectKind.SpeedBurst || own == ZoneEffectKind.Charge || own == ZoneEffectKind.Strike || own == ZoneEffectKind.Shrine) continue;
                if (!zone.IsNear || !zone.IsActive(Time) || !zone.Contains(position)) continue;
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
                    case ZoneEffectKind.Protection:
                        defense += effect.PlayerIncomingDamageReduction;
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
            ApplyModifier(move, skill, action, regen, defense);
            if (portal != null) Teleport(portal);
        }

        private void Teleport(ZonePlacement from)
        {
            if (from.PartnerIndex < 0 || from.PartnerIndex >= _zones.Count) return;
            var partner = _zones[from.PartnerIndex];
            // Both ends must be ready; the remote end need not intersect this camera's active window.
            if (!partner.Effect.IsActive(partner.PhaseSeconds, Time)) return;
            var toArenaCenter = -partner.Center;
            var direction = toArenaCenter.sqrMagnitude > 1e-6f ? toArenaCenter.normalized : Vector2.up;
            _player.TeleportTo(partner.Center + direction * (partner.Effect.Radius + partner.Effect.PortalExitDistance));
            from.RecordApplication(Time);
            partner.RecordApplication(Time);
            _portalCooldown = from.Effect.PortalCooldownSeconds;
        }

        private void ApplyModifier(float move, float skill, float action, float regen, float defense)
        {
            var zero = Mathf.Abs(move) < ModifierEpsilon && Mathf.Abs(skill) < ModifierEpsilon &&
                       Mathf.Abs(action) < ModifierEpsilon && Mathf.Abs(regen) < ModifierEpsilon && Mathf.Abs(defense) < ModifierEpsilon;
            if (zero)
            {
                if (_modifierSet) _player.RemoveStatModifier(ModifierKey);
                _modifierSet = false;
                _move = _skill = _action = _regen = _defense = 0f;
                return;
            }
            if (_modifierSet && Mathf.Approximately(move, _move) && Mathf.Approximately(skill, _skill) &&
                Mathf.Approximately(action, _action) && Mathf.Approximately(regen, _regen) && Mathf.Approximately(defense, _defense)) return;
            _player.SetStatModifier(ModifierKey, new CharacterStatModifier(movementSpeedMultiplierBonus: move,
                activeSkillDamageMultiplierBonus: skill, actionSpeedBonus: action, healthRegenerationPerSecondBonus: regen,
                incomingDamageReductionBonus: defense));
            _modifierSet = true;
            _move = move; _skill = skill; _action = action; _regen = regen; _defense = defense;
        }

        private void TickEnemies(float deltaTime)
        {
            var touchesEnemies = false;
            foreach (var zone in _zones)
                // Area-only slows must clear even when the last zone switches off or leaves the window.
                if (zone.Effect.Kind == ZoneEffectKind.Slow && zone.Effect.EnemySlowSeconds == 0f)
                { touchesEnemies = true; break; }
                else
                if (zone.IsNear && zone.IsActive(Time) && AffectsEnemies(zone.Effect)) { touchesEnemies = true; break; }
            if (!touchesEnemies) return;
            // Cost scales with living enemies times zones (DECISION-0008).
            using var guard = PerfGuard.Measure("Zones.TickEnemies", 2f);
            var count = _enemies.Refresh();
            for (var i = 0; i < count; i++)
            {
                var position = _enemies.Position(i);
                foreach (var zone in _zones)
                {
                    if (!zone.IsNear || !zone.IsActive(Time) || !AffectsEnemies(zone.Effect) || !zone.Contains(position)) continue;
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
            (_enemies as IDisposable)?.Dispose();
            if (_modifierSet) _player.RemoveStatModifier(ModifierKey);
            _modifierSet = false;
            _buffs.Clear();
            _shields.Clear();
        }
    }
}
