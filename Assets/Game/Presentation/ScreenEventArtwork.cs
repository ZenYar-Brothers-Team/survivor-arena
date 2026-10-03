using System;
using Game.Content;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>A typed visual reference with authored UV crop; never derives gameplay dimensions from pixels.</summary>
    public sealed class ScreenEventArtwork
    {
        public ContentRef<SpriteDefinition> Visual { get; }
        public Rect UvRect { get; }
        public SpriteRole Role { get; }

        public ScreenEventArtwork(ScreenEventArtworkData data, SpriteRole role)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            Visual = new ContentRef<SpriteDefinition>(data.VisualId);
            if (!Visual.Id.IsValid) throw new ArgumentException("Screen-event visualId required.");
            if (data.UvRect == null || data.UvRect.Length != 4) throw new ArgumentException("Screen-event uvRect needs x/y/width/height.");
            foreach (var value in data.UvRect) NumericValidation.ValidateRange(value, 0f, 1f, "uvRect");
            NumericValidation.ValidatePositive(data.UvRect[2], "uvRect width");
            NumericValidation.ValidatePositive(data.UvRect[3], "uvRect height");
            if (data.UvRect[0] + data.UvRect[2] > 1f || data.UvRect[1] + data.UvRect[3] > 1f)
                throw new ArgumentException("Screen-event uvRect must fit the texture.");
            UvRect = new Rect(data.UvRect[0], data.UvRect[1], data.UvRect[2], data.UvRect[3]);
            Role = role;
        }
    }
}
