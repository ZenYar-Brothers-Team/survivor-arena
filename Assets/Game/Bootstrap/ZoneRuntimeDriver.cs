using System;
using System.Collections.Generic;
using Game.Run;
using Game.Zones;
using Game.Presentation;
using Game.Diagnostics;
using Game.Content;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Game.Bootstrap
{
    /// <summary>
    /// Scene side of the effect zones: ticks <see cref="ZoneRuntime"/> only while the run is Running (pause advances no zone time)
    /// and draws academy seals for prepared layouts, retaining placeholder discs for the separate altar study.
    /// </summary>
    public sealed class ZoneRuntimeDriver : MonoBehaviour
    {
        private const float DiscAlpha = 0.38f;
        private const int SortingOrder = -6;
        private const float IdleAltarVisibility = 0.25f;
        private const float StrikeWarningAlpha = 0.6f;
        private const float StrikeFlashAlpha = 0.9f;

        private readonly List<SpriteRenderer> _discs = new List<SpriteRenderer>();
        private readonly List<ZoneSealPresentationRuntime> _seals = new List<ZoneSealPresentationRuntime>();
        private readonly List<AltarPresentationRuntime> _altars = new List<AltarPresentationRuntime>();
        private SpritePresentationRuntime _playerPresentation;
        private SpeedStatusPresentationRuntime _playerSpeed;
        private SlowStatusPresentationProfile _speedProfile;
        private ZoneSealPresentationProfile _sealProfile;
        private readonly HashSet<ZoneRiftHitPresentationRuntime> _riftHits = new HashSet<ZoneRiftHitPresentationRuntime>();
        private readonly List<ZoneRiftHitPresentationRuntime> _expiredHits = new List<ZoneRiftHitPresentationRuntime>();
        // Warning circles of strike altars: a reused list of what to draw and a grow-only pool of discs for them.
        private readonly List<StrikeCircle> _circles = new List<StrikeCircle>();
        private readonly List<SpriteRenderer> _strikeDiscs = new List<SpriteRenderer>();
        private ZoneRuntime _runtime;
        private RunController _run;
        private Sprite _sprite;

        public ZoneRuntime Runtime => _runtime;

        public static ZoneRuntimeDriver Create(ZoneRuntime runtime, RunController run, Scene scene,
            SpritePresentationRuntime playerPresentation = null, AltarPresentationProfile altarProfile = null,
            ContentRegistry registry = null)
        {
            var root = new GameObject("FieldZones");
            SceneManager.MoveGameObjectToScene(root, scene);
            var driver = root.AddComponent<ZoneRuntimeDriver>();
            try { driver.Initialize(runtime, run, playerPresentation, altarProfile, registry); }
            catch { driver.Shutdown(); throw; }
            return driver;
        }

        /// <summary>World rectangle an orthographic camera shows (empty without a camera).</summary>
        public static Rect CameraRect(Camera camera)
        {
            if (camera == null) return default;
            var height = camera.orthographicSize * 2f;
            var width = height * camera.aspect;
            var center = camera.transform.position;
            return new Rect(center.x - width * .5f, center.y - height * .5f, width, height);
        }

        private void Initialize(ZoneRuntime runtime, RunController run, SpritePresentationRuntime playerPresentation,
            AltarPresentationProfile altarProfile, ContentRegistry registry)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _run = run ?? throw new ArgumentNullException(nameof(run));
            _sprite = ZoneDiscSprite.Create();
            var useSeals = false;
            foreach (var zone in runtime.Zones) if (zone.Effect.HasPreparation) { useSeals = true; break; }
            var profile = useSeals ? ZoneSealPresentationProfile.Load() : null;
            _sealProfile = profile;
            _playerPresentation = useSeals ? playerPresentation : null;
            if (_playerPresentation != null)
            {
                _speedProfile = FixtureSlowStatusPresentationCatalog.Create();
                _playerSpeed = _playerPresentation.GetComponent<SpeedStatusPresentationRuntime>() ??
                    _playerPresentation.gameObject.AddComponent<SpeedStatusPresentationRuntime>();
            }
            foreach (var zone in runtime.Zones)
            {
                if (altarProfile != null)
                {
                    var visual = new GameObject($"Altar-{zone.Index}-{zone.Effect.Id}");
                    visual.transform.SetParent(transform, false);
                    var altar = visual.AddComponent<AltarPresentationRuntime>();
                    _altars.Add(altar);
                    altar.Initialize(zone, altarProfile, registry);
                    continue;
                }
                if (useSeals)
                {
                    var visual = new GameObject($"Zone-{zone.Index}-{zone.Effect.Id}");
                    visual.transform.SetParent(transform, false);
                    var seal = visual.AddComponent<ZoneSealPresentationRuntime>();
                    _seals.Add(seal);
                    seal.Initialize(zone.Effect.Kind, profile);
                    continue;
                }
                var disc = new GameObject($"Zone-{zone.Index}-{zone.Effect.Id}").AddComponent<SpriteRenderer>();
                disc.transform.SetParent(transform, false);
                disc.sprite = _sprite;
                disc.sortingOrder = SortingOrder;
                _discs.Add(disc);
            }
            Refresh();
        }

        private void Update()
        {
            if (_runtime == null) return;
            if (_run.Model != null && _run.Model.State == RunState.Running) _runtime.Tick(Time.deltaTime);
            Refresh();
        }

        private void Refresh()
        {
            using var guard = PerfGuard.Measure("Zones.Presentation", 2f);
            _expiredHits.Clear();
            foreach (var altar in _altars) altar.Apply(_runtime.Time);
            foreach (var hit in _riftHits)
            {
                if (hit != null) hit.Tick(_runtime.Time);
                if (hit == null || !hit.IsShowing) _expiredHits.Add(hit);
            }
            foreach (var hit in _expiredHits) _riftHits.Remove(hit);
            for (var i = 0; i < _seals.Count; i++)
                _seals[i].Apply(_runtime.Zones[i], _runtime.Time, _runtime.PortalCooldownRemaining);
            if (_playerSpeed != null)
                _playerSpeed.Apply(_playerPresentation, _speedProfile, _runtime.SpeedBuffRemaining01 > 0f,
                    _runtime.SpeedBuffRemaining01, false, _runtime.Time);
            for (var i = 0; i < _discs.Count; i++)
            {
                var zone = _runtime.Zones[i];
                var disc = _discs[i];
                // Only zones within the active window around the player are drawn (and simulated).
                var visibility = zone.IsNear ? zone.Visibility(_runtime.Time) : 0f;
                // Altars stay faintly drawn while resting so the player can find them; a charging altar brightens as it fills.
                if (zone.IsNear && zone.Effect.AlwaysShown)
                    visibility = Mathf.Max(visibility, IdleAltarVisibility + (1f - IdleAltarVisibility) * zone.Charge);
                // A shrine that just fired rests dimly until its cooldown is over.
                if (zone.IsNear && zone.Effect.Kind == ZoneEffectKind.Shrine && zone.ShrineCooldownRemaining > 0f)
                    visibility = IdleAltarVisibility;
                disc.enabled = visibility > 0f;
                if (!disc.enabled) continue;
                var color = zone.Effect.Color;
                color.a = DiscAlpha * visibility;
                disc.color = color;
                disc.transform.position = new Vector3(zone.Center.x, zone.Center.y, 0f);
                var scale = zone.Effect.Radius * 2f / _sprite.bounds.size.x * zone.RadiusScale(_runtime.Time);
                disc.transform.localScale = new Vector3(scale, scale * zone.Effect.VerticalScale, 1f);
            }
            RefreshStrikes();
        }

        /// <summary>Called only after the enemy adapter applies actual Rift damage, never on mere zone entry.</summary>
        public void ShowRiftHit(SpritePresentationRuntime body)
        {
            if (_sealProfile == null || _runtime == null || body == null || !body.IsInitialized) return;
            var hit = body.GetComponent<ZoneRiftHitPresentationRuntime>() ?? body.gameObject.AddComponent<ZoneRiftHitPresentationRuntime>();
            hit.Show(body, _sealProfile, _runtime.Time);
            _riftHits.Add(hit);
        }

        private void RefreshStrikes()
        {
            _runtime.CollectStrikeCircles(_circles);
            while (_strikeDiscs.Count < _circles.Count)
            {
                var disc = new GameObject($"Strike-{_strikeDiscs.Count}").AddComponent<SpriteRenderer>();
                disc.transform.SetParent(transform, false);
                disc.sprite = _sprite;
                disc.sortingOrder = SortingOrder + 1;
                _strikeDiscs.Add(disc);
            }
            for (var i = 0; i < _strikeDiscs.Count; i++)
            {
                var disc = _strikeDiscs[i];
                disc.enabled = i < _circles.Count;
                if (!disc.enabled) continue;
                var circle = _circles[i];
                var color = circle.Zone.Effect.Color;
                // The warning fills in as the strike nears; the flash grows a little and fades out.
                var alpha = circle.Flash > 0f ? StrikeFlashAlpha * (1f - circle.Flash) : StrikeWarningAlpha * circle.Telegraph;
                color.a = alpha;
                disc.color = color;
                disc.transform.position = new Vector3(circle.Center.x, circle.Center.y, 0f);
                var scale = circle.Radius * 2f / _sprite.bounds.size.x * (1f + ZoneEffectDefinition.BurstFlashGrowth * circle.Flash);
                disc.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        /// <summary>Removes the zone effects and the scene objects; safe to call more than once.</summary>
        public void Shutdown()
        {
            ClearOwned();
            if (this == null) return;
            if (Application.isPlaying) Destroy(gameObject); else DestroyImmediate(gameObject);
        }

        private void ClearOwned()
        {
            foreach (var hit in _riftHits) if (hit != null) hit.Clear();
            _riftHits.Clear(); _expiredHits.Clear(); _sealProfile = null;
            foreach (var seal in _seals) if (seal != null) seal.Shutdown();
            _seals.Clear();
            foreach (var altar in _altars) if (altar != null) altar.Shutdown();
            _altars.Clear();
            if (_playerSpeed != null)
            {
                _playerSpeed.Clear();
            }
            _playerSpeed = null; _playerPresentation = null; _speedProfile = null;
            _runtime?.Dispose();
            _runtime = null;
            if (_sprite != null)
            {
                var texture = _sprite.texture;
                if (Application.isPlaying) { Destroy(_sprite); Destroy(texture); } else { DestroyImmediate(_sprite); DestroyImmediate(texture); }
                _sprite = null;
            }
        }

        private void OnDestroy() => ClearOwned();
    }
}
