using UnityEngine;

namespace Game.Presentation
{
    /// <summary>One shared soft circular mask; renderers stretch it into a ground ellipse.</summary>
    public static class GroundShadowSprite
    {
        private const int TextureSize = 32;
        private static Sprite _shared;

        public static Sprite Shared
        {
            get
            {
                if (_shared != null) return _shared;
                var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
                {
                    name = "Procedural ground shadow",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave
                };
                var pixels = new Color32[TextureSize * TextureSize];
                for (var y = 0; y < TextureSize; y++)
                for (var x = 0; x < TextureSize; x++)
                {
                    var dx = (x + .5f) / TextureSize * 2f - 1f;
                    var dy = (y + .5f) / TextureSize * 2f - 1f;
                    var alpha = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    pixels[y * TextureSize + x] = new Color(1f, 1f, 1f, alpha * alpha);
                }
                texture.SetPixels32(pixels);
                texture.Apply(false, true);
                _shared = Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize),
                    new Vector2(.5f, .5f), TextureSize);
                _shared.name = "Procedural ground shadow";
                _shared.hideFlags = HideFlags.HideAndDontSave;
                return _shared;
            }
        }
    }
}
