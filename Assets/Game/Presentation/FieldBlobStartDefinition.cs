using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;

namespace Game.Presentation
{
    /// <summary>
    /// Rule that one blob partly covers the start screen so an obstacle is visible from the first frame. The blob never
    /// reaches the player (<see cref="FieldBlobLayoutDefinition.PlayerClearRadius"/>).
    /// </summary>
    public sealed class FieldBlobStartDefinition
    {
        public float HalfWidth { get; }
        public float HalfHeight { get; }
        /// <summary>How far past the start screen the blob center may be drawn.</summary>
        public float Reach { get; }
        public float Radius { get; }
        public IReadOnlyList<FieldBlobStyle> Styles { get; }

        public FieldBlobStartDefinition(float halfWidth, float halfHeight, float reach, float radius,
            IEnumerable<FieldBlobStyle> styles)
        {
            NumericValidation.ValidatePositive(halfWidth, nameof(halfWidth));
            NumericValidation.ValidatePositive(halfHeight, nameof(halfHeight));
            NumericValidation.ValidateNonNegative(reach, nameof(reach));
            NumericValidation.ValidatePositive(radius, nameof(radius));
            var list = (styles ?? throw new ArgumentNullException(nameof(styles))).ToList();
            if (list.Count == 0 || list.Contains(FieldBlobStyle.Linear))
                throw new ArgumentException("Start blob styles must be nonempty and compact (Round or Angular).", nameof(styles));
            HalfWidth = halfWidth; HalfHeight = halfHeight; Reach = reach; Radius = radius; Styles = list.AsReadOnly();
        }
    }
}
