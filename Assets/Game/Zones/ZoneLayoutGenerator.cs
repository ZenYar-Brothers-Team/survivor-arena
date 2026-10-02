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
                    dormant.ManageOccurrences();
                    // Scheduled portals are listed in consecutive pairs; each pair appears and disappears together.
                    if (effect.IsScheduledPortalPair && placed.Count > 0 && placed[placed.Count - 1].Effect == effect &&
                        placed[placed.Count - 1].PartnerIndex < 0)
                    {
                        placed[placed.Count - 1].LinkPartner(dormant.Index); dormant.LinkPartner(placed[placed.Count - 1].Index);
                    }
                    placed.Add(dormant); continue;
                }
                var pairFirst = effect.Kind == ZoneEffectKind.Portal && !effect.IsBurstPortal && placed.Count > 0 &&
                                placed[placed.Count - 1].Effect == effect && placed[placed.Count - 1].PartnerIndex < 0
                    ? placed[placed.Count - 1] : null;
                var present = placed.FindAll(z => z.IsPresent);
                if (!rules.TryPick(effect, random, present, pairFirst, out var center, occurrenceRadius: radius,
                        preference: MixPreference(layout, effect, present), candidates: layout.PolarityMixCandidates)) return null;
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

        // Altars of one polarity prefer the candidate farthest from the nearest placed altar of the same polarity, so positive and
        // negative altars each spread over the whole arena and mix evenly instead of clumping by chance.
        private static Func<Vector2, float> MixPreference(ZoneLayoutDefinition layout, ZoneEffectDefinition effect, List<ZonePlacement> present)
        {
            if (layout.PolarityMixCandidates < 2 || !effect.IsAltar || !effect.Polarity.HasValue) return null;
            var same = present.FindAll(z => z.Effect.IsAltar && z.Effect.Polarity == effect.Polarity);
            if (same.Count == 0) return null;
            return candidate =>
            {
                var nearest = float.MaxValue;
                foreach (var other in same) nearest = Mathf.Min(nearest, Vector2.Distance(other.Center, candidate));
                return nearest;
            };
        }
    }
}
