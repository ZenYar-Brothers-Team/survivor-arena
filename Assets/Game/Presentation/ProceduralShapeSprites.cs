using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Shared runtime-generated shapes for procedural world effects; one unit = one world unit diameter.</summary>
    public static class ProceduralShapeSprites
    {
        private const int TextureSize = 128;
        private static Sprite _disc;
        private static Sprite _ring;

        /// <summary>Soft-edged filled disc, 1 world unit across at scale 1.</summary>
        public static Sprite Disc => _disc != null ? _disc : _disc = Create("Procedural skill disc", 0f);

        /// <summary>Thin annulus whose outer edge is 1 world unit across at scale 1 (inner edge at 88%).</summary>
        public static Sprite Ring => _ring != null ? _ring : _ring = Create("Procedural skill ring", .88f);

        private static Sprite Create(string name, float innerRadius)
        {
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            var pixels = new Color32[TextureSize * TextureSize];
            var edge = 2f / TextureSize;
            for (var y = 0; y < TextureSize; y++)
            for (var x = 0; x < TextureSize; x++)
            {
                var dx = (x + .5f) / TextureSize * 2f - 1f;
                var dy = (y + .5f) / TextureSize * 2f - 1f;
                var r = Mathf.Sqrt(dx * dx + dy * dy);
                var outer = Mathf.Clamp01((1f - r) / edge);
                var inner = innerRadius <= 0f ? 1f : Mathf.Clamp01((r - innerRadius) / edge);
                pixels[y * TextureSize + x] = new Color(1f, 1f, 1f, outer * inner);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(.5f, .5f), TextureSize);
            sprite.name = name;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }
    }
}
