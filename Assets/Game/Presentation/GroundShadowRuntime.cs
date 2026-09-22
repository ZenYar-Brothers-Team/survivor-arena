using System;
using UnityEngine;

namespace Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class GroundShadowRuntime : MonoBehaviour
    {
        private SpriteRenderer _renderer;

        public SpriteRenderer Renderer => _renderer;

        /// <summary>Creates or reuses one renderer and aligns it with the body's authored ground point.</summary>
        public void Initialize(GroundShadowPresentationProfile profile, SpriteContactProfile contact,
            float rootScale, SpriteRenderer bodyRenderer, SpriteRenderer existingRenderer = null)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (bodyRenderer == null) throw new ArgumentNullException(nameof(bodyRenderer));
            if (!float.IsFinite(rootScale) || rootScale <= 0f) throw new ArgumentOutOfRangeException(nameof(rootScale));
            if (_renderer == null) _renderer = existingRenderer;
            if (_renderer == null)
            {
                var shadow = new GameObject("GroundShadow");
                shadow.transform.SetParent(transform, false);
                _renderer = shadow.AddComponent<SpriteRenderer>();
            }

            var centerY = contact?.CenterY ?? profile.FallbackCenterY;
            var width = contact == null
                ? profile.FallbackWidth
                : contact.Radius * 2f * profile.ContactWidthScale;
            _renderer.transform.localPosition = new Vector3(0f, (-centerY + profile.OffsetY) / rootScale, 0f);
            _renderer.transform.localRotation = Quaternion.identity;
            _renderer.transform.localScale = new Vector3(width / rootScale, profile.Height / rootScale, 1f);
            _renderer.sharedMaterial = bodyRenderer.sharedMaterial;
            _renderer.sortingLayerID = bodyRenderer.sortingLayerID;
            _renderer.sortingOrder = bodyRenderer.sortingOrder - 1;
            _renderer.sprite = GroundShadowSprite.Shared;
            _renderer.color = profile.Color;
            _renderer.enabled = true;
        }

        public void Shutdown()
        {
            if (_renderer != null) _renderer.enabled = false;
        }

        private void OnDisable() => Shutdown();
    }
}
