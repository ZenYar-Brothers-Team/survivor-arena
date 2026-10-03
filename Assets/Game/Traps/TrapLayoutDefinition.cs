using System;
using System.Collections.Generic;
using Game.Content;
using Game.Traps.Json;

namespace Game.Traps
{
    /// <summary>
    /// Per-run trap layout of a field (DECISION-0156): turrets that fire projectiles at the player and barrels, scattered
    /// randomly each run, in their own budget separate from the field's obstacles. Distances are world units.
    /// </summary>
    public sealed class TrapLayoutDefinition
    {
        public int ReferenceSeed { get; }
        /// <summary>Activation, rage and projectile cutoff radius around the player, in screen widths.</summary>
        public float RadiusScreenWidths { get; }
        public float PlayerHitRadius { get; }
        public int MaxActiveProjectiles { get; }
        public float EdgeMargin { get; }
        public float StartClearRadius { get; }
        public float MinGap { get; }
        public float ObstacleClearance { get; }
        public float ForwardClearance { get; }
        public int PlacementAttempts { get; }
        public TrapRageDefinition Rage { get; }
        public IReadOnlyDictionary<string, TrapProjectileDefinition> Projectiles { get; }
        public IReadOnlyList<TrapTypeDefinition> Types { get; }
        public TrapBarrelDefinition Barrels { get; }
        public IReadOnlyDictionary<string, TrapModelDefinition> Models { get; }
        public IReadOnlyList<TrapStartDefinition> StartTraps { get; }
        /// <summary>Screen-based scatter, or null for the fixed per-type counts.</summary>
        public TrapDensityDefinition Density { get; }
        public IReadOnlyList<TrapStartBarrelDefinition> StartBarrels { get; }
        /// <summary>Sprite ids by key (projectile visual keys and "barrel"); empty = placeholder shapes.</summary>
        public IReadOnlyDictionary<string, string> Sprites { get; }
        public int TotalTrapCount { get; }

