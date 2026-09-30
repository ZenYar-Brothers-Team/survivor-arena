using UnityEngine;
namespace Game.Traveler
{
    /// <summary>Runtime-generated outlines for Traveler effects (DECISION-0123); one unit = one world unit across at scale 1, built once per process.</summary>
    public static class TravelerShapeSprites
    {
        private const int Size = 128;
        private static Sprite _hexagon;
        private static Sprite _dots;
        /// <summary>Flat-top hexagon outline touching the left and right edges.</summary>
        public static Sprite Hexagon => _hexagon != null ? _hexagon : _hexagon = Create("Traveler hexagon", HexagonAlpha);
        /// <summary>Twelve alternating big and small dots on a circle.</summary>
        public static Sprite Dots => _dots != null ? _dots : _dots = Create("Traveler dots", DotsAlpha);
        /// <summary>Outline coverage at a point in [-0.5, 0.5] (public so tests can read it: the texture itself is not readable).</summary>
        public static float HexagonAlpha(float x, float y)
        {
            const float apothem = .5f * .8660254f, thickness = .04f, edge = 1f / Size;
            var m = 0f;
            for (var k = 0; k < 3; k++)
            {
                var angle = (30f + 60f * k) * Mathf.Deg2Rad;
                m = Mathf.Max(m, Mathf.Abs(x * Mathf.Cos(angle) + y * Mathf.Sin(angle)));
            }
            return Mathf.Clamp01((apothem - m) / edge) * Mathf.Clamp01((m - (apothem - thickness)) / edge);
        }
        /// <summary>Dot coverage at a point in [-0.5, 0.5].</summary>
        public static float DotsAlpha(float x, float y)
        {
            const int count = 12; const float radius = .44f, edge = 1f / Size;
            var angle = Mathf.Atan2(y, x);
            var step = 2f * Mathf.PI / count;
            var index = Mathf.RoundToInt(angle / step);
            var center = new Vector2(Mathf.Cos(index * step), Mathf.Sin(index * step)) * radius;
            var dotRadius = (index % 2 == 0) ? .04f : .026f;
            return Mathf.Clamp01((dotRadius - Vector2.Distance(new Vector2(x, y), center)) / edge);
        }
        private static Sprite Create(string name, System.Func<float, float, float> alpha)
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            { name = name, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color32[Size * Size];
            for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
                pixels[y * Size + x] = new Color(1f, 1f, 1f, alpha((x + .5f) / Size - .5f, (y + .5f) / Size - .5f));
            texture.SetPixels32(pixels); texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(.5f, .5f), Size);
            sprite.name = name; sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }
    }
}
