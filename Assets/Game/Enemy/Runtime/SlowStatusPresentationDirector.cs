using System.Collections.Generic;
using Game.Combat;
using Game.Diagnostics;
using Game.Presentation;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// Shows the selected slow look and the timed speed-boost look on living enemies with bound body art
    /// (DECISION-0108/0124). The bar-and-ice slow look is the production default.
    /// Reads gameplay status only; the development preview command explicitly applies a slow.
    /// </summary>
    public sealed class SlowStatusPresentationDirector : MonoBehaviour, ISlowStatusPreview
    {
        public event System.Action Changed;
        private const string PreviewChannel = "development-slow-preview";
        private readonly List<EnemyRuntime> _alive = new List<EnemyRuntime>();
        private readonly List<SlowStatusPresentationRuntime> _shown = new List<SlowStatusPresentationRuntime>();
        private readonly HashSet<SlowStatusPresentationRuntime> _shownNext = new HashSet<SlowStatusPresentationRuntime>();
        private readonly List<SpeedStatusPresentationRuntime> _speedShown = new List<SpeedStatusPresentationRuntime>();
        private readonly HashSet<SpeedStatusPresentationRuntime> _speedNext = new HashSet<SpeedStatusPresentationRuntime>();
        private SlowStatusPresentationProfile _profile;
        private bool _initialized;

        public SlowStatusStyle Style { get; private set; } = SlowStatusStyle.Ice;
        public int ShownCount => _shown.Count;

        public void Initialize(SlowStatusPresentationProfile profile)
        {
            if (_initialized) Shutdown();
            _profile = profile ?? throw new System.ArgumentNullException(nameof(profile));
            _initialized = true; // The selected preview style survives restarts within this session.
        }

        public void SetStyle(SlowStatusStyle style)
        {
            if (!System.Enum.IsDefined(typeof(SlowStatusStyle), style)) throw new System.ArgumentOutOfRangeException(nameof(style));
            Style = style;
            Refresh();
            Changed?.Invoke();
        }

        /// <summary>Development preview only: slows every living enemy with the profile's preview slow.</summary>
        public int SlowAllForPreview()
        {
            if (!_initialized) return 0;
            var request = new CombatDamageRequest(default, 0f,
                new CombatControlProfile(slowFraction: _profile.PreviewSlowFraction, slowSeconds: _profile.PreviewSlowSeconds, channel: PreviewChannel));
            EnemyRegistry.CopyAliveTo(_alive);
            var slowed = 0;
            foreach (var enemy in _alive)
            {
                enemy.ApplyControl(request);
                if (enemy.Controls.IsSlowed) slowed++;
            }
            Refresh();
            return slowed;
        }

        public void Refresh()
        {
            if (!_initialized) return;
            using (PerfGuard.Measure("Enemy.SlowStatusPresentation", 2f))
            {
                _shownNext.Clear();
                _speedNext.Clear();
                EnemyRegistry.CopyAliveTo(_alive);
                foreach (var enemy in _alive)
                {
                    var presentation = enemy.BodyPresentation;
                    if (presentation == null) continue;
                    if (Style != SlowStatusStyle.Off && enemy.Controls.IsSlowed)
                    {
                        var overlay = presentation.GetComponent<SlowStatusPresentationRuntime>();
                        if (overlay == null) overlay = presentation.gameObject.AddComponent<SlowStatusPresentationRuntime>();
                        overlay.Apply(presentation, _profile, Style, true, enemy.Controls.SlowRemaining01);
                        _shownNext.Add(overlay);
                    }
                    if (!enemy.Protection.HasSpeedBoost) continue;
                    var speedOverlay = presentation.GetComponent<SpeedStatusPresentationRuntime>();
                    if (speedOverlay == null) speedOverlay = presentation.gameObject.AddComponent<SpeedStatusPresentationRuntime>();
                    var slowBar = enemy.Controls.IsSlowed &&
                        (Style == SlowStatusStyle.Ice || Style == SlowStatusStyle.Bar || Style == SlowStatusStyle.All);
                    speedOverlay.Apply(presentation, _profile, true, enemy.Protection.SpeedBoostRemaining01,
                        slowBar, enemy.Protection.Now);
                    _speedNext.Add(speedOverlay);
                }
                // Anything shown last frame but not this one (slow expired, death, despawn, Off) is cleared.
                foreach (var overlay in _shown)
                    if (overlay != null && !_shownNext.Contains(overlay)) overlay.Clear();
                foreach (var overlay in _speedShown)
                    if (overlay != null && !_speedNext.Contains(overlay)) overlay.Clear();
                _shown.Clear();
                _shown.AddRange(_shownNext);
                _speedShown.Clear();
                _speedShown.AddRange(_speedNext);
            }
        }

        public void Shutdown()
        {
            foreach (var overlay in _shown) if (overlay != null) overlay.Clear();
            foreach (var overlay in _speedShown) if (overlay != null) overlay.Clear();
            _shown.Clear();
            _shownNext.Clear();
            _speedShown.Clear();
            _speedNext.Clear();
            _alive.Clear();
            _profile = null;
            _initialized = false;
        }

        private void LateUpdate() => Refresh();

        private void OnDestroy() => Shutdown();
    }
}
