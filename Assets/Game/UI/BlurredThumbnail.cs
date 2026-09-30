using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace Game.UI
{
    /// <summary>Heavily blurred stand-in for the picture of a closed map; the blur is baked once per sprite into a tiny texture.</summary>
    public static class BlurredThumbnail
    {
        private const int CoarseWidth = 16;
        private const int SmoothWidth = 128;
        private static readonly Dictionary<Sprite, RenderTexture> Cache = new Dictionary<Sprite, RenderTexture>();
        public static void Apply(Image image, Sprite sprite)
        {
            image.sprite = null;
            image.image = sprite != null ? Get(sprite) : null;
        }
        public static Texture Get(Sprite sprite)
        {
            if (Cache.TryGetValue(sprite, out var cached) && cached != null) return cached;
            var rect = sprite.textureRect;
            var source = sprite.texture;
            var scale = new Vector2(rect.width / source.width, rect.height / source.height);
            var offset = new Vector2(rect.x / source.width, rect.y / source.height);
            var aspect = rect.height / rect.width;
            // Halve step by step (plain bilinear downsampling of a big texture aliases), then climb back up.
            var current = Blit(source, source.width / 2, Mathf.Max(2, Mathf.RoundToInt(source.width / 2 * aspect)), scale, offset, null);
            for (var width = current.width / 2; width >= CoarseWidth; width /= 2)
                current = Blit(current, width, Mathf.Max(2, Mathf.RoundToInt(width * aspect)), Vector2.one, Vector2.zero, current);
            for (var width = current.width * 2; width <= SmoothWidth; width *= 2)
                current = Blit(current, width, Mathf.Max(2, Mathf.RoundToInt(width * aspect)), Vector2.one, Vector2.zero, current);
            Cache[sprite] = current;
            return current;
        }
        public static void ReleaseAll()
        {
            foreach (var texture in Cache.Values) if (texture != null) { texture.Release(); Object.Destroy(texture); }
            Cache.Clear();
        }
        private static RenderTexture Blit(Texture source, int width, int height, Vector2 scale, Vector2 offset, RenderTexture disposeAfter)
        {
            var target = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            target.Create();
            Graphics.Blit(source, target, scale, offset);
            if (disposeAfter != null) { disposeAfter.Release(); Object.Destroy(disposeAfter); }
            return target;
        }
    }
}
