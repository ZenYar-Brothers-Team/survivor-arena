using UnityEngine;

namespace Game.Presentation
{
    public readonly struct SpritePose
    {
        public Vector3 PositionOffset { get; }
        public float RotationDegrees { get; }
        public Vector2 ScaleMultiplier { get; }
        public bool FlipX { get; }
        public float FlashAmount { get; }

        public SpritePose(
            Vector3 positionOffset,
            float rotationDegrees,
            Vector2 scaleMultiplier,
            bool flipX,
            float flashAmount)
        {
            PositionOffset = positionOffset;
            RotationDegrees = rotationDegrees;
            ScaleMultiplier = scaleMultiplier;
            FlipX = flipX;
            FlashAmount = flashAmount;
        }
    }
}