        public TrapLayoutDefinition(TrapLayoutData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            ReferenceSeed = data.ReferenceSeed ?? throw new ArgumentException("trapLayout.referenceSeed is required.");
            RadiusScreenWidths = data.RadiusScreenWidths ?? throw new ArgumentException("trapLayout.radiusScreenWidths is required.");
            PlayerHitRadius = data.PlayerHitRadius ?? throw new ArgumentException("trapLayout.playerHitRadius is required.");
            MaxActiveProjectiles = data.MaxActiveProjectiles ?? throw new ArgumentException("trapLayout.maxActiveProjectiles is required.");
            EdgeMargin = data.EdgeMargin ?? throw new ArgumentException("trapLayout.edgeMargin is required.");
            StartClearRadius = data.StartClearRadius ?? throw new ArgumentException("trapLayout.startClearRadius is required.");
            MinGap = data.MinGap ?? throw new ArgumentException("trapLayout.minGap is required.");
            ObstacleClearance = data.ObstacleClearance ?? throw new ArgumentException("trapLayout.obstacleClearance is required.");
            ForwardClearance = data.ForwardClearance ?? throw new ArgumentException("trapLayout.forwardClearance is required.");
            PlacementAttempts = data.PlacementAttempts ?? throw new ArgumentException("trapLayout.placementAttempts is required.");
            NumericValidation.ValidatePositive(RadiusScreenWidths, nameof(RadiusScreenWidths));
            NumericValidation.ValidatePositive(PlayerHitRadius, nameof(PlayerHitRadius));
            NumericValidation.ValidateCount(MaxActiveProjectiles, nameof(MaxActiveProjectiles));
            NumericValidation.ValidateNonNegative(EdgeMargin, nameof(EdgeMargin));
            NumericValidation.ValidateNonNegative(StartClearRadius, nameof(StartClearRadius));
            NumericValidation.ValidateNonNegative(MinGap, nameof(MinGap));
            NumericValidation.ValidateNonNegative(ObstacleClearance, nameof(ObstacleClearance));
            NumericValidation.ValidateNonNegative(ForwardClearance, nameof(ForwardClearance));
            NumericValidation.ValidateCount(PlacementAttempts, nameof(PlacementAttempts));
            Rage = new TrapRageDefinition(data.Rage ?? throw new ArgumentException("trapLayout.rage is required."));

            var projectiles = new Dictionary<string, TrapProjectileDefinition>(StringComparer.Ordinal);
            foreach (var item in data.Projectiles ?? throw new ArgumentException("trapLayout.projectiles is required."))
            {
                var projectile = new TrapProjectileDefinition(item);
                if (!projectiles.TryAdd(projectile.Id, projectile))
                    throw new ArgumentException($"Duplicate trap projectile '{projectile.Id}'.");
            }
            Projectiles = projectiles;

            var types = new List<TrapTypeDefinition>();
            var ids = new HashSet<ContentId>();
            var referenced = new HashSet<string>(StringComparer.Ordinal);
            var total = 0;
            foreach (var item in data.Types ?? throw new ArgumentException("trapLayout.types is required."))
            {
                var type = new TrapTypeDefinition(item, projectiles);
                if (!ids.Add(type.Id)) throw new ArgumentException($"Duplicate trap type '{type.Id}'.");
                foreach (var shot in type.Shots) referenced.Add(shot.Projectile.Id);
                total += type.Count;
                types.Add(type);
            }
            if (types.Count == 0) throw new ArgumentException("A trap layout needs at least one trap type.");
            foreach (var id in projectiles.Keys)
                if (!referenced.Contains(id)) throw new ArgumentException($"Trap projectile '{id}' is not used by any trap type.");
            Types = types.AsReadOnly();
            TotalTrapCount = total;
            var models = new Dictionary<string, TrapModelDefinition>(StringComparer.Ordinal);
            foreach (var item in data.Models ?? Array.Empty<TrapModelData>())
            {
                var model = new TrapModelDefinition(item);
                if (!models.TryAdd(model.Key, model)) throw new ArgumentException($"Duplicate trap model '{model.Key}'.");
            }
            Models = models;
            Density = data.Density == null ? null : new TrapDensityDefinition(data.Density);
            var starts = new List<TrapStartDefinition>();
            var startCounts = new Dictionary<TrapTypeDefinition, int>();
            foreach (var item in data.StartTraps ?? Array.Empty<TrapStartData>())
            {
                var start = new TrapStartDefinition(item ?? throw new ArgumentException("startTraps cannot contain null."), Types, models);
                startCounts[start.Type] = startCounts.TryGetValue(start.Type, out var existing) ? existing + 1 : 1;
                if (Density == null && startCounts[start.Type] > start.Type.Count)
                    throw new ArgumentException($"More start traps than the count of '{start.Type.Id}'.");
                starts.Add(start);
            }
            StartTraps = starts.AsReadOnly();
            foreach (var type in types)
                if (type.ModelKey != null && !models.ContainsKey(type.ModelKey))
                    throw new ArgumentException($"Trap type '{type.Id}' references unknown model '{type.ModelKey}'.");
            var sprites = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var pair in data.Sprites ?? new Dictionary<string, string>())
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || string.IsNullOrWhiteSpace(pair.Value))
                    throw new ArgumentException("Trap sprite entries need a key and an id.");
                sprites.Add(pair.Key, pair.Value);
            }
            if (sprites.Count > 0)
            {
                foreach (var projectile in projectiles.Values)
                    if (!sprites.ContainsKey(projectile.Visual))
                        throw new ArgumentException($"No sprite for projectile visual '{projectile.Visual}'.");
                if (!sprites.ContainsKey("barrel")) throw new ArgumentException("No sprite for the barrel.");
            }
            Sprites = sprites;
            var startBarrels = new List<TrapStartBarrelDefinition>();
            foreach (var item in data.StartBarrels ?? Array.Empty<TrapStartBarrelData>())
                startBarrels.Add(new TrapStartBarrelDefinition(item ?? throw new ArgumentException("startBarrels cannot contain null.")));
            StartBarrels = startBarrels.AsReadOnly();
            Barrels = new TrapBarrelDefinition(data.Barrels ?? throw new ArgumentException("trapLayout.barrels is required."));
            if (StartBarrels.Count > Barrels.Count) throw new ArgumentException("More start barrels than barrels.count.");
        }
    }
}
