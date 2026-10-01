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

        private readonly List<SpriteRenderer> _discs = new List<SpriteRenderer>();
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
                var visibility = zone.Visibility(_runtime.Time);
                disc.enabled = visibility > 0f;
                if (!disc.enabled) continue;
                var color = zone.Effect.Color;
                color.a = DiscAlpha * visibility;
                disc.color = color;
                disc.transform.position = new Vector3(zone.Center.x, zone.Center.y, 0f);
                var scale = zone.Effect.Radius * 2f / _sprite.bounds.size.x;
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
