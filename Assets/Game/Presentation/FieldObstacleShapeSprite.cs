using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Paints a generated obstacle silhouette into a flat-colour sprite with an outline (placeholder look for the
    /// geometry study). The sprite pivot is the shape center, so the renderer sits at <see cref="FieldObstacleShapeDefinition.Center"/>.
    /// The caller owns the returned sprite and texture and must destroy both.
    /// </summary>
    public static class FieldObstacleShapeSprite
    {
        public static Sprite Create(FieldObstacleShapeDefinition shape, Color fill, Color outline, float pixelsPerUnit,
            int outlinePixels)
        {
            if (shape == null) throw new ArgumentNullException(nameof(shape));
            if (pixelsPerUnit <= 0f) throw new ArgumentOutOfRangeException(nameof(pixelsPerUnit));
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            foreach (var point in shape.Points)
            {
                var local = point - shape.Center;
                min = Vector2.Min(min, local);
                max = Vector2.Max(max, local);
            }
            const int padding = 2;
            var width = Mathf.CeilToInt((max.x - min.x) * pixelsPerUnit) + padding * 2;
            var height = Mathf.CeilToInt((max.y - min.y) * pixelsPerUnit) + padding * 2;
            var origin = new Vector2(min.x - padding / pixelsPerUnit, min.y - padding / pixelsPerUnit);
            var mask = Rasterize(shape, origin, width, height, pixelsPerUnit);
            var pixels = new Color32[width * height];
            Color32 fillColor = fill;
            Color32 outlineColor = outline;
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    var index = y * width + x;
                    if (!mask[index]) continue;
                    pixels[index] = IsEdge(mask, x, y, width, height, outlinePixels) ? outlineColor : fillColor;
                }
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = shape.Id, filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            var pivot = new Vector2(-origin.x * pixelsPerUnit / width, -origin.y * pixelsPerUnit / height);
            var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = shape.Id;
            return sprite;
        }

        // Even-odd scanline fill sampled at pixel centers.
        private static bool[] Rasterize(FieldObstacleShapeDefinition shape, Vector2 origin, int width, int height, float ppu)
        {
            var mask = new bool[width * height];
            var points = shape.Points;
            var crossings = new List<float>();
            for (var y = 0; y < height; y++)
            {
                var sampleY = shape.Center.y + origin.y + (y + .5f) / ppu;
                crossings.Clear();
                for (int i = 0, j = points.Count - 1; i < points.Count; j = i++)
                {
                    var a = points[j];
                    var b = points[i];
                    if ((a.y > sampleY) == (b.y > sampleY)) continue;
                    crossings.Add(a.x + (sampleY - a.y) / (b.y - a.y) * (b.x - a.x));
                }
                crossings.Sort();
                for (var c = 0; c + 1 < crossings.Count; c += 2)
                {
                    var from = Mathf.Max(0, Mathf.CeilToInt((crossings[c] - shape.Center.x - origin.x) * ppu - .5f));
                    var to = Mathf.Min(width - 1, Mathf.FloorToInt((crossings[c + 1] - shape.Center.x - origin.x) * ppu - .5f));
                    for (var x = from; x <= to; x++) mask[y * width + x] = true;
                }
            }
            return mask;
        }

        private static bool IsEdge(bool[] mask, int x, int y, int width, int height, int outlinePixels)
        {
            for (var d = 1; d <= outlinePixels; d++)
                if (!Inside(mask, x - d, y, width, height) || !Inside(mask, x + d, y, width, height) ||
                    !Inside(mask, x, y - d, width, height) || !Inside(mask, x, y + d, width, height))
                    return true;
            return false;
        }

        private static bool Inside(bool[] mask, int x, int y, int width, int height) =>
            x >= 0 && y >= 0 && x < width && y < height && mask[y * width + x];
    }
}
