using Game.Content;
using Game.Presentation;
using UnityEngine;
namespace Game.Traveler
{
    /// <summary>Runtime-generated outlines for Traveler effects (DECISION-0123); one unit = one world unit across at scale 1, built once per process.</summary>
    public static class TravelerShapeSprites
    {
        // Generated once and shared by every pooled effect; the extra resolution keeps the narrow ornaments legible at a 6–8 unit diameter.
        private const int Size = 256;
        private static Sprite _hexagon;
        private static Sprite _dots;
        private static Sprite _lotus;
        private static Sprite _shieldArt, _speedArt, _healArt;
        /// <summary>Approved circular source images; the effect root flattens them into the ground ellipse after rotation.</summary>
        public static Sprite ShieldArt => _shieldArt != null ? _shieldArt :
            _shieldArt = LoadArt("TRAVELER-005-VISUAL-TELEGRAPH");
        public static Sprite SpeedArt => _speedArt != null ? _speedArt :
            _speedArt = LoadArt("TRAVELER-007-VISUAL-TELEGRAPH");
        public static Sprite HealArt => _healArt != null ? _healArt :
            _healArt = LoadArt("TRAVELER-009-VISUAL-TELEGRAPH");
        /// <summary>Flat-top hexagon outline touching the left and right edges.</summary>
        public static Sprite Hexagon => _hexagon != null ? _hexagon : _hexagon = Create("Traveler ward", WardAlpha);
        /// <summary>Twelve alternating long and short sparks on the aura rim.</summary>
        public static Sprite Dots => _dots != null ? _dots : _dots = Create("Traveler dots", DotsAlpha);
        /// <summary>Eight broad lotus petals; used by the healing waves.</summary>
        public static Sprite Lotus => _lotus != null ? _lotus : _lotus = Create("Traveler lotus", LotusAlpha);
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
            var direction = index * step;
            var radial = x * Mathf.Cos(direction) + y * Mathf.Sin(direction) - radius;
            var tangent = -x * Mathf.Sin(direction) + y * Mathf.Cos(direction);
            var length = index % 2 == 0 ? .058f : .038f;
            // A pointed outer tip and a wider inner shoulder read as a small flame at gameplay scale.
            var width = .024f * (1f - .55f * Mathf.Clamp01(radial / length));
            var diamond = 1f - Mathf.Abs(radial) / length - Mathf.Abs(tangent) / width;
            return Mathf.Clamp01(diamond / edge);
        }
        /// <summary>Hexagonal boundary, a nested petal line and six compact diamond runes.</summary>
        public static float WardAlpha(float x, float y)
        {
            var radius = Mathf.Sqrt(x * x + y * y);
            var angle = Mathf.Atan2(y, x);
            var petalRadius = .382f + .045f * Mathf.Pow(Mathf.Max(0f, Mathf.Cos(6f * angle)), 2f);
            var petal = Line(radius - petalRadius, .012f);
            var step = Mathf.PI / 3f;
            var runeAngle = Mathf.Round(angle / step) * step;
            var radial = x * Mathf.Cos(runeAngle) + y * Mathf.Sin(runeAngle) - .455f;
            var tangent = -x * Mathf.Sin(runeAngle) + y * Mathf.Cos(runeAngle);
            var rune = Mathf.Clamp01((1f - Mathf.Abs(radial) / .018f - Mathf.Abs(tangent) / .026f) * Size * .5f);
            return Mathf.Max(HexagonAlpha(x, y), Mathf.Max(petal, rune));
        }
        public static float LotusAlpha(float x, float y)
        {
            var radius = Mathf.Sqrt(x * x + y * y);
            var angle = Mathf.Atan2(y, x);
            var petals = .405f + .07f * Mathf.Pow(Mathf.Max(0f, Mathf.Cos(8f * angle)), 2f);
            return Line(radius - petals, .016f);
        }
        private static float Line(float distance, float halfWidth) => Mathf.Clamp01((halfWidth - Mathf.Abs(distance)) * Size);
        private static Sprite LoadArt(string visualId) =>
            FixtureSpriteCatalog.CreateOne(new ContentId(visualId), SpriteRole.Telegraph).Sprite;
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
