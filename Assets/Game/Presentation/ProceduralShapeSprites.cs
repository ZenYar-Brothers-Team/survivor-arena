using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Shared runtime-generated shapes for procedural world effects; one unit = one world unit diameter.</summary>
    public static class ProceduralShapeSprites
    {
        private const int TextureSize = 128;
        private static Sprite _disc;
        private static Sprite _ring;
        private static Sprite _pillar;

        /// <summary>Soft-edged filled disc, 1 world unit across at scale 1.</summary>
        public static Sprite Disc => _disc != null ? _disc : _disc = Create("Procedural skill disc", 0f);

        /// <summary>Thin annulus whose outer edge is 1 world unit across at scale 1 (inner edge at 88%).</summary>
        public static Sprite Ring => _ring != null ? _ring : _ring = Create("Procedural skill ring", .88f);

        /// <summary>
        /// Vertical light pillar, 1 world unit tall at scale 1 (width from its bounds), pivot at the bottom center: soft side edges,
        /// full brightness at the base fading out toward the top.
        /// </summary>
        public static Sprite Pillar => _pillar != null ? _pillar : _pillar = CreatePillar();

        private static Sprite CreatePillar()
        {
            const int width = 32;
            const int height = 128;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Procedural strike pillar",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var across = Mathf.Abs((x + .5f) / width * 2f - 1f);
                var side = 1f - across * across;
                var up = 1f - (y + .5f) / height;
                pixels[y * width + x] = new Color(1f, 1f, 1f, side * up);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(.5f, 0f), height);
            sprite.name = texture.name;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

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
