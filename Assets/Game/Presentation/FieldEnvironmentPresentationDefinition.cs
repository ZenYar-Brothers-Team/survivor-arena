using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Presentation.Json;

namespace Game.Presentation
{
    public sealed class FieldEnvironmentPresentationDefinition : IContentDefinition, IReferencesContent
    {
        public ContentId Id { get; }
        public ContentId EnvironmentId { get; }
        public ContentRef<SpriteDefinition> Ground { get; }
        public ContentRef<SpriteDefinition> Fence { get; }
        public ContentRef<SpriteDefinition> Obstacle { get; }
        public ContentRef<SpriteDefinition> Bush { get; }
        public ContentRef<SpriteDefinition> Grass { get; }
        public ContentRef<SpriteDefinition> Column { get; }
        public ContentRef<SpriteDefinition> Barrel { get; }
        public ContentRef<SpriteDefinition> Rock { get; }
        public ContentRef<SpriteDefinition> Shrine { get; }
        public float ShrineChance { get; }
        public string ObstacleName { get; }
        public float ObstacleScale { get; }
        public float DecorationSpacing { get; }
        public float DecorationJitter { get; }
        public float DecorationChance { get; }
        public float BushChance { get; }
        public float DecorationMargin { get; }
        public float SafeRadius { get; }
        public float GrassScaleMin { get; }
        public float GrassScaleMax { get; }
        public float BushScaleMin { get; }
        public float BushScaleMax { get; }
        public int Seed { get; }
        public int ObstacleSeed { get; }
        public int InteriorObstacleCount { get; }
        public int NearObstacleCount { get; }
        public int ObstaclePlacementAttempts { get; }
        public float NearObstacleRadius { get; }
        public float FenceChance { get; }
        public float ObstacleSeparation { get; }
        public float FenceColliderWidth { get; }
        public float FenceColliderHeight { get; }
        public float StumpColliderRadius { get; }
        /// <summary>Authored obstacles; empty = layout or seeded random placement (fixture arena).</summary>
        public IReadOnlyList<FieldObstacleDefinition> ExplicitObstacles { get; }
        /// <summary>Per-run pattern layout (DECISION-0068); null = authored or fixture random obstacles.</summary>
        public FieldObstacleLayoutDefinition ObstacleLayout { get; }
        /// <summary>Per-run blob layout (field geometry study); null = no blobs.</summary>
        public FieldBlobLayoutDefinition BlobLayout { get; }
        public FieldRoadLayoutDefinition RoadLayout { get; }
        /// <summary>Per-run circular platforms over a damaging void (field geometry study); null = none.</summary>
        public FieldPlatformLayoutDefinition PlatformLayout { get; }
        public IReadOnlyList<FieldRoadLayout> RoadFallbackLayouts { get; }
        /// <summary>Arena side override in world units; null = the shared fixture arena.</summary>
        public float? ArenaSideLength { get; }
        /// <summary>Per-run effect zones; null = none.</summary>
        public Game.Zones.ZoneLayoutDefinition ZoneLayout { get; }
        public AltarPresentationProfile AltarPresentation { get; }

