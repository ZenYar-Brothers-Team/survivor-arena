using System;
using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    public sealed class SpriteDefinition : IContentDefinition
    {
        public ContentId Id { get; }
        public Sprite Sprite { get; }

        public SpriteDefinition(ContentId id, Sprite sprite)
        {
            if (!id.IsValid)
                throw new ArgumentException("Sprite definition requires a valid content id.", nameof(id));

            Id = id;
            Sprite = sprite != null ? sprite : throw new ArgumentNullException(nameof(sprite));
        }
    }
}
