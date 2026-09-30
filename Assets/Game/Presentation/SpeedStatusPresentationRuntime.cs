using System;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Visual-only timed speed boost (DECISION-0120): an amber duration bar and a brief repeating bolt.
    /// The bar shares slow's dimensions and touches its lower edge when both controls are active.
    /// All children stay under VisualRoot; the gameplay root and collider are untouched.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SpeedStatusPresentationRuntime : MonoBehaviour
    {
        private static Sprite _pixel;
        private static Sprite _boltSprite;
        private Transform _bar;
        private SpriteRenderer _back, _fill, _bolt;
        private bool _built;

        public bool IsShowing { get; private set; }

        public void Apply(SpritePresentationRuntime presentation, SlowStatusPresentationProfile profile,
            bool boosted, float remaining01, bool belowSlow, float runSeconds)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (presentation == null || !presentation.IsInitialized || !boosted)
            {
                Clear();
                return;
            }
            var rig = presentation.Rig;
            Build(rig);
            var body = rig.BodyRenderer;
            var scale = _bar.parent.lossyScale;
            var width = profile.BarWidth / Mathf.Max(1e-4f, Mathf.Abs(scale.x));
            var height = profile.BarHeight / Mathf.Max(1e-4f, Mathf.Abs(scale.y));
            var center = new Vector3(body.bounds.center.x,
                body.bounds.min.y - profile.BarOffsetY - profile.BarHeight * (belowSlow ? 1.5f : .5f),
                body.transform.position.z);
            var local = _bar.parent.InverseTransformPoint(center);
            _bar.localPosition = new Vector3(local.x, local.y, 0f);
            _bar.gameObject.SetActive(true);
            Style(_back, body, profile.BarBackColor, body.sortingOrder + 2);
            Style(_fill, body, profile.SpeedBarFillColor, body.sortingOrder + 3);
            _back.transform.localScale = new Vector3(width, height, 1f);
            var remaining = Mathf.Clamp01(remaining01);
            _fill.transform.localPosition = new Vector3(-width * (1f - remaining) * .5f, 0f, 0f);
            _fill.transform.localScale = new Vector3(width * remaining, height, 1f);

            var spriteWidth = body.sprite != null ? body.sprite.bounds.size.x : 1f;
            var spriteBounds = body.sprite != null ? body.sprite.bounds : new Bounds(Vector3.zero, Vector3.one);
            _bolt.transform.localScale = Vector3.one * (profile.SpeedBoltScale * spriteWidth);
            _bolt.transform.localPosition = new Vector3(spriteBounds.max.x * .7f, spriteBounds.max.y * .25f, 0f);
            _bolt.sortingLayerID = body.sortingLayerID;
            _bolt.sortingOrder = body.sortingOrder + 2;
            _bolt.color = profile.SpeedBoltColor;
            var phase = Mathf.Repeat(runSeconds, profile.SpeedBlinkPeriod) / profile.SpeedBlinkPeriod;
            _bolt.enabled = phase < .19f || (phase > .29f && phase < .37f);
            IsShowing = true;
        }

        public void Clear()
        {
            if (_built) { _bar.gameObject.SetActive(false); _bolt.enabled = false; }
            IsShowing = false;
        }

        private void OnDisable() => Clear();

        private void Build(SpritePresentationRig rig)
        {
            if (_built) return;
            _bar = new GameObject("SpeedBar").transform;
            _bar.SetParent(rig.transform, false);
            _back = Create("Back", _bar, Pixel());
            _fill = Create("Fill", _bar, Pixel());
            _bolt = Create("SpeedBolt", rig.BodyRoot, Bolt());
            _built = true;
        }

        private static SpriteRenderer Create(string name, Transform parent, Sprite sprite)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.enabled = false;
            return renderer;
        }

        private static void Style(SpriteRenderer renderer, SpriteRenderer body, Color color, int order)
        {
            renderer.sharedMaterial = SlowStatusPresentationRuntime.MaterialFor(color);
            renderer.color = Color.white;
            renderer.sortingLayerID = body.sortingLayerID;
            renderer.sortingOrder = order;
            renderer.enabled = true;
        }

        private static Sprite Pixel()
        {
            if (_pixel != null) return _pixel;
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                { name = "Speed bar pixel", filterMode = FilterMode.Point, hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply(false, true);
            return _pixel = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1f);
        }

        private static Sprite Bolt()
        {
            if (_boltSprite != null) return _boltSprite;
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                { name = "Speed bolt", filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color32[size * size];
            var points = new[]
            {
                new Vector2(.12f, .44f), new Vector2(.42f, .44f), new Vector2(.22f, .91f),
                new Vector2(.76f, .91f), new Vector2(.57f, .58f), new Vector2(.87f, .58f), new Vector2(.31f, .08f)
            };
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
                pixels[y * size + x] = Inside(points, (x + .5f) / size, (y + .5f) / size)
                    ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return _boltSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
        }

        private static bool Inside(Vector2[] points, float x, float y)
        {
            var inside = false;
            for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
                if ((points[i].y > y) != (points[j].y > y) &&
                    x < (points[j].x - points[i].x) * (y - points[i].y) / (points[j].y - points[i].y) + points[i].x)
                    inside = !inside;
            return inside;
        }
    }
}
