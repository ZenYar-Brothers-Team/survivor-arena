using System;
using Game.Zones;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Three transparent mesh layers per seal, driven solely by the paused zone clock and authoritative state.</summary>
    public sealed class ZoneSealPresentationRuntime : MonoBehaviour
    {
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private readonly MeshRenderer[] _layers = new MeshRenderer[3];
        private readonly Mesh[] _meshes = new Mesh[3];
        private MaterialPropertyBlock _properties;
        private Material _material;
        private ZoneSealPresentationProfile _profile;
        public bool IsShowing { get; private set; }
        public bool IsActive { get; private set; }

        public void Initialize(ZoneEffectKind kind, ZoneSealPresentationProfile profile)
        {
            Shutdown();
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _properties = new MaterialPropertyBlock();
            var shader = Resources.Load<Shader>("Shaders/SpriteSolidColor");
            if (shader == null) throw new InvalidOperationException("Missing seal color shader.");
            _material = new Material(shader) { name = "Zone seal ink", hideFlags = HideFlags.HideAndDontSave };
            var builder = new ZoneSealMeshBuilder(profile.StrokeFraction);
            _meshes[0] = builder.Boundary(kind); _meshes[1] = builder.Glyph(kind); _meshes[2] = builder.Motion(kind, profile.MotionRadius);
            for (var i = 0; i < _layers.Length; i++)
            {
                var child = new GameObject(i == 0 ? "Rim" : i == 1 ? "Glyph" : "Motion");
                child.transform.SetParent(transform, false);
                child.AddComponent<MeshFilter>().sharedMesh = _meshes[i];
                var renderer = child.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = _material;
                renderer.sortingOrder = profile.SortingOrder + i;
                renderer.enabled = false;
                _layers[i] = renderer;
            }
        }

        public void Apply(ZonePlacement zone, float runSeconds, float portalCooldown)
        {
            if (_profile == null) return;
            var effect = zone.Effect;
            var flash = ApplicationFlash(zone, runSeconds);
            var visibility = zone.IsNear ? zone.Visibility(runSeconds) : 0f;
            IsShowing = visibility > 0f;
            IsActive = zone.IsNear && zone.IsActive(runSeconds) &&
                (effect.Kind != ZoneEffectKind.SpeedBurst || zone.RadiusScale(runSeconds) >= 1f);
            var portalRest = effect.Kind == ZoneEffectKind.Portal && portalCooldown > 0f;
            if (portalRest) { IsActive = false; visibility *= _profile.PortalRestVisibility; }
            if (zone.IsNear) visibility = Mathf.Max(visibility, flash);
            IsShowing = visibility > 0f;
            transform.position = new Vector3(zone.Center.x, zone.Center.y, 0f);
            // Flatten the parent after child rotation, keeping both the rim and rotating ink on the ground plane.
            transform.localScale = new Vector3(effect.Radius, effect.Radius * effect.VerticalScale, 1f);
            // Rotation precedes ground-plane flattening: only relocating seals turn their rim,
            // including faint idle/preparation states. The paused authoritative clock freezes it.
            _layers[0].transform.localRotation = Quaternion.Euler(0f, 0f,
                effect.RelocatesBetweenCycles ? runSeconds * _profile.RelocatingRimDegreesPerSecond : 0f);
            // A burst always advertises its final radius; its inner arcs swell to that rim.
            var size = effect.Kind == ZoneEffectKind.SpeedBurst ? zone.RadiusScale(runSeconds) : 1f;
            _layers[2].transform.localScale = Vector3.one * size;
            var moving = IsShowing && !portalRest && (IsActive || effect.IsPreparing(zone.PhaseSeconds, runSeconds) ||
                effect.Kind == ZoneEffectKind.SpeedBurst);
            var speed = effect.Kind == ZoneEffectKind.Slow ? _profile.SlowMotionMultiplier :
                effect.Kind == ZoneEffectKind.Haste ? _profile.HasteMotionMultiplier : 1f;
            _layers[2].transform.localRotation = Quaternion.Euler(0f, 0f, moving ? runSeconds * _profile.RotationDegreesPerSecond * speed : 0f);
            var flicker = moving ? 1f - _profile.FlickerAmount * (.5f + .5f * Mathf.Sin(runSeconds * _profile.FlickerFrequency * Mathf.PI * 2f + zone.Index)) : 1f;
            for (var i = 0; i < _layers.Length; i++)
            {
                var renderer = _layers[i];
                renderer.enabled = IsShowing && (i != 2 || moving);
                var color = i == 1 ? Color.Lerp(effect.Color, _profile.LightColor, _profile.GlyphLightBlend)
                    : IsActive ? effect.Color : Color.Lerp(_profile.InkColor, effect.Color, _profile.IdleColorBlend);
                color = Color.Lerp(color, _profile.LightColor, flash * _profile.ApplicationFlashLightBlend);
                var alpha = i == 0 ? _profile.RimAlpha : i == 1 ? _profile.GlyphAlpha : _profile.MotionAlpha;
                color.a = visibility * alpha * (i == 0 ? 1f : flicker);
                _properties.SetColor(ColorProperty, color);
                renderer.SetPropertyBlock(_properties);
            }
        }

        private float ApplicationFlash(ZonePlacement zone, float runSeconds)
        {
            var elapsed = zone.LastApplicationSeconds.HasValue ? runSeconds - zone.LastApplicationSeconds.Value : float.PositiveInfinity;
            if (zone.Effect.Lifetime == ZoneLifetimeMode.Burst)
            {
                var phase = Mathf.Repeat(runSeconds + zone.PhaseSeconds, zone.Effect.PulsePeriodSeconds);
                elapsed = phase - zone.Effect.TelegraphSeconds;
            }
            return elapsed >= 0f ? Mathf.Clamp01(1f - elapsed / _profile.ApplicationFlashSeconds) : 0f;
        }

        public void Shutdown()
        {
            for (var i = 0; i < _layers.Length; i++)
            {
                if (_layers[i] != null) { _layers[i].enabled = false; Release(_layers[i].gameObject); }
                if (_meshes[i] != null) Release(_meshes[i]);
                _layers[i] = null; _meshes[i] = null;
            }
            if (_material != null) Release(_material);
            _material = null; _profile = null; IsShowing = IsActive = false;
        }

        private static void Release(UnityEngine.Object value)
        {
            if (Application.isPlaying) Destroy(value); else DestroyImmediate(value);
        }

        private void OnDestroy() => Shutdown();
    }
}
