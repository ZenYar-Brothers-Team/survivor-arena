using System;
using Game.Content;
using Game.Traps.Json;

namespace Game.Traps
{
    /// <summary>Presentation settings of a 3D turret model (DECISION-0156); the prefab itself is found by <see cref="Key"/>.</summary>
    public sealed class TrapModelDefinition
    {
        public string Key { get; }
        public string HeadNode { get; }
        public float TiltDegrees { get; }
        public float Scale { get; }
        public float DepthOffset { get; }

        public TrapModelDefinition(TrapModelData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(data.Key)) throw new ArgumentException("Trap model key is required.");
            Key = data.Key;
            HeadNode = string.IsNullOrWhiteSpace(data.HeadNode) ? throw new ArgumentException($"{Key}: headNode is required.") : data.HeadNode;
            TiltDegrees = data.TiltDegrees ?? throw new ArgumentException($"{Key}: tiltDegrees is required.");
            Scale = data.Scale ?? throw new ArgumentException($"{Key}: scale is required.");
            DepthOffset = data.DepthOffset ?? throw new ArgumentException($"{Key}: depthOffset is required.");
            NumericValidation.ValidateRange(TiltDegrees, 1f, 89f, nameof(TiltDegrees));
            NumericValidation.ValidatePositive(Scale, nameof(Scale));
            NumericValidation.ValidateNonNegative(DepthOffset, nameof(DepthOffset));
        }
    }
}