        public FieldEnvironmentPresentationDefinition(FieldEnvironmentPresentationData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            Id = data.Id;
            EnvironmentId = data.EnvironmentId;
            Ground = new ContentRef<SpriteDefinition>(data.GroundVisualId);
            Fence = new ContentRef<SpriteDefinition>(data.FenceVisualId);
            Obstacle = new ContentRef<SpriteDefinition>(data.ObstacleVisualId);
            Bush = new ContentRef<SpriteDefinition>(data.BushVisualId);
            Grass = new ContentRef<SpriteDefinition>(data.GrassVisualId);
            if (!string.IsNullOrWhiteSpace(data.ColumnVisualId)) Column = new ContentRef<SpriteDefinition>(data.ColumnVisualId);
            if (!string.IsNullOrWhiteSpace(data.BarrelVisualId)) Barrel = new ContentRef<SpriteDefinition>(data.BarrelVisualId);
            if (!string.IsNullOrWhiteSpace(data.RockVisualId)) Rock = new ContentRef<SpriteDefinition>(data.RockVisualId);
            if (!string.IsNullOrWhiteSpace(data.ShrineVisualId)) Shrine = new ContentRef<SpriteDefinition>(data.ShrineVisualId);
            ShrineChance = data.ShrineChance ?? 0f;
            ObstacleName = data.ObstacleName;
            ObstacleScale = Required(data.ObstacleScale, nameof(data.ObstacleScale));
            DecorationSpacing = Required(data.DecorationSpacing, nameof(data.DecorationSpacing));
            DecorationJitter = Required(data.DecorationJitter, nameof(data.DecorationJitter));
            DecorationChance = Required(data.DecorationChance, nameof(data.DecorationChance));
            BushChance = Required(data.BushChance, nameof(data.BushChance));
            DecorationMargin = Required(data.DecorationMargin, nameof(data.DecorationMargin));
            SafeRadius = Required(data.SafeRadius, nameof(data.SafeRadius));
            GrassScaleMin = Required(data.GrassScaleMin, nameof(data.GrassScaleMin));
            GrassScaleMax = Required(data.GrassScaleMax, nameof(data.GrassScaleMax));
            BushScaleMin = Required(data.BushScaleMin, nameof(data.BushScaleMin));
            BushScaleMax = Required(data.BushScaleMax, nameof(data.BushScaleMax));
            Seed = data.Seed ?? throw new ArgumentException("seed is required.");
            ObstacleSeed = data.ObstacleSeed ?? throw new ArgumentException("obstacleSeed is required.");
            InteriorObstacleCount = data.InteriorObstacleCount ?? throw new ArgumentException("interiorObstacleCount is required.");
            NearObstacleCount = data.NearObstacleCount ?? throw new ArgumentException("nearObstacleCount is required.");
            ObstaclePlacementAttempts = data.ObstaclePlacementAttempts ?? throw new ArgumentException("obstaclePlacementAttempts is required.");
            NearObstacleRadius = Required(data.NearObstacleRadius, nameof(data.NearObstacleRadius));
            FenceChance = Required(data.FenceChance, nameof(data.FenceChance));
            ObstacleSeparation = Required(data.ObstacleSeparation, nameof(data.ObstacleSeparation));
            FenceColliderWidth = Required(data.FenceColliderWidth, nameof(data.FenceColliderWidth));
            FenceColliderHeight = Required(data.FenceColliderHeight, nameof(data.FenceColliderHeight));
            StumpColliderRadius = Required(data.StumpColliderRadius, nameof(data.StumpColliderRadius));
            var obstacles = new List<FieldObstacleDefinition>();
            var obstacleIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in data.Obstacles ?? Array.Empty<FieldObstacleData>())
            {
                if (item == null) throw new ArgumentException("Field obstacles cannot contain null.");
                var obstacle = new FieldObstacleDefinition(item.Id, item.Kind ?? throw new ArgumentException("Obstacle kind is required."),
                    Required(item.X, "obstacle x"), Required(item.Y, "obstacle y"), Required(item.Width, "obstacle width"),
                    Required(item.Height, "obstacle height"), item.VisualId);
                if (!obstacleIds.Add(obstacle.Id)) throw new ArgumentException($"Duplicate field obstacle '{obstacle.Id}'.");
                obstacles.Add(obstacle);
            }
            if (obstacles.Count > 0 && obstacles.Count != (data.InteriorObstacleCount ?? -1))
                throw new ArgumentException("Authored obstacle count must match interiorObstacleCount.");
            ExplicitObstacles = obstacles.AsReadOnly();
            ObstacleLayout = ToLayout(data.ObstacleLayout);
            if (ObstacleLayout != null && obstacles.Count > 0)
                throw new ArgumentException("A field uses either authored obstacles or a per-run layout, not both.");
            BlobLayout = data.BlobLayout == null ? null : new FieldBlobLayoutDefinition(data.BlobLayout);
            RoadLayout = data.RoadLayout == null ? null : new FieldRoadLayoutDefinition(data.RoadLayout);
            RoadFallbackLayouts = RoadLayout == null ? Array.Empty<FieldRoadLayout>() :
                (data.RoadFallbackLayouts ?? throw new ArgumentException("Road fallback layouts required."))
                .Select(item => FieldRoadLayout.FromReference(RoadLayout, item, item.Seed ?? throw new ArgumentException("Fallback seed required."))).ToArray();
            if (RoadLayout != null && (ObstacleLayout != null || BlobLayout != null || obstacles.Count > 0 || RoadFallbackLayouts.Count == 0 ||
                data.ArenaSideLength != RoadLayout.ArenaSideLength))
                throw new ArgumentException("Roads require their own arena, fallback and no other obstacle layout.");
            PlatformLayout = data.PlatformLayout == null ? null : new FieldPlatformLayoutDefinition(data.PlatformLayout);
            if (PlatformLayout != null && (RoadLayout != null || ObstacleLayout != null || BlobLayout != null || obstacles.Count > 0 ||
                data.ArenaSideLength != PlatformLayout.ArenaSideLength))
                throw new ArgumentException("Platforms require their own arena and no other obstacle layout.");
            if (BlobLayout != null && (ObstacleLayout != null || obstacles.Count > 0))
                throw new ArgumentException("A blob layout excludes authored obstacles and the pattern layout.");
            if (BlobLayout != null && data.InteriorObstacleCount != BlobLayout.TotalCount)
                throw new ArgumentException("interiorObstacleCount must match the blob layout count.");
            ZoneLayout = data.ZoneLayout == null ? null : new Game.Zones.ZoneLayoutDefinition(data.ZoneLayout);
            AltarPresentation = data.AltarPresentation == null ? null : new AltarPresentationProfile(data.AltarPresentation);
            if (AltarPresentation != null && (ZoneLayout == null || ZoneLayout.Zones.Any(effect =>
                    !effect.IsAltar || effect.Kind == Game.Zones.ZoneEffectKind.Charge ||
                    effect.Polarity == Game.Zones.ZoneAltarPolarity.Neutral ||
                    (effect.Kind != Game.Zones.ZoneEffectKind.Shrine && effect.Lifetime != Game.Zones.ZoneLifetimeMode.Cycling) ||
                    (effect.Polarity == Game.Zones.ZoneAltarPolarity.Negative && effect.HarmsEnemies))))
                throw new ArgumentException("Prepared altar fields require cycling altars or positive shrines, without Charge or Neutral.");
            ArenaSideLength = data.ArenaSideLength;
            if (ArenaSideLength.HasValue) NumericValidation.ValidatePositive(ArenaSideLength.Value, nameof(ArenaSideLength));

