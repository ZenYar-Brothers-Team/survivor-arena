using System;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Visual-only bob and pulse for pooled world pickups.</summary>
    public sealed class PickupSpritePresentation
    {
        private readonly Transform _root;
        private readonly SpriteRenderer _renderer;
        private float _scale;
        private float _phase;

        public PickupSpritePresentation(Transform owner, int sortingOrder)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            var child = new GameObject("VisualRoot");
            child.transform.SetParent(owner, false);
            _root = child.transform;
            _renderer = child.AddComponent<SpriteRenderer>();
            _renderer.sortingOrder = sortingOrder;
        }

        public SpriteRenderer Renderer => _renderer;

        public void Initialize(Sprite sprite, float scale, Color color, float phase)
        {
            if (sprite == null) throw new ArgumentNullException(nameof(sprite));
            if (!float.IsFinite(scale) || scale <= 0f) throw new ArgumentOutOfRangeException(nameof(scale));
            _scale = scale;
            _phase = phase;
            _renderer.sprite = sprite;
            _renderer.color = color;
            _renderer.enabled = true;
            ApplyPose();
        }

        public void Tick(float deltaTime)
        {
            if (!_renderer.enabled) return;
            _phase = Mathf.Repeat(_phase + deltaTime * 3.1f, Mathf.PI * 2f);
            ApplyPose();
        }

        public void Shutdown()
        {
            _renderer.enabled = false;
            _renderer.sprite = null;
            _renderer.color = Color.white;
            _root.localPosition = Vector3.zero;
            _root.localRotation = Quaternion.identity;
            _root.localScale = Vector3.one;
            _scale = 1f;
            _phase = 0f;
        }

        private void ApplyPose()
        {
            _root.localPosition = Vector3.up * (Mathf.Sin(_phase) * 0.025f);
            _root.localRotation = Quaternion.identity;
            var pulse = 1f + Mathf.Sin(_phase * 1.7f) * 0.045f;
            _root.localScale = Vector3.one * (_scale * pulse);
        }
    }
}
