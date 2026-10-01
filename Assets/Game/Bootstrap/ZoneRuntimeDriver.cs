using System;
using System.Collections.Generic;
using Game.Run;
using Game.Zones;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Game.Bootstrap
{
    /// <summary>
    /// Scene side of the effect zones: ticks <see cref="ZoneRuntime"/> only while the run is Running (pause advances no zone time)
    /// and draws each zone as a placeholder disc whose alpha follows its pulse. Art replaces the discs later.
    /// </summary>
    public sealed class ZoneRuntimeDriver : MonoBehaviour
    {
        private const float DiscAlpha = 0.38f;
        private const int SortingOrder = -6;
        private const float IdleAltarVisibility = 0.25f;
        private const float StrikeWarningAlpha = 0.6f;
        private const float StrikeFlashAlpha = 0.9f;

        private readonly List<SpriteRenderer> _discs = new List<SpriteRenderer>();
        // Warning circles of strike altars: a reused list of what to draw and a grow-only pool of discs for them.
        private readonly List<StrikeCircle> _circles = new List<StrikeCircle>();
        private readonly List<SpriteRenderer> _strikeDiscs = new List<SpriteRenderer>();
        private ZoneRuntime _runtime;
        private RunController _run;
        private Sprite _sprite;

        public ZoneRuntime Runtime => _runtime;

        public static ZoneRuntimeDriver Create(ZoneRuntime runtime, RunController run, Scene scene)
        {
            var root = new GameObject("FieldZones");
            SceneManager.MoveGameObjectToScene(root, scene);
            var driver = root.AddComponent<ZoneRuntimeDriver>();
            driver.Initialize(runtime, run);
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

        private void Initialize(ZoneRuntime runtime, RunController run)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _run = run ?? throw new ArgumentNullException(nameof(run));
            _sprite = ZoneDiscSprite.Create();
            foreach (var zone in runtime.Zones)
            {
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
                disc.transform.localScale = new Vector3(scale, scale, 1f);
            }
            RefreshStrikes();
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
            _runtime?.Dispose();
            _runtime = null;
            if (_sprite != null)
            {
                var texture = _sprite.texture;
                if (Application.isPlaying) { Destroy(_sprite); Destroy(texture); } else { DestroyImmediate(_sprite); DestroyImmediate(texture); }
                _sprite = null;
            }
            if (this == null) return;
            if (Application.isPlaying) Destroy(gameObject); else DestroyImmediate(gameObject);
        }

        private void OnDestroy() => _runtime?.Dispose();
    }
}
