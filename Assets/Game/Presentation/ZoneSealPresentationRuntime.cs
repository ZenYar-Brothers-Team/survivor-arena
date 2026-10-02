using System;
using Game.Zones;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Approved irregular rim, scalable effect glyph and inner motion, driven by the paused zone clock.</summary>
    public sealed class ZoneSealPresentationRuntime : MonoBehaviour
    {
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private readonly MeshRenderer[] _layers = new MeshRenderer[3];
        private readonly Mesh[] _meshes = new Mesh[3];
        private MaterialPropertyBlock _properties;
        private Material _material;
        private Material _spriteMaterial;
        private Material _glyphMaterial;
        private ZoneSealPresentationProfile _profile;
        private SpriteRenderer _rimArt;
        private SpriteRenderer _glyphArt;
        public bool IsShowing { get; private set; }
        public bool IsActive { get; private set; }

        public void Initialize(ZoneEffectKind kind, ZoneSealPresentationProfile profile)
        {
            Shutdown();
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _properties = new MaterialPropertyBlock();
            var shader = Resources.Load<Shader>("Shaders/SpriteSolidColor");
            if (shader == null) throw new InvalidOperationException("Missing seal color shader.");
            _material = new Material(shader) { name = "Zone seal ink", hideFlags = HideFlags.HideAndDontSave,
                mainTexture = Texture2D.whiteTexture };
            var spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader == null) throw new InvalidOperationException("Missing unlit portal/seal sprite shader.");
            _spriteMaterial = new Material(spriteShader) { name = "Zone seal artwork", hideFlags = HideFlags.HideAndDontSave };
            _spriteMaterial.mainTexture = profile.RimSprite.texture;
            var builder = new ZoneSealMeshBuilder(profile.StrokeFraction);
            _meshes[0] = builder.Boundary(kind); _meshes[1] = builder.Glyph(kind); _meshes[2] = builder.Motion(kind, profile.MotionRadius);
            for (var i = 0; i < _layers.Length; i++)
            {
                var child = new GameObject(i == 0 ? "Rim" : i == 1 ? "Glyph" : "Motion");
                child.transform.SetParent(transform, false);
                child.AddComponent<MeshFilter>().sharedMesh = _meshes[i];
                var renderer = child.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = _material;
                // Every central symbol is above every zone's decorative motion, including overlapping zones.
                renderer.sortingOrder = profile.SortingOrder + (i == 1 ? 2 : i == 2 ? 1 : 0);
                renderer.enabled = false;
                _layers[i] = renderer;
            }
            {
                var art = new GameObject("Approved outline");
                art.transform.SetParent(_layers[0].transform, false);
                _rimArt = art.AddComponent<SpriteRenderer>();
                _rimArt.sprite = profile.RimSprite;
                // DECISION-0152: the gameplay ellipse crosses the middle of the thick painted contour.
                // Scale only the artwork; the authoritative radius and inner glyph keep their size.
                _rimArt.sharedMaterial = _spriteMaterial;
                _rimArt.sortingOrder = profile.SortingOrder;
                _rimArt.enabled = false;
            }
            var glyphSprite = kind == ZoneEffectKind.Experience ? profile.ExperienceGlyphSprite :
                kind == ZoneEffectKind.Knockback ? profile.KnockbackGlyphSprite :
                kind == ZoneEffectKind.Portal ? profile.PortalGlyphSprite : null;
            if (glyphSprite != null)
            {
                var glyph = new GameObject("Approved glyph");
                glyph.transform.SetParent(_layers[1].transform, false);
                glyph.transform.localScale = Vector3.one * (profile.RasterGlyphDiameterFraction /
                    Mathf.Max(glyphSprite.bounds.size.x, glyphSprite.bounds.size.y));
                _glyphArt = glyph.AddComponent<SpriteRenderer>();
                _glyphArt.sprite = glyphSprite;
                // Keep each raster texture bound to its own material, including when sprite batches change.
                _glyphMaterial = new Material(spriteShader) { name = "Zone seal approved glyph", hideFlags = HideFlags.HideAndDontSave,
                    mainTexture = glyphSprite.texture };
                _glyphArt.sharedMaterial = _glyphMaterial;
                _glyphArt.sortingOrder = profile.SortingOrder + 2;
                _glyphArt.enabled = false;
            }
            _layers[1].transform.localScale = Vector3.one * profile.GlyphScale;
        }

        public void Apply(ZonePlacement zone, float runSeconds, float portalCooldown)
        {
            if (_profile == null) return;
            var effect = zone.Effect;
            var flash = ApplicationFlash(zone, runSeconds);
            var visibility = zone.IsNear ? zone.Visibility(runSeconds) : 0f;
            IsShowing = visibility > 0f;
            IsActive = zone.IsNear && zone.IsActive(runSeconds) &&
                (effect.Lifetime != ZoneLifetimeMode.Burst || zone.RadiusScale(runSeconds) >= 1f);
            // Shared doors remain active while one actor's personal cooldown runs.
            var portalRest = effect.Kind == ZoneEffectKind.Portal && !effect.AffectsBothSides && portalCooldown > 0f;
            if (portalRest) { IsActive = false; visibility *= _profile.PortalRestVisibility; }
            if (!IsActive) visibility *= _profile.InactiveVisibilityMultiplier;
            if (zone.IsNear) visibility = Mathf.Max(visibility, flash);
            IsShowing = visibility > 0f;
            transform.position = new Vector3(zone.Center.x, zone.Center.y, 0f);
            // Flatten the parent after child rotation, keeping both the rim and rotating ink on the ground plane.
            transform.localScale = new Vector3(zone.Radius, zone.Radius * effect.VerticalScale, 1f);
            _rimArt.transform.localScale = Vector3.one * (2f * effect.ActiveRadiusFraction /
                (_profile.RimSprite.bounds.size.x * _profile.RimReferenceRadius));
            // Rotation precedes ground-plane flattening: only relocating seals turn their rim,
            // including faint idle/preparation states. The paused authoritative clock freezes it.
            _layers[0].transform.localRotation = Quaternion.Euler(0f, 0f,
                effect.RelocatesBetweenCycles ? runSeconds * _profile.RelocatingRimDegreesPerSecond : 0f);
            // A burst always advertises its final radius; its inner arcs swell to that rim.
            var size = effect.Lifetime == ZoneLifetimeMode.Burst ? zone.RadiusScale(runSeconds) : 1f;
            _layers[2].transform.localScale = Vector3.one * size;
            var moving = IsShowing && !portalRest && (IsActive || effect.IsPreparing(zone.PhaseSeconds, runSeconds) ||
                effect.Lifetime == ZoneLifetimeMode.Burst);
            var speed = effect.Kind == ZoneEffectKind.Slow ? _profile.SlowMotionMultiplier :
                effect.Kind == ZoneEffectKind.Haste ? _profile.HasteMotionMultiplier : 1f;
            _layers[2].transform.localRotation = Quaternion.Euler(0f, 0f, moving ? runSeconds * _profile.RotationDegreesPerSecond * speed : 0f);
            var flicker = moving ? 1f - _profile.FlickerAmount * (.5f + .5f * Mathf.Sin(runSeconds * _profile.FlickerFrequency * Mathf.PI * 2f + zone.Index)) : 1f;
            for (var i = 0; i < _layers.Length; i++)
            {
                var renderer = _layers[i];
                // Approved raster symbols replace the procedural interior, including its rotating arcs.
                // Otherwise burst arcs grow across the artwork and appear to replace its central symbol.
                renderer.enabled = IsShowing && (i != 2 || (moving && _glyphArt == null)) && (i != 1 || _glyphArt == null);
                var color = i == 1 ? Color.Lerp(effect.Color, _profile.LightColor, _profile.GlyphLightBlend)
                    : IsActive ? effect.Color : Color.Lerp(_profile.InkColor, effect.Color, _profile.IdleColorBlend);
                color = Color.Lerp(color, _profile.LightColor, flash * _profile.ApplicationFlashLightBlend);
                var alpha = i == 0 ? _profile.RimAlpha : i == 1 ? _profile.GlyphAlpha : _profile.MotionAlpha;
                color.a = visibility * alpha * (i == 0 ? 1f : flicker);
                _properties.SetColor(ColorProperty, color);
                renderer.SetPropertyBlock(_properties);
                if (i == 1 && _glyphArt != null)
                {
                    _glyphArt.enabled = IsShowing;
                    var glyphTint = Color.Lerp(Color.white, _profile.LightColor, flash * _profile.ApplicationFlashLightBlend);
                    glyphTint.a = color.a;
                    _glyphArt.color = glyphTint;
                }
                if (i == 0 && _rimArt != null)
                {
                    renderer.enabled = false;
                    _rimArt.enabled = IsShowing;
                    var tint = Color.Lerp(Color.white, color, _profile.RimTintBlend);
                    tint.a = color.a;
                    _rimArt.color = tint;
                }
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
            if (_spriteMaterial != null) Release(_spriteMaterial);
            if (_glyphMaterial != null) Release(_glyphMaterial);
            _glyphMaterial = null;
            _spriteMaterial = null;
            _rimArt = null;
            _glyphArt = null;
            _material = null; _profile = null; IsShowing = IsActive = false;
        }

        private static void Release(UnityEngine.Object value)
        {
            if (Application.isPlaying) Destroy(value); else DestroyImmediate(value);
        }

        private void OnDestroy() => Shutdown();
    }
}
