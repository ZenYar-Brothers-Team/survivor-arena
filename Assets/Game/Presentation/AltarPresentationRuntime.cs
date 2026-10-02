using System;
using Game.Content;
using Game.Zones;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>One stationary visual-only altar: persistent body, boundary and charge/rest arc on the paused run clock.</summary>
    public sealed class AltarPresentationRuntime : MonoBehaviour
    {
        private const int Segments = 64;
        private readonly Vector3[] _points = new Vector3[Segments + 1];
        private SpriteRenderer _body;
        private LineRenderer _rim;
        private LineRenderer _progress;
        private Material _material;
        private AltarPresentationProfile _profile;
        private ZonePlacement _zone;
        private float _height;
        public SpriteRenderer Body => _body;
        public bool IsActive { get; private set; }
        public float Progress { get; private set; }

        public void Initialize(ZonePlacement zone, AltarPresentationProfile profile, ContentRegistry registry)
        {
            Shutdown();
            _zone = zone ?? throw new ArgumentNullException(nameof(zone));
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            var shrine = zone.Effect.Kind == ZoneEffectKind.Shrine;
            var definition = (shrine ? profile.Shrine : zone.Effect.Polarity == ZoneAltarPolarity.Positive ? profile.Positive : profile.Negative).Resolve(registry);
            definition.RequireRole(SpriteRole.Prop);
            var child = new GameObject("Body"); child.transform.SetParent(transform, false);
            _body = child.AddComponent<SpriteRenderer>();
            _body.sprite = definition.Sprite; _body.sortingOrder = profile.SortingOrder;
            _height = shrine ? profile.ShrineHeight : profile.AltarHeight;
            var scale = _height / definition.Sprite.bounds.size.y;
            child.transform.localScale = Vector3.one * scale;
            child.transform.localPosition = new Vector3(0f, -definition.Sprite.bounds.min.y * scale, 0f);
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) throw new InvalidOperationException("Altar ring shader missing.");
            _material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            _rim = CreateLine("Boundary", profile.SortingOrder - 2);
            _progress = CreateLine("ChargeOrRest", profile.SortingOrder - 1);
        }

        public void Apply(float runSeconds)
        {
            if (_zone == null) return;
            transform.position = new Vector3(_zone.Center.x, _zone.Center.y, 0f);
            _body.enabled = _zone.IsNear;
            IsActive = _zone.IsNear && _zone.IsActive(runSeconds);
            var strength = IsActive ? _zone.Visibility(runSeconds) : 0f;
            if (_zone.Effect.Kind == ZoneEffectKind.Shrine && IsActive) strength = Mathf.Max(strength, _zone.Charge);
            var brightness = Mathf.Lerp(_profile.IdleBrightness, _profile.ActiveBrightness, strength);
            _body.color = new Color(brightness, brightness, brightness, 1f);
            var rest = _zone.RestProgress(runSeconds);
            Progress = _zone.Effect.Kind == ZoneEffectKind.Shrine && _zone.ShrineCooldownRemaining <= 0f ? _zone.Charge : Mathf.Max(0f, rest);
            var color = _zone.Effect.Color;
            Draw(_rim, 1f, color, IsActive ? _profile.RingAlpha : _profile.RestingAlpha);
            Draw(_progress, Progress, color, _profile.RingAlpha);
        }

        private LineRenderer CreateLine(string name, int order)
        {
            var child = new GameObject(name); child.transform.SetParent(transform, false);
            var line = child.AddComponent<LineRenderer>();
            line.sharedMaterial = _material; line.useWorldSpace = false; line.loop = false;
            line.startWidth = line.endWidth = _profile.RingThickness;
            line.sortingOrder = order; line.enabled = false;
            return line;
        }

        private void Draw(LineRenderer line, float fraction, Color color, float alpha)
        {
            line.enabled = _zone.IsNear && fraction > 0f;
            if (!line.enabled) return;
            var steps = Mathf.Max(1, Mathf.CeilToInt(Segments * fraction));
            var radius = line == _rim ? _zone.Effect.Radius : _zone.Effect.Radius - _profile.RingThickness * 2f;
            for (var i = 0; i <= steps; i++)
            {
                var angle = Mathf.PI * .5f - Mathf.PI * 2f * fraction * i / steps;
                _points[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            }
            line.positionCount = steps + 1;
            // SetPosition avoids handing a grow-only buffer's unused tail to the renderer.
            for (var i = 0; i <= steps; i++) line.SetPosition(i, _points[i]);
            color.a = alpha; line.startColor = line.endColor = color;
        }

        public void Shutdown()
        {
            if (_body != null) DestroyOwned(_body.gameObject);
            if (_rim != null) DestroyOwned(_rim.gameObject);
            if (_progress != null) DestroyOwned(_progress.gameObject);
            if (_material != null) DestroyOwned(_material);
            _body = null; _rim = null; _progress = null; _material = null; _profile = null; _zone = null;
            IsActive = false; Progress = 0f;
        }

        private static void DestroyOwned(UnityEngine.Object obj)
        {
            if (Application.isPlaying) Destroy(obj); else DestroyImmediate(obj);
        }
        private void OnDestroy() => Shutdown();
    }
}
