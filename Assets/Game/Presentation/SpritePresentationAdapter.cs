using System;
using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    // One writer per renderer/visual child. Gameplay owns lifetime and signal timing.
    public sealed class SpritePresentationAdapter : IDisposable
    {
        private readonly Transform _visual;
        private readonly SpriteRenderer _renderer;
        private readonly Vector3 _position, _scale;
        private readonly Quaternion _rotation;
        private readonly Color _color;
        private readonly Sprite _sprite;
        private readonly bool _flip, _enabled;
        private IPresentationSource _source;
        private ProceduralSpriteAnimator _animator;
        private SpriteMotionProfile _motion;
        private PresentationFeedbackProfile _feedback;
        private float _fade, _proc;
        private bool _fading;

        public bool IsInitialized => _source != null;
        public bool IsComplete => _fading && _fade <= 0;

        public SpritePresentationAdapter(Transform gameplayRoot, SpriteRenderer renderer)
        {
            if (gameplayRoot == null || renderer == null) throw new ArgumentNullException();
            if (renderer.transform == gameplayRoot || !renderer.transform.IsChildOf(gameplayRoot))
                throw new InvalidOperationException("Presentation renderer must be a child of gameplay root.");
            if (renderer.GetComponentInChildren<Collider2D>() != null || renderer.GetComponentInChildren<Rigidbody2D>() != null)
                throw new InvalidOperationException("Animated visual subtree must not own physics.");
            _visual = renderer.transform;
            _renderer = renderer;
            _position = _visual.localPosition; _rotation = _visual.localRotation; _scale = _visual.localScale;
            _color = renderer.color; _sprite = renderer.sprite; _flip = renderer.flipX; _enabled = renderer.enabled;
        }

        public void Initialize(SpriteDefinition definition, SpriteRole role, IPresentationSource source,
            PresentationFeedbackProfile feedback, SpriteMotionProfile motion = null)
        {
            Shutdown();
            if (definition == null || source == null || feedback == null) throw new ArgumentNullException();
            definition.RequireRole(role);
            _feedback = feedback;
            _motion = motion;
            _animator = motion == null ? null : new ProceduralSpriteAnimator(motion);
            _renderer.sprite = definition.Sprite;
            _source = source;
            _source.Signaled += HandleSignal;
        }

        public void Tick(float deltaTime)
        {
            NumericValidation.ValidateNonNegativeFinite(deltaTime, nameof(deltaTime));
            if (_source == null) throw new InvalidOperationException("Presentation is not initialized.");
            if (!_source.IsRunning) return;
            _proc = Mathf.Max(0, _proc - deltaTime);
            if (_fading) _fade = Mathf.Max(0, _fade - deltaTime);
            var pose = _animator?.Tick(deltaTime, true, _source.Velocity);
            _visual.localPosition = _position + (pose?.PositionOffset ?? Vector3.zero);
            _visual.localRotation = _rotation * Quaternion.Euler(0, 0, pose?.RotationDegrees ?? 0);
            var size = pose?.ScaleMultiplier ?? Vector2.one;
            var accent = 1 + _feedback.ProcScale * (_proc / _feedback.ProcSeconds);
            _visual.localScale = Vector3.Scale(_scale, new Vector3(
                Mathf.Clamp(size.x * accent, .88f, 1.12f), Mathf.Clamp(size.y * accent, .9f, 1.1f), 1));
            _renderer.flipX = pose?.FlipX ?? _flip;
            var color = Color.Lerp(_color, _motion?.HitFlashColor ?? _color, pose?.FlashAmount ?? 0);
            color.a *= _fading ? _fade / _feedback.FadeSeconds : 1;
            _renderer.color = color;
        }

        private void HandleSignal(PresentationSignal signal)
        {
            if (_source == null || !_source.IsRunning || _fading) return;
            switch (signal)
            {
                case PresentationSignal.Hit: _animator?.PlayHit(1); break;
                case PresentationSignal.Proc: _proc = _feedback.ProcSeconds; break;
                case PresentationSignal.Death:
                case PresentationSignal.Collect: _fading = true; _fade = _feedback.FadeSeconds; break;
                default: throw new ArgumentOutOfRangeException(nameof(signal));
            }
        }

        public void Shutdown()
        {
            if (_source != null) _source.Signaled -= HandleSignal;
            _source = null; _animator = null; _motion = null; _feedback = null;
            _fade = 0; _proc = 0; _fading = false;
            if (_renderer == null) return;
            _visual.localPosition = _position; _visual.localRotation = _rotation; _visual.localScale = _scale;
            _renderer.color = _color; _renderer.sprite = _sprite; _renderer.flipX = _flip; _renderer.enabled = _enabled;
        }

        public void Dispose() => Shutdown();
    }
}
