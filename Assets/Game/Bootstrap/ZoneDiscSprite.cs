using UnityEngine;

namespace Game.Bootstrap
{
    /// <summary>Placeholder look of a zone: a white disc with a soft edge and a slightly brighter rim, tinted per effect.</summary>
    public static class ZoneDiscSprite
    {
        private const int Size = 128;
        private const float PixelsPerUnit = 32f;

        /// <summary>Creates a 4-unit-wide disc sprite; the caller owns the sprite and its texture.</summary>
        public static Sprite Create()
        {
            var pixels = new Color32[Size * Size];
            var half = Size * .5f;
            for (var y = 0; y < Size; y++)
                for (var x = 0; x < Size; x++)
                {
                    var distance = Mathf.Sqrt((x + .5f - half) * (x + .5f - half) + (y + .5f - half) * (y + .5f - half)) / half;
                    var alpha = Mathf.Clamp01((1f - distance) / .12f);
                    var rim = Mathf.Clamp01(1f - Mathf.Abs(distance - .9f) / .06f) * .35f;
                    pixels[y * Size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha * (.65f + rim)));
                }
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { name = "ZoneDisc", wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(.5f, .5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }
    }
}