            if (!Id.IsValid || !EnvironmentId.IsValid || !Ground.Id.IsValid || !Fence.Id.IsValid ||
                !Obstacle.Id.IsValid || !Bush.Id.IsValid || !Grass.Id.IsValid || string.IsNullOrWhiteSpace(ObstacleName))
                throw new ArgumentException("Field presentation requires valid IDs and an obstacle name.");
            NumericValidation.ValidatePositive(ObstacleScale, nameof(ObstacleScale));
            NumericValidation.ValidatePositive(DecorationSpacing, nameof(DecorationSpacing));
            NumericValidation.ValidateNonNegative(DecorationJitter, nameof(DecorationJitter));
            NumericValidation.ValidateRange(DecorationChance, 0, 1, nameof(DecorationChance));
            NumericValidation.ValidateRange(BushChance, 0, 1, nameof(BushChance));
            NumericValidation.ValidateRange(ShrineChance, 0, 1, nameof(ShrineChance));
            if (ShrineChance > 0 && !Shrine.Id.IsValid)
                throw new ArgumentException("shrineVisualId is required when shrineChance is positive.");
            if ((obstacles.Exists(item => item.Kind == FieldObstacleKind.Column) ||
                 ObstacleLayout?.UsesKind(FieldObstacleKind.Column) == true) && !Column.Id.IsValid)
                throw new ArgumentException("columnVisualId is required for column obstacles.");
            if ((obstacles.Exists(item => item.Kind == FieldObstacleKind.Barrel) ||
                 ObstacleLayout?.UsesKind(FieldObstacleKind.Barrel) == true) && !Barrel.Id.IsValid)
                throw new ArgumentException("barrelVisualId is required for barrel obstacles.");
            if ((obstacles.Exists(item => item.Kind == FieldObstacleKind.Rock) ||
                 ObstacleLayout?.UsesKind(FieldObstacleKind.Rock) == true) && !Rock.Id.IsValid)
                throw new ArgumentException("rockVisualId is required for rock obstacles.");
            NumericValidation.ValidateNonNegative(DecorationMargin, nameof(DecorationMargin));
            NumericValidation.ValidateNonNegative(SafeRadius, nameof(SafeRadius));
            ValidateScaleRange(GrassScaleMin, GrassScaleMax, "grass");
            ValidateScaleRange(BushScaleMin, BushScaleMax, "bush");
            if (DecorationJitter * 2 >= DecorationSpacing)
                throw new ArgumentException("Decoration jitter must stay inside its placement cell.");
            NumericValidation.ValidateNonNegative(InteriorObstacleCount, nameof(InteriorObstacleCount));
            NumericValidation.ValidateCount(ObstaclePlacementAttempts, nameof(ObstaclePlacementAttempts));
            NumericValidation.ValidateNonNegative(NearObstacleCount, nameof(NearObstacleCount));
            if (NearObstacleCount > InteriorObstacleCount)
                throw new ArgumentException("Near obstacle count cannot exceed the total obstacle count.");
            NumericValidation.ValidatePositive(NearObstacleRadius, nameof(NearObstacleRadius));
            NumericValidation.ValidateRange(FenceChance, 0, 1, nameof(FenceChance));
            NumericValidation.ValidatePositive(ObstacleSeparation, nameof(ObstacleSeparation));
            NumericValidation.ValidatePositive(FenceColliderWidth, nameof(FenceColliderWidth));
            NumericValidation.ValidatePositive(FenceColliderHeight, nameof(FenceColliderHeight));
            NumericValidation.ValidatePositive(StumpColliderRadius, nameof(StumpColliderRadius));
        }

