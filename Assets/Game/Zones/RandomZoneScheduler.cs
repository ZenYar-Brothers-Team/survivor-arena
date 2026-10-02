using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Zones
{
    /// <summary>DECISION-0142: independent random chains reuse dormant placements; the entire visible life occupies a slot.</summary>
    public sealed class RandomZoneScheduler : IDisposable
    {
        private readonly RandomZoneScheduleDefinition _config;
        private readonly ZonePlacementRules _rules;
        private readonly IReadOnlyList<ZonePlacement> _zones;
        private readonly List<ZonePlacement> _pool = new List<ZonePlacement>();
        private readonly List<ZonePlacement> _available = new List<ZonePlacement>();
        private readonly List<ZonePlacement> _occupied = new List<ZonePlacement>();
        private readonly ZonePlacement[] _chains;
        private readonly float[] _next;
        private readonly System.Random _random;

        public RandomZoneScheduler(RandomZoneScheduleDefinition config, ZonePlacementRules rules,
            IReadOnlyList<ZonePlacement> zones, int seed)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _zones = zones ?? throw new ArgumentNullException(nameof(zones));
            _random = new System.Random(unchecked(seed * 7919 + 6151));
            _chains = new ZonePlacement[config.Chains]; _next = new float[config.Chains];
            foreach (var zone in zones)
                if (zone.Effect.RelocatesBetweenCycles) { zone.ManageOccurrences(); _pool.Add(zone); }
            if (_pool.Count < config.Chains) throw new ArgumentException("Random schedule needs at least one placement per chain.");
            for (var i = 0; i < _chains.Length; i++) _next[i] = Delay();
        }

        private float Delay() => ZonePlacementRules.Range(_random, _config.IntervalMinSeconds, _config.IntervalMaxSeconds);

        /// <summary>Spawns only at the current camera, never catches up missed appearances using old/offscreen centers.</summary>
        public void Tick(float runSeconds, Rect view)
        {
            using var guard = Game.Diagnostics.PerfGuard.Measure("Zones.RandomSchedule", 2f);
            for (var i = 0; i < _chains.Length; i++)
            {
                if (_chains[i] != null || runSeconds < _next[i]) continue;
                // A missing camera is not permission to scatter zones across the arena.
                if (view.width <= 0f || view.height <= 0f) { _next[i] = runSeconds + Delay(); continue; }
                _available.Clear(); _occupied.Clear();
                foreach (var zone in _pool) if (!zone.IsPresent) _available.Add(zone);
                foreach (var zone in _zones) if (zone.IsPresent) _occupied.Add(zone);
                if (_available.Count == 0) { _next[i] = runSeconds + Delay(); continue; }
                var picked = _available[_random.Next(_available.Count)];
                var effect = picked.Effect;
                var radius = ZonePlacementRules.Range(_random, effect.MinRadius, effect.Radius);
                var margin = _config.ScreenMarginWorldUnits;
                var window = new Rect(view.xMin - margin, view.yMin - margin, view.width + 2f * margin, view.height + 2f * margin);
                if (!_rules.TryPick(effect, _random, _occupied, null, out var center, window, radius, allowStartOverlap: true))
                { _next[i] = runSeconds + Delay(); continue; }
                picked.BeginOccurrence(center, radius, runSeconds);
                _chains[i] = picked;
            }
        }

        /// <summary>End after gameplay processes this tick, so a large tick crossing a burst still applies it once.</summary>
        public bool FinishTick(float runSeconds)
        {
            var ended = false;
            for (var i = 0; i < _chains.Length; i++)
            {
                var zone = _chains[i];
                if (zone == null || runSeconds < zone.OccurrenceEndSeconds.Value) continue;
                zone.EndOccurrence(); _chains[i] = null; _next[i] = runSeconds + Delay();
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
