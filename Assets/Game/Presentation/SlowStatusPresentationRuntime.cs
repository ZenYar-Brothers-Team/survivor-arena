using System;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Slow-status candidate looks on one sprite body (DECISION-0108, GI-09): a shrinking bar under the feet,
    /// a blue body tint, a light ice silhouette over the body and a blue outline behind it. Presentation only:
    /// the owner feeds the current slow state; the gameplay root, collider and movement are never touched.
    /// Overlays are children of the body root, so they follow the pose writer's squash/tilt/flip.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SlowStatusPresentationRuntime : MonoBehaviour
    {
        private const string SolidColorShaderPath = "Shaders/SpriteSolidColor";
        private static readonly Vector2[] OutlineDirections =
        {
            new Vector2(1f, 0f), new Vector2(-1f, 0f), new Vector2(0f, 1f), new Vector2(0f, -1f),
            new Vector2(.7071f, .7071f), new Vector2(-.7071f, .7071f), new Vector2(.7071f, -.7071f), new Vector2(-.7071f, -.7071f)
        };
        private static Material _solidColorMaterial;
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
            if (ice) Mirror(_ice, body, profile.IceColor, body.sortingOrder + 1, Vector3.zero);

            var outline = all || style == SlowStatusStyle.Outline;
            var scale = Mathf.Max(1e-4f, Mathf.Abs(presentation.Rig.BodyRoot.lossyScale.x));
            for (var i = 0; i < _outline.Length; i++)
            {
                _outline[i].enabled = outline;
                if (outline) Mirror(_outline[i], body, profile.OutlineColor, body.sortingOrder - 1,
                    (Vector3)(OutlineDirections[i] * (profile.OutlineWidth / scale)));
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
            var material = SolidColorMaterial();
            _ice = CreateRenderer("SlowIce", rig.BodyRoot, material);
            for (var i = 0; i < _outline.Length; i++) _outline[i] = CreateRenderer("SlowOutline" + i, rig.BodyRoot, material);
            _bar = new GameObject("SlowBar").transform;
            _bar.SetParent(rig.transform, false);
            _barBack = CreateRenderer("Back", _bar, rig.BodyRenderer.sharedMaterial);
            _barFill = CreateRenderer("Fill", _bar, rig.BodyRenderer.sharedMaterial);
            _barBack.sprite = _barFill.sprite = Pixel();
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

        private static void Mirror(SpriteRenderer target, SpriteRenderer body, Color color, int order, Vector3 offset)
        {
            target.sprite = body.sprite;
            target.flipX = body.flipX;
            target.flipY = body.flipY;
            target.transform.localPosition = offset;
            Style(target, body, color, order);
        }

        private static void Style(SpriteRenderer target, SpriteRenderer body, Color color, int order)
        {
            target.color = color;
            target.sortingLayerID = body.sortingLayerID;
            target.sortingOrder = order;
        }

        private static SpriteRenderer CreateRenderer(string name, Transform parent, Material material)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sharedMaterial = material;
            renderer.enabled = false;
            return renderer;
        }

        private static Material SolidColorMaterial()
        {
            if (_solidColorMaterial != null) return _solidColorMaterial;
            var shader = Resources.Load<Shader>(SolidColorShaderPath);
            if (shader == null) throw new InvalidOperationException($"Missing shader Resources/{SolidColorShaderPath}.");
            return _solidColorMaterial = new Material(shader) { name = "Slow status silhouette" };
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
