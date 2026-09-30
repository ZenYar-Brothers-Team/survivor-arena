using Game.Presentation;
using UnityEngine;
namespace Game.Traveler
{
    /// <summary>
    /// World effect of Traveler abilities: a one-shot pulse or a steady outline that follows its owner.
    /// Approved circular aura art rotates inside one flattened root, so its ground ellipse stays at the authored support ratio.
    /// Advances only through <see cref="Tick"/>, so a paused run freezes it; instances are pooled by the encounter.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TravelerPulseEffect : MonoBehaviour
    {
        /// <summary>Above ground shadows, below player skill world effects (same layer as boss teleport ground effects).</summary>
        public const int SortingOrder = 4;
        private const float StartScale = .35f;
        private SpriteRenderer _disc, _main, _second, _third;
        private Transform _scaleRoot, _follow;
        private Color _color;
        private TravelerEffectShape _shape;
        private float _diameter, _seconds, _elapsed, _clock, _verticalScale = 1f;
        private bool _steady;
        public bool IsFinished => !_steady && _elapsed >= _seconds;
        public static TravelerPulseEffect CreateInstance() => new GameObject("TravelerPulseEffect").AddComponent<TravelerPulseEffect>();
        public void PlayPulse(Vector2 position, float diameter, Color color, float seconds, float verticalScale = 1f, TravelerEffectShape shape = TravelerEffectShape.Ring)
        {
            Begin(diameter, color, verticalScale, shape); transform.position = position;
            _follow = null; _steady = false; _seconds = Mathf.Max(.01f, seconds);
            Render();
        }
        public void ShowSteady(Transform follow, float diameter, Color color, float verticalScale = 1f, TravelerEffectShape shape = TravelerEffectShape.Ring)
        {
            Begin(diameter, color, verticalScale, shape);
            _follow = follow; _steady = true;
            if (follow != null) transform.position = follow.position;
            Render();
        }
        public void Tick(float deltaTime)
        {
            if (_follow != null) transform.position = _follow.position;
            _clock += deltaTime;
            if (!_steady) _elapsed += deltaTime;
            Render();
        }
        private void Begin(float diameter, Color color, float verticalScale, TravelerEffectShape shape)
        {
            EnsureRenderers();
            _diameter = diameter; _color = color; _verticalScale = verticalScale; _shape = shape; _elapsed = 0; _clock = 0;
            var outline = shape == TravelerEffectShape.Hexagon ? TravelerShapeSprites.ShieldArt :
                shape == TravelerEffectShape.Dots ? TravelerShapeSprites.SpeedArt :
                shape == TravelerEffectShape.Ripples ? TravelerShapeSprites.HealArt : ProceduralShapeSprites.Ring;
            _main.sprite = outline;
            // Keep the secondary wave geometric: duplicating detailed art clutters a crowded field.
            _second.sprite = ProceduralShapeSprites.Ring;
            _third.sprite = ProceduralShapeSprites.Ring;
        }
        private void Render()
        {
            // Flattened like the gameplay zone: the ground ellipse of a 3/4 camera (DECISION-0058).
            _scaleRoot.localScale = new Vector3(_diameter, _diameter * _verticalScale, 1f);
            if (_steady)
            {
                Set(_disc, 1f, 0f, .06f);
                // The outer ornament breathes inward only, keeping the visible boundary inside the authored support radius.
                Set(_main, .975f + .025f * Mathf.Sin(_clock * 2f), _clock * 10f, .62f);
                if (_shape == TravelerEffectShape.Hexagon) Set(_second, .73f, -_clock * 16f, .28f); else Set(_second, 0f, 0f, 0f);
                Set(_third, 0f, 0f, 0f);
                return;
            }
            var t = Mathf.Clamp01(_elapsed / _seconds);
            var eased = 1f - (1f - t) * (1f - t);
            var scale = Mathf.Lerp(StartScale, 1f, eased);
            switch (_shape)
            {
                case TravelerEffectShape.Ripples:
                    Set(_disc, Mathf.Lerp(.5f, 1f, eased), 0f, .13f * (1f - t));
                    Ripple(_main, t, 0f); Ripple(_second, t, .32f);
                    Set(_third, 0f, 0f, 0f);
                    break;
                case TravelerEffectShape.Dots:
                    Set(_disc, scale, 0f, .13f * (1f - t));
                    Set(_main, scale, t * 200f, 1f - t * t);
                    Set(_second, scale, 0f, .25f * (1f - t)); Set(_third, 0f, 0f, 0f);
                    break;
                case TravelerEffectShape.Hexagon:
                    Set(_disc, scale, 0f, .15f * (1f - t));
                    Set(_main, scale, t * 60f, 1f - t); Set(_second, scale * .73f, -t * 60f, .35f * (1f - t)); Set(_third, 0f, 0f, 0f);
                    break;
                default:
                    Set(_disc, scale, 0f, .35f * (1f - t));
                    Set(_main, scale, 0f, 1f - t); Set(_second, 0f, 0f, 0f); Set(_third, 0f, 0f, 0f);
                    break;
            }
        }
        // One ring of the ripple group: starts after `delay` of the pulse and spreads to the full size while fading.
        private void Ripple(SpriteRenderer ring, float t, float delay)
        {
            if (t < delay) { Set(ring, 0f, 0f, 0f); return; }
            var local = Mathf.Clamp01((t - delay) / (1f - delay));
            var eased = 1f - (1f - local) * (1f - local);
            Set(ring, Mathf.Lerp(.3f, 1f, eased), _clock * 12f, .8f * (1f - local));
        }
        private void Set(SpriteRenderer renderer, float scale, float degrees, float alpha)
        {
            renderer.enabled = scale > 0f && alpha > 0f;
            renderer.transform.localScale = Vector3.one * scale;
            renderer.transform.localRotation = Quaternion.Euler(0f, 0f, degrees);
            // The approved images carry their own palette. Only the procedural backing/waves use the effect tint.
            renderer.color = renderer == _main && _shape != TravelerEffectShape.Ring
                ? new Color(1f, 1f, 1f, _color.a * alpha)
                : new Color(_color.r, _color.g, _color.b, _color.a * alpha);
        }
        private void EnsureRenderers()
        {
            if (_scaleRoot != null) return;
            _scaleRoot = new GameObject("Flattened").transform; _scaleRoot.SetParent(transform, false);
            _disc = Create("Disc", ProceduralShapeSprites.Disc, SortingOrder);
            _main = Create("Main", ProceduralShapeSprites.Ring, SortingOrder + 1);
            _second = Create("Second", ProceduralShapeSprites.Ring, SortingOrder + 1);
            _third = Create("Third", ProceduralShapeSprites.Ring, SortingOrder + 1);
        }
        private SpriteRenderer Create(string name, Sprite sprite, int order)
        {
            var child = new GameObject(name);
            child.transform.SetParent(_scaleRoot, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite; renderer.sortingOrder = order;
            return renderer;
        }
    }
}
