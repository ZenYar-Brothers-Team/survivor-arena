using System;
using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    public sealed class SpriteDefinition : IContentDefinition
    {
        public ContentId Id { get; }
        public Sprite Sprite { get; }
        public SpriteRole Role { get; }

        public SpriteDefinition(ContentId id, Sprite sprite, SpriteRole role = SpriteRole.Unspecified)
        {
            if (!id.IsValid)
                throw new ArgumentException("Sprite definition requires a valid content id.", nameof(id));

            Id = id;
            Sprite = sprite != null ? sprite : throw new ArgumentNullException(nameof(sprite));
            if (!Enum.IsDefined(typeof(SpriteRole), role)) throw new ArgumentOutOfRangeException(nameof(role));
            Role = role;
        }

        public void RequireRole(SpriteRole expected)
        {
            if (expected == SpriteRole.Unspecified || Role != expected)
                throw new InvalidOperationException($"Visual '{Id}' has role {Role}, expected {expected}.");
        }
    }
}
