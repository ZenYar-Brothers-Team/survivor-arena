using System;
using Game.Content;
using UnityEngine;

namespace Game.Presentation.Editor
{
    // Technical import contract; values are authored in Art/ImportProfiles.json.
    public sealed class SpriteImportProfile
    {
        public string Key { get; set; }
        public float? PixelsPerUnit { get; set; }
        public int? MaxSize { get; set; }
        public float? PivotX { get; set; }
        public float? PivotY { get; set; }
        public string Reason { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Key) || string.IsNullOrWhiteSpace(Reason) ||
                !PixelsPerUnit.HasValue || !MaxSize.HasValue || !PivotX.HasValue || !PivotY.HasValue)
                throw new InvalidOperationException("Import profile requires key, PPU, maxSize, pivot and reason.");
            NumericValidation.ValidatePositive(PixelsPerUnit.Value, nameof(PixelsPerUnit));
            NumericValidation.ValidateRange(PivotX.Value, 0, 1, nameof(PivotX));
            NumericValidation.ValidateRange(PivotY.Value, 0, 1, nameof(PivotY));
            if (MaxSize < 32 || MaxSize > 8192 || !Mathf.IsPowerOfTwo(MaxSize.Value))
                throw new InvalidOperationException("Import maxSize must be a power of two in [32, 8192].");
        }
    }
}
