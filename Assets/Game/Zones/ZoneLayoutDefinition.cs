using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Zones.Json;

namespace Game.Zones
{
    /// <summary>
    /// Per-run zone layout: which effects exist, how many zones of each are scattered over the arena and the clearances
    /// they keep. Distances are world units.
    /// </summary>
    public sealed class ZoneLayoutDefinition
    {
        public float EdgeMargin { get; }
        public float StartClearRadius { get; }
        public float MinGap { get; }
        public float ObstacleClearance { get; }
        public int PlacementAttempts { get; }
        public int MaxRestarts { get; }
        public int ReferenceSeed { get; }
        public IReadOnlyDictionary<ContentId, ZoneEffectDefinition> Effects { get; }
        /// <summary>One entry per zone to place, largest radius first; portals are listed in consecutive pairs.</summary>
        public IReadOnlyList<ZoneEffectDefinition> Zones { get; }

        public ZoneLayoutDefinition(ZoneLayoutData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            EdgeMargin = Required(data.EdgeMargin, "zoneLayout.edgeMargin");
            StartClearRadius = Required(data.StartClearRadius, "zoneLayout.startClearRadius");
            MinGap = Required(data.MinGap, "zoneLayout.minGap");
            ObstacleClearance = Required(data.ObstacleClearance, "zoneLayout.obstacleClearance");
            PlacementAttempts = data.PlacementAttempts ?? throw new ArgumentException("zoneLayout.placementAttempts is required.");
            MaxRestarts = data.MaxRestarts ?? throw new ArgumentException("zoneLayout.maxRestarts is required.");
            ReferenceSeed = data.ReferenceSeed ?? throw new ArgumentException("zoneLayout.referenceSeed is required.");
            NumericValidation.ValidateNonNegative(EdgeMargin, nameof(EdgeMargin));
            NumericValidation.ValidateNonNegative(StartClearRadius, nameof(StartClearRadius));
            NumericValidation.ValidateNonNegative(MinGap, nameof(MinGap));
            NumericValidation.ValidateNonNegative(ObstacleClearance, nameof(ObstacleClearance));
            NumericValidation.ValidateCount(PlacementAttempts, nameof(PlacementAttempts));
            NumericValidation.ValidateNonNegative(MaxRestarts, nameof(MaxRestarts));

            var effects = new Dictionary<ContentId, ZoneEffectDefinition>();
            foreach (var item in data.Effects ?? throw new ArgumentException("zoneLayout.effects is required."))
            {
                var effect = new ZoneEffectDefinition(item);
                if (!effects.TryAdd(effect.Id, effect)) throw new ArgumentException($"Duplicate zone effect '{effect.Id}'.");
            }
            Effects = effects;

            var zones = new List<ZoneEffectDefinition>();
            var used = new HashSet<ContentId>();
            foreach (var entry in data.Zones ?? throw new ArgumentException("zoneLayout.zones is required."))
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.EffectId)) throw new ArgumentException("Zone entries need an effectId.");
                var id = new ContentId(entry.EffectId);
                if (!effects.TryGetValue(id, out var effect)) throw new ArgumentException($"Unknown zone effect '{id}'.");
                if (!used.Add(id)) throw new ArgumentException($"Zone effect '{id}' is listed twice.");
                var count = entry.Count ?? throw new ArgumentException($"Zone entry '{id}' requires a count.");
                NumericValidation.ValidateCount(count, "zone count");
                if (effect.Kind == ZoneEffectKind.Portal && count % 2 != 0)
                    throw new ArgumentException($"Portal effect '{id}' needs an even count (zones are paired).");
                for (var i = 0; i < count; i++) zones.Add(effect);
            }
            if (zones.Count == 0) throw new ArgumentException("A zone layout needs at least one zone.");
            // Largest first packs better; the stable order keeps portal partners adjacent.
            Zones = zones.Select((effect, index) => (effect, index)).OrderByDescending(pair => pair.effect.Radius)
                .ThenBy(pair => pair.index).Select(pair => pair.effect).ToList().AsReadOnly();
        }

        private static float Required(float? value, string name) => value ?? throw new ArgumentException(name + " is required.");
    }
}
