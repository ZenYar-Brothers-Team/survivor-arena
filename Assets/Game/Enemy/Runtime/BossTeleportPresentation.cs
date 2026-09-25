using System;
using Game.Presentation;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// Ground effect of a boss teleport-slam (DECISION-0059): during the telegraph a ring marks the impact area
    /// and a disc fills it toward the slam; on impact a flash and an expanding shockwave ring fade out.
    /// Procedural shapes only, colors/timings from the teleport profile; paused runs advance nothing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BossTeleportPresentation : MonoBehaviour
    {
        /// <summary>Above ground shadows, below player skill world effects.</summary>
        public const int SortingOrder = 4;
        private const float WaveStartScale = .45f;
        private const float WaveEndScale = 1.2f;

        private SpriteRenderer _edge;
        private SpriteRenderer _fill;
        private SpriteRenderer _wave;
        private BossTeleportProfile _profile;
        private float _telegraphElapsed;
        private float _impactElapsed;

        public bool IsTelegraphVisible { get; private set; }
        public bool IsImpactVisible { get; private set; }

        public void ShowTelegraph(BossTeleportProfile profile, Vector2 landing)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            EnsureRenderers();
            transform.position = landing;
            _telegraphElapsed = 0f;
            IsTelegraphVisible = true;
            IsImpactVisible = false;
            _wave.enabled = false;
            Render();
        }

        public void PlayImpact(BossTeleportProfile profile, Vector2 landing)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            EnsureRenderers();
            transform.position = landing;
            _impactElapsed = 0f;
            IsTelegraphVisible = false;
            IsImpactVisible = true;
            Render();
        }

        public void Tick(float deltaTime, bool isRunning)
        {
            if (!isRunning || _profile == null || (!IsTelegraphVisible && !IsImpactVisible)) return;
            if (IsTelegraphVisible) _telegraphElapsed += deltaTime;
            if (IsImpactVisible)
            {
                _impactElapsed += deltaTime;
                if (_impactElapsed >= _profile.ImpactEffectSeconds) IsImpactVisible = false;
            }
            Render();
        }

        public void ResetPresentation()
        {
            IsTelegraphVisible = false;
            IsImpactVisible = false;
            _telegraphElapsed = _impactElapsed = 0f;
            if (_edge != null) Render();
        }

        private void Render()
        {
            var diameter = _profile != null ? _profile.ImpactRadius * 2f : 0f;
            if (IsTelegraphVisible)
            {
                var progress = Mathf.Clamp01(_telegraphElapsed / _profile.TelegraphSeconds);
                var color = _profile.TelegraphColor;
                // The edge pulses faster as the slam approaches; the fill shows how much time is left.
                var pulse = .65f + .35f * Mathf.Abs(Mathf.Sin(_telegraphElapsed * Mathf.PI * (2f + 4f * progress)));
                Show(_edge, diameter, WithAlpha(color, color.a * pulse));
                Show(_fill, diameter * Mathf.Lerp(.15f, 1f, progress), WithAlpha(color, color.a * .35f));
                _wave.enabled = false;
                return;
            }
            if (IsImpactVisible)
            {
                var t = Mathf.Clamp01(_impactElapsed / _profile.ImpactEffectSeconds);
                var easeOut = 1f - (1f - t) * (1f - t);
                var color = _profile.ImpactColor;
                Show(_wave, diameter * Mathf.Lerp(WaveStartScale, WaveEndScale, easeOut), WithAlpha(color, color.a * (1f - t)));
                Show(_fill, diameter, WithAlpha(color, color.a * .5f * (1f - easeOut)));
                _edge.enabled = false;
                return;
            }
            _edge.enabled = _fill.enabled = _wave.enabled = false;
        }

        private static void Show(SpriteRenderer renderer, float diameter, Color color)
        {
            renderer.enabled = true;
            renderer.transform.localScale = Vector3.one * diameter;
            renderer.color = color;
        }

        private static Color WithAlpha(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);

        private void EnsureRenderers()
        {
            if (_edge != null) return;
            _fill = CreateRenderer("TeleportFill", ProceduralShapeSprites.Disc, SortingOrder);
            _edge = CreateRenderer("TeleportEdge", ProceduralShapeSprites.Ring, SortingOrder + 1);
            _wave = CreateRenderer("TeleportWave", ProceduralShapeSprites.Ring, SortingOrder + 1);
        }

        private SpriteRenderer CreateRenderer(string name, Sprite sprite, int order)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            renderer.enabled = false;
            return renderer;
        }
    }
}
