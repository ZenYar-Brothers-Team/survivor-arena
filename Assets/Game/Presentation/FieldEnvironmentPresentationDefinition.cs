using System;
using System.Collections.Generic;
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
        public string ObstacleName { get; }
        public float FenceHeight { get; }
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
            ObstacleName = data.ObstacleName;
            FenceHeight = Required(data.FenceHeight, nameof(data.FenceHeight));
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

            if (!Id.IsValid || !EnvironmentId.IsValid || !Ground.Id.IsValid || !Fence.Id.IsValid ||
                !Obstacle.Id.IsValid || !Bush.Id.IsValid || !Grass.Id.IsValid || string.IsNullOrWhiteSpace(ObstacleName))
                throw new ArgumentException("Field presentation requires valid IDs and an obstacle name.");
            NumericValidation.ValidatePositive(FenceHeight, nameof(FenceHeight));
            NumericValidation.ValidatePositive(ObstacleScale, nameof(ObstacleScale));
            NumericValidation.ValidatePositive(DecorationSpacing, nameof(DecorationSpacing));
            NumericValidation.ValidateNonNegative(DecorationJitter, nameof(DecorationJitter));
            NumericValidation.ValidateRange(DecorationChance, 0, 1, nameof(DecorationChance));
            NumericValidation.ValidateRange(BushChance, 0, 1, nameof(BushChance));
            NumericValidation.ValidateNonNegative(DecorationMargin, nameof(DecorationMargin));
            NumericValidation.ValidateNonNegative(SafeRadius, nameof(SafeRadius));
            ValidateScaleRange(GrassScaleMin, GrassScaleMax, "grass");
            ValidateScaleRange(BushScaleMin, BushScaleMax, "bush");
            if (DecorationJitter * 2 >= DecorationSpacing)
                throw new ArgumentException("Decoration jitter must stay inside its placement cell.");
        }

        public IEnumerable<ContentReference> GetReferencedContent()
        {
            yield return Ground.ToReference();
            yield return Fence.ToReference();
            yield return Obstacle.ToReference();
            yield return Bush.ToReference();
            yield return Grass.ToReference();
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
