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
            IReadOnlyList<IReadOnlyList<Vector2>> obstacles, int seed)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            NumericValidation.ValidatePositive(sideLength, nameof(sideLength));
            var rules = new ZonePlacementRules(layout, sideLength, start, obstacles);
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
                var pairFirst = effect.Kind == ZoneEffectKind.Portal && placed.Count > 0 &&
                                placed[placed.Count - 1].Effect == effect && placed[placed.Count - 1].PartnerIndex < 0
                    ? placed[placed.Count - 1] : null;
                if (!rules.TryPick(effect, random, placed, pairFirst, out var center)) return null;
                // A pulsing zone starts at a random point of its cycle so zones do not all blink together.
                var phase = effect.PhaseRange > 0f ? ZonePlacementRules.Range(random, 0f, effect.PhaseRange) : 0f;
                var zone = new ZonePlacement(index, effect, center, phase);
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
