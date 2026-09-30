using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Slow-status candidate looks on one sprite body (DECISION-0108, GI-09): a shrinking bar under the feet,
    /// a blue body tint, a light ice silhouette over the body and a blue outline behind it. Presentation only:
    /// the owner feeds the current slow state; the gameplay root, collider and movement are never touched.
    /// Overlays are children of the body root, so they follow the pose writer's squash/tilt. Colors come from
    /// unlit materials and facing from the overlay transform, because SpriteRenderer color and flipX are
    /// shader-side renderer properties that a custom silhouette shader does not receive.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SlowStatusPresentationRuntime : MonoBehaviour
    {
        private const string SolidColorShaderPath = "Shaders/SpriteSolidColor";
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private static readonly Vector2[] OutlineDirections =
        {
            new Vector2(1f, 0f), new Vector2(-1f, 0f), new Vector2(0f, 1f), new Vector2(0f, -1f),
            new Vector2(.7071f, .7071f), new Vector2(-.7071f, .7071f), new Vector2(.7071f, -.7071f), new Vector2(-.7071f, -.7071f)
        };
        private static readonly Dictionary<Color, Material> Materials = new Dictionary<Color, Material>();
        private static Shader _shader;
        private static Sprite _pixel;

        private SpritePresentationRuntime _presentation;
        private SpriteRenderer _ice;
        private readonly SpriteRenderer[] _outline = new SpriteRenderer[OutlineDirections.Length];
        private Transform _bar;
        private SpriteRenderer _barBack;
        private SpriteRenderer _barFill;
        private bool _built;

        public bool IsShowing { get; private set; }
        public SlowStatusStyle AppliedStyle { get; private set; }

        public void Apply(SpritePresentationRuntime presentation, SlowStatusPresentationProfile profile,
            SlowStatusStyle style, bool slowed, float remaining01)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (presentation == null || !presentation.IsInitialized || !slowed || style == SlowStatusStyle.Off)
            {
                Clear();
                return;
            }
            _presentation = presentation;
            Build(presentation.Rig);
            var body = presentation.Rig.BodyRenderer;
            var all = style == SlowStatusStyle.All;

            presentation.SetStatusTint(all || style == SlowStatusStyle.Tint ? profile.TintColor : Color.white);

            var ice = all || style == SlowStatusStyle.Ice;
            _ice.enabled = ice;
            if (ice) Mirror(_ice, body, profile.IceColor, body.sortingOrder + 1, Vector2.zero);

            var outline = all || style == SlowStatusStyle.Outline;
            var scale = Mathf.Max(1e-4f, Mathf.Abs(presentation.Rig.BodyRoot.lossyScale.x));
            for (var i = 0; i < _outline.Length; i++)
            {
                _outline[i].enabled = outline;
                if (outline) Mirror(_outline[i], body, profile.OutlineColor, body.sortingOrder - 1,
                    OutlineDirections[i] * (profile.OutlineWidth / scale));
            }

            var bar = all || style == SlowStatusStyle.Bar;
            _bar.gameObject.SetActive(bar);
            if (bar) LayoutBar(profile, body, Mathf.Clamp01(remaining01));

            IsShowing = true;
            AppliedStyle = style;
        }

        public void Clear()
        {
            if (_presentation != null && _presentation.IsInitialized) _presentation.SetStatusTint(Color.white);
            if (_built)
            {
                _ice.enabled = false;
                foreach (var renderer in _outline) renderer.enabled = false;
                _bar.gameObject.SetActive(false);
            }
            IsShowing = false;
            AppliedStyle = SlowStatusStyle.Off;
        }

        private void OnDisable() => Clear();

        private void Build(SpritePresentationRig rig)
        {
            if (_built) return;
            _ice = CreateRenderer("SlowIce", rig.BodyRoot);
            for (var i = 0; i < _outline.Length; i++) _outline[i] = CreateRenderer("SlowOutline" + i, rig.BodyRoot);
            _bar = new GameObject("SlowBar").transform;
            _bar.SetParent(rig.transform, false);
            _barBack = CreateRenderer("Back", _bar);
            _barFill = CreateRenderer("Fill", _bar);
            _barBack.sprite = _barFill.sprite = Pixel();
            _barBack.enabled = _barFill.enabled = true;
            _built = true;
        }

        private void LayoutBar(SlowStatusPresentationProfile profile, SpriteRenderer body, float remaining01)
        {
            var scale = _bar.parent.lossyScale;
            var sx = Mathf.Max(1e-4f, Mathf.Abs(scale.x));
            var sy = Mathf.Max(1e-4f, Mathf.Abs(scale.y));
            var width = profile.BarWidth / sx;
            var height = profile.BarHeight / sy;
            _bar.localPosition = new Vector3(0f, -profile.BarOffsetY / sy, 0f);
            Style(_barBack, body, profile.BarBackColor, body.sortingOrder + 2);
            _barBack.transform.localPosition = Vector3.zero;
            _barBack.transform.localScale = new Vector3(width, height, 1f);
            Style(_barFill, body, profile.BarFillColor, body.sortingOrder + 3);
            _barFill.transform.localPosition = new Vector3(-width * (1f - remaining01) * .5f, 0f, 0f);
            _barFill.transform.localScale = new Vector3(width * remaining01, height, 1f);
        }

        /// <summary>Copies the body sprite; facing is mirrored by the overlay's own X scale (see class summary).</summary>
        private static void Mirror(SpriteRenderer target, SpriteRenderer body, Color color, int order, Vector2 offset)
        {
            target.sprite = body.sprite;
            target.flipX = target.flipY = false;
            var facing = body.flipX ? -1f : 1f;
            target.transform.localScale = new Vector3(facing, body.flipY ? -1f : 1f, 1f);
            target.transform.localPosition = new Vector3(offset.x, offset.y, 0f);
            Style(target, body, color, order);
        }

        private static void Style(SpriteRenderer target, SpriteRenderer body, Color color, int order)
        {
            target.sharedMaterial = MaterialFor(color);
            target.color = Color.white;
            target.sortingLayerID = body.sortingLayerID;
            target.sortingOrder = order;
        }

        private static SpriteRenderer CreateRenderer(string name, Transform parent)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.enabled = false;
            return renderer;
        }

        public static Material MaterialFor(Color color)
        {
            if (Materials.TryGetValue(color, out var material) && material != null) return material;
            if (_shader == null) _shader = Resources.Load<Shader>(SolidColorShaderPath);
            if (_shader == null) throw new InvalidOperationException($"Missing shader Resources/{SolidColorShaderPath}.");
            material = new Material(_shader) { name = "Slow status " + ColorUtility.ToHtmlStringRGBA(color) };
            material.SetColor(ColorProperty, color);
            Materials[color] = material;
            return material;
        }

        private static Sprite Pixel()
        {
            if (_pixel != null) return _pixel;
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { name = "Slow status pixel", filterMode = FilterMode.Point };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply(false, true);
            return _pixel = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(.5f, .5f), 1f);
        }
    }
}
