using System;
using System.Collections.Generic;
using Game.Content;
using Game.Diagnostics;
using Game.Pooling;
using Game.Presentation;
using Game.Run;
using Game.ScreenEvents;
using Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Bootstrap
{
    /// <summary>Ticks FIELD-010 events only while Running and draws approved, pooled art over authoritative hazard geometry.</summary>
    public sealed class ScreenEventDriver : MonoBehaviour, IScreenEventDevelopmentControl
    {
        /// <summary>Development panel: pressing an event button starts it this many running seconds later.</summary>
        public const float DevelopmentLaunchDelaySeconds = 2f;

        private readonly List<ScreenHazardArtView> _views = new List<ScreenHazardArtView>();
        private ScreenEventRuntime _runtime;
        private RunController _run;
        private Func<Rect> _view;
        private Func<bool> _bossAlive;
        private ScreenEventArtResources _art;
        private GameObjectPool<ScreenHazardArtView> _pool;
        private bool _cleared;

        public ScreenEventRuntime Runtime => _runtime;

        private IReadOnlyList<ScreenEventDevelopmentEntry> _entries;
        public IReadOnlyList<ScreenEventDevelopmentEntry> Entries
        {
            get
            {
                if (_entries != null || _runtime == null) return _entries ?? Array.Empty<ScreenEventDevelopmentEntry>();
                var list = new List<ScreenEventDevelopmentEntry>();
                foreach (var definition in _runtime.Definition.Events.Values)
                    list.Add(new ScreenEventDevelopmentEntry(definition.Id.ToString(), definition.Name, definition.Rare));
                return _entries = list;
            }
        }

        public float LaunchDelaySeconds => DevelopmentLaunchDelaySeconds;
        public bool Busy => _runtime != null && (_runtime.Active != null || _runtime.QueuedEventId != null);
        public string ActiveName => _runtime?.Active?.Definition.Name;
        public float ActiveRemainingSeconds => _runtime?.Active == null ? 0f : Mathf.Max(0f, _runtime.Active.Duration - _runtime.Active.Elapsed);
        public string QueuedName => _runtime?.QueuedEventId == null ? null : _runtime.Definition.Events[_runtime.QueuedEventId].Name;
        public float QueuedInSeconds => _runtime?.QueuedInSeconds ?? 0f;
        public float Intensity => _runtime?.Intensity ?? 0f;
        public float SecondsUntilNext => _runtime?.SecondsUntilNext ?? 0f;
        public int PlayerHitCount => _runtime?.PlayerHitCount ?? 0;
        public int UnfairStartCount => _runtime?.UnfairStartCount ?? 0;
        public bool TryQueue(string eventId) => _runtime != null && _runtime.TryQueue(eventId, DevelopmentLaunchDelaySeconds);
        public int ActiveViewCount => _views.Count;
        public int PooledViewCount => _pool?.InactiveCount ?? 0;

        public static ScreenEventDriver Create(ScreenEventsDefinition definition, IScreenEventPlayerTarget player, RunController run,
            Scene scene, Func<Rect> view, Func<bool> bossAlive, int seed, ScreenEventPresentationProfile art, ContentRegistry registry)
        {
            var root = new GameObject("FieldScreenEvents");
            SceneManager.MoveGameObjectToScene(root, scene);
            var driver = root.AddComponent<ScreenEventDriver>();
            try { driver.Initialize(definition, player, run, view, bossAlive, seed, art, registry); }
            catch { driver.Shutdown(); throw; }
            return driver;
        }

        private void Initialize(ScreenEventsDefinition definition, IScreenEventPlayerTarget player, RunController run, Func<Rect> view,
            Func<bool> bossAlive, int seed, ScreenEventPresentationProfile art, ContentRegistry registry)
        {
            _run = run ?? throw new ArgumentNullException(nameof(run));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _bossAlive = bossAlive ?? throw new ArgumentNullException(nameof(bossAlive));
            _art = new ScreenEventArtResources(art, registry);
            _pool = new GameObjectPool<ScreenHazardArtView>(() =>
            {
                var child = new GameObject("Screen hazard view");
                child.transform.SetParent(transform, false);
                return child.AddComponent<ScreenHazardArtView>();
            }, transform);
            _runtime = new ScreenEventRuntime(definition, player, seed);
            _runtime.EventStarted += OnEventStarted;
            _runtime.EventFinished += OnEventFinished;
        }

        private void Update()
        {
            if (_runtime == null || _run.Model == null) return;
            var state = _run.Model.State;
            if (state == RunState.Won || state == RunState.Lost || state == RunState.Stopped)
            {
                if (!_cleared) { _runtime.Clear(); ClearViews(); _cleared = true; }
                return;
            }
            if (state != RunState.Running) return;
            _cleared = false;
            var view = _view();
            if (view.width > 0f) _runtime.Tick(Time.deltaTime, view, _bossAlive());
            Refresh(view);
        }

        private void OnEventStarted(ScreenEventInstance instance)
        {
            ClearViews();
            _art.Profile.SweepArtwork.TryGetValue(instance.Definition.Id, out var sweepKey);
            foreach (var hazard in instance.Hazards)
            {
                if (hazard.Kind == ScreenHazardKind.Sweep && sweepKey == null)
                    throw new InvalidOperationException("Missing approved sweep artwork for " + instance.Definition.Id);
                var view = _pool.Rent();
                _views.Add(view);
                view.Initialize(_art, hazard, sweepKey);
            }
            Refresh(_view());
        }

        private void OnEventFinished(ScreenEventInstance instance) => ClearViews();

        private void Refresh(Rect screen)
        {
            var instance = _runtime?.Active;
            if (instance == null) return;
            using (PerfGuard.Measure("ScreenEvents.Art", 2f))
                foreach (var view in _views) view.Apply(instance.Elapsed, screen);
        }

        private void ClearViews()
        {
            foreach (var view in _views)
            {
                view.Shutdown();
                _pool.Return(view);
            }
            _views.Clear();
        }

        private void Cleanup()
        {
            if (_runtime != null)
            {
                _runtime.EventStarted -= OnEventStarted;
                _runtime.EventFinished -= OnEventFinished;
                _runtime.Clear();
                _runtime = null;
            }
            ClearViews();
            _pool = null;
            _art?.Dispose();
            _art = null;
            _run = null;
            _view = null;
            _bossAlive = null;
        }

        /// <summary>Unsubscribes, disposes the artwork resources and removes the pool; safe after partial initialization.</summary>
        public void Shutdown()
        {
            Cleanup();
            if (this == null) return;
            if (Application.isPlaying) Destroy(gameObject); else DestroyImmediate(gameObject);
        }

        private void OnDestroy() => Cleanup();
    }
}
