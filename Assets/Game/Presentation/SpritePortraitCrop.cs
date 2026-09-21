using System;
using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    // Owns only the derived Sprite; the source texture remains owned by Resources.
    public sealed class SpritePortraitCrop : IDisposable
    {
        public SpriteDefinition Definition { get; }
        public SpritePortraitCrop(ContentId id, SpriteDefinition body, Rect normalizedRect)
        {
            if (!id.IsValid) throw new ArgumentException("A crop requires a valid visual id.", nameof(id));
            if (body == null) throw new ArgumentNullException(nameof(body));
            body.RequireRole(SpriteRole.Body);
            NumericValidation.ValidateRange(normalizedRect.x, 0, 1, nameof(normalizedRect));
            NumericValidation.ValidateRange(normalizedRect.y, 0, 1, nameof(normalizedRect));
            NumericValidation.ValidatePositive(normalizedRect.width, nameof(normalizedRect));
            NumericValidation.ValidatePositive(normalizedRect.height, nameof(normalizedRect));
            if (normalizedRect.xMax > 1 || normalizedRect.yMax > 1)
                throw new ArgumentOutOfRangeException(nameof(normalizedRect));
            var source = body.Sprite.rect;
            var rect = new Rect(source.x + source.width * normalizedRect.x, source.y + source.height * normalizedRect.y,
                source.width * normalizedRect.width, source.height * normalizedRect.height);
            Definition = new SpriteDefinition(id, Sprite.Create(body.Sprite.texture, rect, new Vector2(.5f, .5f),
                body.Sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect), SpriteRole.Portrait);
        }
        public void Dispose()
        {
            if (Definition.Sprite == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(Definition.Sprite);
            else UnityEngine.Object.DestroyImmediate(Definition.Sprite);
        }
    }
}
