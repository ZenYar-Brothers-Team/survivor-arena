using UnityEngine;

namespace Game.Movement
{
    // Procedural 1x1 sprite used for fixture visualization before real art
    // (Content Design art/animation) exists. Shared across objects; tint via
    // SpriteRenderer.color rather than creating a new texture per object.
    public static class PlaceholderSprite
    {
        private static Sprite _shared;

        public static Sprite Shared
        {
            get
            {
                if (_shared == null)
                {
                    var texture = new Texture2D(1, 1);
                    texture.SetPixel(0, 0, Color.white);
                    texture.Apply();
                    _shared = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                }

                return _shared;
            }
        }
    }
}
