using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// One shape to draw for a boss hazard: circles use <see cref="Center"/> and <see cref="Radius"/>; beams start at
    /// <see cref="Center"/>, point along <see cref="AngleDegrees"/> and are <see cref="Length"/> by <see cref="Width"/>.
    /// <see cref="Progress"/> is 0…1 through the current stage.
    /// </summary>
    public readonly struct BossHazardVisual
    {
        public BossHazardVisualKind Kind { get; }
        public Vector2 Center { get; }
        public float Radius { get; }
        public float AngleDegrees { get; }
        public float Length { get; }
        public float Width { get; }
        public float Progress { get; }
        public Color Color { get; }

        public BossHazardVisual(BossHazardVisualKind kind, Vector2 center, float radius, float progress, Color color,
            float angleDegrees = 0f, float length = 0f, float width = 0f)
        {
            Kind = kind;
            Center = center;
            Radius = radius;
            Progress = Mathf.Clamp01(progress);
            Color = color;
            AngleDegrees = angleDegrees;
            Length = length;
            Width = width;
        }
    }
}
