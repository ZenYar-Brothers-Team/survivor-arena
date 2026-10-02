using System;
using System.Collections.Generic;
using Game.Content;
using UnityEngine;

namespace Game.Zones
{
    /// <summary>
    /// Scatters a field's effect zones for one run under <see cref="ZonePlacementRules"/>. Portals are paired in order.
    /// The same seed, arena and obstacles always give the same layout; a seed that cannot fit every zone is retried with a
    /// derived seed.
    /// </summary>
    public static class ZoneLayoutGenerator
    {
        private const int RestartSeedStep = 6151;

        public static IReadOnlyList<ZonePlacement> Generate(ZoneLayoutDefinition layout, float sideLength, Vector2 start,
            IReadOnlyList<IReadOnlyList<Vector2>> obstacles, int seed, Vector2? screenSize = null)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            NumericValidation.ValidatePositive(sideLength, nameof(sideLength));
            using var guard = Game.Diagnostics.PerfGuard.Measure("Zones.Layout", 500f);
            var rules = new ZonePlacementRules(layout, sideLength, start, obstacles, screenSize);
            for (var restart = 0; restart <= layout.MaxRestarts; restart++)
            {
                var result = TryGenerate(layout, rules, unchecked(seed + restart * RestartSeedStep));
                if (result != null) return result;
            }
            throw new InvalidOperationException("Zone layout does not fit the arena; reduce zone sizes or counts.");
        }

        private static IReadOnlyList<ZonePlacement> TryGenerate(ZoneLayoutDefinition layout, ZonePlacementRules rules, int seed)
        {
            var random = new System.Random(seed);
            var placed = new List<ZonePlacement>();
            for (var index = 0; index < layout.Zones.Count; index++)
            {
                var effect = layout.Zones[index];
                var radius = effect.MinRadius < effect.Radius ? ZonePlacementRules.Range(random, effect.MinRadius, effect.Radius) : effect.Radius;
                if (layout.RandomSchedule != null && effect.RelocatesBetweenCycles)
                {
                    var dormant = new ZonePlacement(index, effect, Vector2.zero, radius: radius);
                    dormant.ManageOccurrences(); placed.Add(dormant); continue;
                }
                var pairFirst = effect.Kind == ZoneEffectKind.Portal && placed.Count > 0 &&
                                placed[placed.Count - 1].Effect == effect && placed[placed.Count - 1].PartnerIndex < 0
                    ? placed[placed.Count - 1] : null;
                if (!rules.TryPick(effect, random, placed.FindAll(z => z.IsPresent), pairFirst, out var center, occurrenceRadius: radius)) return null;
                // A pulsing zone starts at a random point of its cycle so zones do not all blink together.
                var phase = pairFirst != null ? pairFirst.PhaseSeconds :
                    effect.PhaseRange > 0f ? ZonePlacementRules.Range(random, 0f, effect.PhaseRange) : 0f;
                var zone = new ZonePlacement(index, effect, center, phase, radius);
                if (pairFirst != null)
                {
                    pairFirst.LinkPartner(zone.Index);
                    zone.LinkPartner(pairFirst.Index);
                }
                placed.Add(zone);
            }
            return placed.AsReadOnly();
        }
    }
}
