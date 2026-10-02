using System;
using Game.Presentation;
using UnityEngine;

namespace Game.Bootstrap
{
    /// <summary>Stationary foundation contact owned by the scene adapter, separate from the sprite and effect radius.</summary>
    public static class AltarObstacleFactory
    {
        public static CircleCollider2D Attach(GameObject root, AltarPresentationProfile profile, bool shrine)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            var playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer < 0) throw new InvalidOperationException("Altar contact requires the Player layer.");
            var collider = root.AddComponent<CircleCollider2D>();
            collider.radius = shrine ? profile.ShrineContactRadius : profile.AltarContactRadius;
            collider.offset = new Vector2(0f, shrine ? profile.ShrineContactOffsetY : profile.AltarContactOffsetY);
            collider.isTrigger = false;
            collider.excludeLayers = ~(1 << playerLayer);
            return collider;
        }
    }
}