        public IEnumerable<ContentReference> GetReferencedContent()
        {
            if (PlatformLayout?.Art != null) yield return PlatformLayout.Art.Visual.ToReference();
            if (RoadLayout?.Art != null)
            {
                yield return RoadLayout.Art.Main.ToReference();
                yield return RoadLayout.Art.Branch.ToReference();
                yield return RoadLayout.Art.Curb.ToReference();
            }
            if (AltarPresentation != null)
                foreach (var reference in AltarPresentation.GetReferencedContent()) yield return reference;
            yield return Ground.ToReference();
            yield return Fence.ToReference();
            yield return Obstacle.ToReference();
            yield return Bush.ToReference();
            yield return Grass.ToReference();
            if (Column.Id.IsValid) yield return Column.ToReference();
            if (Barrel.Id.IsValid) yield return Barrel.ToReference();
            if (Rock.Id.IsValid) yield return Rock.ToReference();
            if (Shrine.Id.IsValid) yield return Shrine.ToReference();
            foreach (var obstacle in ExplicitObstacles)
                if (obstacle.VisualId.IsValid)
                    yield return new ContentReference(obstacle.VisualId, typeof(SpriteDefinition));
            if (BlobLayout != null)
                foreach (var item in BlobLayout.Library.Values)
                    yield return item.Visual.ToReference();
            if (ObstacleLayout != null)
                foreach (var visualId in ObstacleLayout.Patterns.SelectMany(pattern => pattern.Pieces)
                             .SelectMany(piece => piece.VisualVariants.Append(piece.VisualId)).Where(id => id.IsValid).Distinct())
                    yield return new ContentReference(visualId, typeof(SpriteDefinition));
        }

        private static FieldObstacleLayoutDefinition ToLayout(FieldObstacleLayoutData data)
        {
            if (data == null) return null;
            if (data.Patterns == null) throw new ArgumentException("obstacleLayout.patterns is required.");
            var patterns = new List<FieldObstaclePattern>();
            foreach (var pattern in data.Patterns)
            {
                if (pattern?.Pieces == null || pattern.Rotations == null)
                    throw new ArgumentException("Layout patterns need rotations and pieces.");
                var pieces = new List<FieldObstaclePiece>();
                foreach (var piece in pattern.Pieces)
                {
                    if (piece == null) throw new ArgumentException($"Pattern {pattern.Id} has an empty piece.");
                    pieces.Add(new FieldObstaclePiece(piece.Kind ?? throw new ArgumentException("Piece kind is required."),
                        Required(piece.X, "piece x"), Required(piece.Y, "piece y"), Required(piece.Width, "piece width"),
                        Required(piece.Height, "piece height"), piece.VisualId, piece.VisualIds));
                }
                patterns.Add(new FieldObstaclePattern(pattern.Id, Required(pattern.Weight, "pattern weight"), pattern.Rotations, pieces));
            }
            return new FieldObstacleLayoutDefinition(Required(data.CellSize, "obstacleLayout.cellSize"),
                data.PatternsPerCell ?? throw new ArgumentException("obstacleLayout.patternsPerCell is required."),
                Required(data.EdgeMargin, "obstacleLayout.edgeMargin"), Required(data.CellMargin, "obstacleLayout.cellMargin"),
                Required(data.StartClearRadius, "obstacleLayout.startClearRadius"),
                Required(data.MinPatternGap, "obstacleLayout.minPatternGap"),
                data.PlacementAttempts ?? throw new ArgumentException("obstacleLayout.placementAttempts is required."),
                data.ReferenceSeed ?? throw new ArgumentException("obstacleLayout.referenceSeed is required."), patterns,
                data.StartScreen == null ? null : new FieldStartScreenDefinition(
                    Required(data.StartScreen.HalfWidth, "obstacleLayout.startScreen.halfWidth"),
                    Required(data.StartScreen.HalfHeight, "obstacleLayout.startScreen.halfHeight"),
                    Required(data.StartScreen.MinAbsX, "obstacleLayout.startScreen.minAbsX"),
                    Required(data.StartScreen.MaxAbsX, "obstacleLayout.startScreen.maxAbsX"),
                    Required(data.StartScreen.MinAbsY, "obstacleLayout.startScreen.minAbsY"),
                    Required(data.StartScreen.MaxAbsY, "obstacleLayout.startScreen.maxAbsY"),
                    data.StartScreen.PatternIds));
        }

        private static float Required(float? value, string name) =>
            value ?? throw new ArgumentException($"{name} is required.");

        private static void ValidateScaleRange(float minimum, float maximum, string name)
        {
            NumericValidation.ValidatePositive(minimum, $"{name}ScaleMin");
            NumericValidation.ValidatePositive(maximum, $"{name}ScaleMax");
            if (maximum < minimum) throw new ArgumentException($"{name} scale range is reversed.");
        }
    }
}
