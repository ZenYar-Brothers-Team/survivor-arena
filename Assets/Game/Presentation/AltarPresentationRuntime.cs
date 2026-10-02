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
        private const int BoundarySegments = 192;
        private readonly Vector3[] _points = new Vector3[BoundarySegments + 1];
        private SpriteRenderer _body;
        private LineRenderer _rim;
        private LineRenderer _weave;
        private LineRenderer _progress;
        private LineRenderer _stateRim;
        private MeshRenderer _glow;
        private Mesh _glowMesh;
        private readonly Color[] _glowColors = new Color[Segments + 2];
        private bool _hasState;
        private bool _previousActive;
        private float _flashAt = float.NegativeInfinity;
        private float? _previousApplication;
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
            var definition = profile.EffectVisuals[zone.Effect.Id].Resolve(registry);
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
            _weave = CreateLine("BoundaryWeave", profile.SortingOrder - 2);
            _stateRim = CreateLine("FoundationState", profile.SortingOrder - 2);
            _progress = CreateLine("ChargeOrRest", profile.SortingOrder - 1);
            _stateRim.startWidth = _stateRim.endWidth = profile.StateRingThickness;
            _progress.startWidth = _progress.endWidth = profile.StateRingThickness;
            CreateGlow();
        }

        public void Apply(float runSeconds)
        {
            if (_zone == null) return;
            transform.position = new Vector3(_zone.Center.x, _zone.Center.y, 0f);
            _body.enabled = _zone.IsNear;
            IsActive = _zone.IsNear && _zone.IsActive(runSeconds);
            // Foundation color stays constant; only the separate crown light conveys activity.
            _body.color = Color.white;
            var rest = _zone.RestProgress(runSeconds);
            var shrine = _zone.Effect.Kind == ZoneEffectKind.Shrine;
            if (shrine) Progress = _zone.ShrineCooldownRemaining <= 0f ? _zone.Charge : Mathf.Max(0f, rest);
            else if (rest >= 0f) Progress = rest;
            else
            {
                var phase = Mathf.Repeat(runSeconds + _zone.PhaseSeconds, _zone.Effect.PulsePeriodSeconds);
                Progress = 1f - Mathf.Clamp01(phase / _zone.Effect.PulseVisibleSeconds);
            }
            var active = _zone.IsActive(runSeconds);
            if (_hasState && ((active != _previousActive) || _zone.LastApplicationSeconds != _previousApplication))
                _flashAt = runSeconds;
            _hasState = true; _previousActive = active; _previousApplication = _zone.LastApplicationSeconds;
            var flash = Mathf.Clamp01(1f - (runSeconds - _flashAt) / _profile.FlashSeconds);
            var color = _zone.Effect.Color;
            Draw(_rim, 1f, color, IsActive ? _profile.RingAlpha : _profile.RestingAlpha);
            Draw(_weave, 1f, color, (IsActive ? _profile.RingAlpha : _profile.RestingAlpha) * _profile.BoundaryWeaveAlpha);
            Draw(_stateRim, 1f, color, Mathf.Lerp(_profile.RestingAlpha, 1f, flash));
            // The fill arc is drawn opaque in the bright crown color so it stands out from the dim foundation ring of the same hue.
            var fill = _profile.GlowColor;
            Draw(_progress, Progress, fill, 1f);
            var alpha = active ? (shrine ? Mathf.Lerp(_profile.ReadyGlowAlpha, _profile.ActiveGlowAlpha, _zone.Charge) : _profile.ActiveGlowAlpha) : 0f;
            _glow.enabled = _zone.IsNear && (alpha > 0f || flash > 0f);
            var glowColor = _profile.GlowColor; glowColor.a = Mathf.Lerp(alpha, 1f, flash);
            _glowColors[0] = glowColor;
            glowColor.a = 0f;
            for (var i = 1; i < _glowColors.Length; i++) _glowColors[i] = glowColor;
            _glowMesh.colors = _glowColors;
        }

        private void CreateGlow()
        {
            var child = new GameObject("CrownLight"); child.transform.SetParent(transform, false);
            child.transform.localPosition = Vector3.up * (_height * _profile.GlowHeightFraction);
            _glow = child.AddComponent<MeshRenderer>();
            _glow.sharedMaterial = _material; _glow.sortingOrder = _profile.SortingOrder + 1;
            var vertices = new Vector3[Segments + 2];
            var triangles = new int[Segments * 3];
            for (var i = 0; i <= Segments; i++)
            {
                var angle = i * Mathf.PI * 2f / Segments;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * _profile.GlowRadius;
                if (i == Segments) continue;
                triangles[i * 3] = 0; triangles[i * 3 + 1] = i + 2; triangles[i * 3 + 2] = i + 1;
            }
            _glowMesh = new Mesh { name = "Altar crown light", hideFlags = HideFlags.HideAndDontSave };
            _glowMesh.vertices = vertices; _glowMesh.triangles = triangles; _glowMesh.colors = _glowColors;
            child.AddComponent<MeshFilter>().sharedMesh = _glowMesh;
            _glow.enabled = false;
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
            var shrine = _zone.Effect.Kind == ZoneEffectKind.Shrine;
            var boundary = line == _rim || line == _weave;
            var steps = Mathf.Max(1, Mathf.CeilToInt((boundary ? BoundarySegments : Segments) * fraction));
            var radius = boundary ? _zone.Radius : shrine ? _profile.ShrineStateRingRadius : _profile.StateRingRadius;
            var aspect = boundary ? 1f : _profile.StateRingAspect;
            var offset = boundary ? 0f : shrine ? _profile.ShrineContactOffsetY : _profile.AltarContactOffsetY;
            for (var i = 0; i <= steps; i++)
            {
                var angle = Mathf.PI * .5f - Mathf.PI * 2f * fraction * i / steps;
                var localRadius = radius;
                if (boundary)
                {
                    // Opposing strands are centered on the exact gameplay reach (DECISION-0150).
                    // Their radial midpoint is Radius at every angle, including the crossings.
                    // Polarity changes color only; no effect-specific symbols or gameplay geometry changes.
                    var wave = Mathf.Cos((angle - Mathf.PI * .5f) * _profile.BoundaryLobes);
                    if (line == _weave) wave = -wave;
                    localRadius *= 1f + _profile.BoundaryInsetFraction * wave * .5f;
                }
                _points[i] = new Vector3(Mathf.Cos(angle) * localRadius, Mathf.Sin(angle) * localRadius * aspect + offset, 0f);
            }
            if (fraction >= 1f) _points[steps] = _points[0];
            line.positionCount = steps + 1;
            // SetPosition avoids handing a grow-only buffer's unused tail to the renderer.
            for (var i = 0; i <= steps; i++) line.SetPosition(i, _points[i]);
            color.a = alpha; line.startColor = line.endColor = color;
        }

        public void Shutdown()
        {
            if (_body != null) DestroyOwned(_body.gameObject);
            if (_rim != null) DestroyOwned(_rim.gameObject);
            if (_weave != null) DestroyOwned(_weave.gameObject);
            if (_progress != null) DestroyOwned(_progress.gameObject);
            if (_stateRim != null) DestroyOwned(_stateRim.gameObject);
            if (_glow != null) DestroyOwned(_glow.gameObject);
            if (_glowMesh != null) DestroyOwned(_glowMesh);
            if (_material != null) DestroyOwned(_material);
            _body = null; _rim = null; _progress = null; _material = null; _profile = null; _zone = null;
            _stateRim = null; _glow = null; _glowMesh = null;
            _weave = null;
            _hasState = false; _previousActive = false; _previousApplication = null; _flashAt = float.NegativeInfinity;
            IsActive = false; Progress = 0f;
        }

        private static void DestroyOwned(UnityEngine.Object obj)
        {
            if (Application.isPlaying) Destroy(obj); else DestroyImmediate(obj);
        }
        private void OnDestroy() => Shutdown();
    }
}
