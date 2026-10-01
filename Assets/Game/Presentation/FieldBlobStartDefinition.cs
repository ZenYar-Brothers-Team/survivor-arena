using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;

namespace Game.Presentation
{
    /// <summary>
    /// Rule that one blob partly covers the start screen so an obstacle is visible from the first frame. The blob never
    /// reaches the player (<see cref="FieldBlobLayoutDefinition.PlayerClearRadius"/>). It is drawn either procedurally
    /// (radius and compact styles) or from a pool of library items.
    /// </summary>
    public sealed class FieldBlobStartDefinition
    {
        public float HalfWidth { get; }
        public float HalfHeight { get; }
        /// <summary>How far past the start screen the blob center may be drawn.</summary>
        public float Reach { get; }
        /// <summary>Procedural radius; 0 when <see cref="LibraryIds"/> is used.</summary>
        public float Radius { get; }
        public IReadOnlyList<FieldBlobStyle> Styles { get; }
        /// <summary>Library items one of which covers the start screen; empty for procedural.</summary>
        public IReadOnlyList<string> LibraryIds { get; }
        public bool UsesLibrary => LibraryIds.Count > 0;

        public FieldBlobStartDefinition(float halfWidth, float halfHeight, float reach, float radius,
            IEnumerable<FieldBlobStyle> styles, IEnumerable<string> libraryIds = null)
        {
            NumericValidation.ValidatePositive(halfWidth, nameof(halfWidth));
            NumericValidation.ValidatePositive(halfHeight, nameof(halfHeight));
            NumericValidation.ValidateNonNegative(reach, nameof(reach));
            LibraryIds = (libraryIds ?? Enumerable.Empty<string>()).ToList().AsReadOnly();
            if (LibraryIds.Count > 0)
            {
                if (LibraryIds.Any(string.IsNullOrWhiteSpace) || LibraryIds.Distinct().Count() != LibraryIds.Count)
                    throw new ArgumentException("Start library ids must be unique and nonempty.", nameof(libraryIds));
                Styles = new List<FieldBlobStyle>().AsReadOnly();
            }
            else
            {
                NumericValidation.ValidatePositive(radius, nameof(radius));
                var list = (styles ?? throw new ArgumentNullException(nameof(styles))).ToList();
                if (list.Count == 0 || list.Contains(FieldBlobStyle.Linear) || list.Contains(FieldBlobStyle.Library))
                    throw new ArgumentException("Start blob styles must be nonempty and compact (Round or Angular).", nameof(styles));
                Styles = list.AsReadOnly();
            }
            HalfWidth = halfWidth; HalfHeight = halfHeight; Reach = reach; Radius = radius;
        }
    }
}
