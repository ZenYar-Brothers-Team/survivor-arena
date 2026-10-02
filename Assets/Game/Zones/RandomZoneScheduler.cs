using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Zones
{
    /// <summary>
    /// DECISION-0142 / DECISION-0151: independent random chains. When a chain's delay ends it picks any effect (several chains may
    /// pick the same one) and shows it within a screen-width radius of the player; the entire visible life occupies the chain.
    /// A scheduled portal is a pair of linked placements that lives alone on its own slower chain, outside the general ones.
    /// </summary>
    public sealed class RandomZoneScheduler : IDisposable
    {
        private readonly RandomZoneScheduleDefinition _config;
        private readonly ZonePlacementRules _rules;
        private readonly IReadOnlyList<ZonePlacement> _zones;
        private readonly List<ZoneEffectDefinition> _effects = new List<ZoneEffectDefinition>();
        private readonly List<ZonePlacement> _pool = new List<ZonePlacement>();
        private readonly List<ZoneEffectDefinition> _pickable = new List<ZoneEffectDefinition>();
        private readonly List<ZonePlacement> _occupied = new List<ZonePlacement>();
        private readonly ZonePlacement[] _chains;
        private readonly float[] _next;
        // Index of the dedicated portal chain (last), or -1: portals never compete with the general chains.
        private readonly int _portalChain;
        private readonly System.Random _random;

        public RandomZoneScheduler(RandomZoneScheduleDefinition config, ZonePlacementRules rules,
            IReadOnlyList<ZonePlacement> zones, int seed)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _zones = zones ?? throw new ArgumentNullException(nameof(zones));
            _random = new System.Random(unchecked(seed * 7919 + 6151));
            _portalChain = config.HasPortalChain ? config.Chains : -1;
            var lanes = config.Chains + (config.HasPortalChain ? 1 : 0);
            _chains = new ZonePlacement[lanes]; _next = new float[lanes];
            foreach (var zone in zones)
                if (zone.Effect.RelocatesBetweenCycles)
                {
                    zone.ManageOccurrences(); _pool.Add(zone);
                    if (!_effects.Contains(zone.Effect)) _effects.Add(zone.Effect);
                }
            if (_pool.Count < config.Chains) throw new ArgumentException("Random schedule needs at least one placement per chain.");
            for (var i = 0; i < _chains.Length; i++) _next[i] = Delay(i);
        }

        private float Delay(int chain) => chain == _portalChain
            ? ZonePlacementRules.Range(_random, _config.PortalIntervalMinSeconds.Value, _config.PortalIntervalMaxSeconds.Value)
            : ZonePlacementRules.Range(_random, _config.IntervalMinSeconds, _config.IntervalMaxSeconds);

        /// <summary>
        /// Spawns only around the player's current position and screen size; never catches up missed appearances.
        /// <paramref name="view"/> supplies the screen width (spawn radius) and height (portal pair distance).
        /// </summary>
        public void Tick(float runSeconds, Rect view, Vector2 playerPosition)
        {
            using var guard = Game.Diagnostics.PerfGuard.Measure("Zones.RandomSchedule", 2f);
            for (var i = 0; i < _chains.Length; i++)
            {
                if (_chains[i] != null || runSeconds < _next[i]) continue;
                // A missing camera is not permission to scatter zones across the arena.
                if (view.width <= 0f || view.height <= 0f) { _next[i] = runSeconds + Delay(i); continue; }
                if (!TrySpawn(runSeconds, view, playerPosition, i == _portalChain, out var started)) { _next[i] = runSeconds + Delay(i); continue; }
                _chains[i] = started;
            }
        }

        private bool TrySpawn(float runSeconds, Rect view, Vector2 playerPosition, bool portalLane, out ZonePlacement started)
        {
            started = null;
            _pickable.Clear();
            foreach (var effect in _effects)
                if (effect.IsScheduledPortalPair == portalLane && FindFree(effect) != null) _pickable.Add(effect);
            if (_pickable.Count == 0) return false;
            _occupied.Clear();
            foreach (var zone in _zones) if (zone.IsPresent) _occupied.Add(zone);
            // Each effect is equally likely, regardless of how many spare placements it keeps.
            var picked = _pickable[_random.Next(_pickable.Count)];
            var first = FindFree(picked);
            var radius = ZonePlacementRules.Range(_random, picked.MinRadius, picked.Radius);
            var spawnRadius = view.width * _config.SpawnRadiusScreenWidths;
            if (!_rules.TryPick(picked, _random, _occupied, null, out var center, null, radius, allowStartOverlap: true, allowZoneOverlap: true,
                    aroundCenter: playerPosition, aroundRadius: spawnRadius)) return false;
            if (!picked.IsScheduledPortalPair)
            {
                first.BeginOccurrence(center, radius, runSeconds);
                started = first;
                return true;
            }
            var second = _zones[first.PartnerIndex];
            var otherRadius = ZonePlacementRules.Range(_random, picked.MinRadius, picked.Radius);
            _occupied.Add(first); // first is not yet present, but the second end must keep clear of it
            first.BeginOccurrence(center, radius, runSeconds);
            if (!_rules.TryPick(picked, _random, _occupied, null, out var otherCenter, null, otherRadius, allowStartOverlap: true, allowZoneOverlap: true,
                    ringCenter: center, ringDistance: view.height * picked.PortalPairScreenHeights))
            {
                first.EndOccurrence();
                return false;
            }
            second.BeginOccurrence(otherCenter, otherRadius, runSeconds);
            started = first;
            return true;
        }

        // The first free placement of an effect; for a portal, the first end whose partner is also free.
        private ZonePlacement FindFree(ZoneEffectDefinition effect)
        {
            foreach (var zone in _pool)
            {
                if (zone.Effect != effect || zone.IsPresent) continue;
                if (!effect.IsScheduledPortalPair) return zone;
                if (zone.PartnerIndex > zone.Index && !_zones[zone.PartnerIndex].IsPresent) return zone;
            }
            return null;
        }

        /// <summary>End after gameplay processes this tick, so a large tick crossing a burst still applies it once.</summary>
        public bool FinishTick(float runSeconds)
        {
            var ended = false;
            for (var i = 0; i < _chains.Length; i++)
            {
                var zone = _chains[i];
                if (zone == null || runSeconds < zone.OccurrenceEndSeconds.Value) continue;
                zone.EndOccurrence();
                if (zone.Effect.IsScheduledPortalPair) _zones[zone.PartnerIndex].EndOccurrence();
                _chains[i] = null; _next[i] = runSeconds + Delay(i);
                ended = true;
            }
            return ended;
        }

        public void Dispose()
        {
            foreach (var zone in _pool) zone.EndOccurrence();
            Array.Clear(_chains, 0, _chains.Length);
        }
    }
}
