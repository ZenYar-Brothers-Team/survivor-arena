using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Storybook pressure front and inward tail (Art Direction §12). Pure presentation:
    /// transparent center, tapered cone tips, all ink inside the authoritative outer radius.
    /// The presenter caches these masks and owns their sprite/texture lifetime.
    /// </summary>
    public static class PressureWaveSprites
    {
        public static Sprite Create(SkillWorldEffectProfile profile, float arcDegrees, bool tail, bool keepReadable = false)
        {
            const int size = 512;
            var pixels = new Color32[size * size];
            var halfArc = arcDegrees * Mathf.Deg2Rad * .5f;
            var ring = arcDegrees >= 360f;
            var edge = 2f / size;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var point = new Vector2((x + .5f) / size * 2f - 1f, (y + .5f) / size * 2f - 1f);
                var radius = point.magnitude;
                var angle = Mathf.Atan2(point.y, point.x);
                if (radius >= 1f || (!ring && Mathf.Abs(angle) >= halfArc)) continue;
                var taper = ring ? 1f : Mathf.Sqrt(Mathf.Clamp01(1f - Mathf.Pow(angle / halfArc, 2f)));
                var width = profile.BandFraction * taper;
                var ripple = (Mathf.Sin(angle * 7f) + Mathf.Sin(angle * 13f + 1f)) * .25f + .5f;
                var outerRadius = 1f - profile.BandFraction * profile.ContourVariation * ripple;
                var depth = (outerRadius - radius) / Mathf.Max(width, edge);
                var coverage = Mathf.Clamp01((outerRadius - radius) / edge);
                var tipFade = ring ? 1f : Mathf.Clamp01((halfArc - Mathf.Abs(angle)) / edge);
                var alpha = 0f;
                if (depth >= 0f && depth <= 1f)
                {
                    // Narrow cream crest, softer colored mass trailing inward. No filled disc.
                    alpha = tail ? Mathf.Pow(1f - depth, 2f) * Mathf.Clamp01(depth / .2f)
                        : Mathf.Clamp01(1f - depth / .2f);
                    alpha *= coverage * tipFade;
                }
                if (tail && profile.AccentCount > 0)
                {
                    var span = ring ? Mathf.PI * 2f : halfArc * 2f;
                    var start = ring ? -Mathf.PI : -halfArc;
                    for (var i = 0; i < profile.AccentCount; i++)
                    {
                        var accentAngle = start + span * (i + .5f) / profile.AccentCount;
                        var angularDistance = Mathf.Abs(Mathf.DeltaAngle(angle * Mathf.Rad2Deg, accentAngle * Mathf.Rad2Deg)) * Mathf.Deg2Rad;
                        var along = (outerRadius - width - radius) / profile.AccentLengthFraction;
                        if (along <= 0f || along >= 1f) continue;
                        var stroke = Mathf.Clamp01(1f - angularDistance / profile.AccentWidthRadians);
                        alpha = Mathf.Max(alpha, stroke * Mathf.Sin(along * Mathf.PI) * tipFade);
                    }
                }
                var color = tail ? profile.Color : profile.ImpactColor;
                color.a *= alpha;
                pixels[y * size + x] = color;
            }
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = tail ? "Pressure wave tail" : "Pressure wave crest",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, !keepReadable);
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size,
                0, SpriteMeshType.FullRect);
            sprite.name = texture.name;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }
    }
}
