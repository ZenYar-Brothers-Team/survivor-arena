using System;
using UnityEngine;
namespace Game.UI
{
    /// <summary>
    /// One Traveler on the HUD (UI/UX §11). Visible: only an HP bar above the body. Off-screen: a pointer on the
    /// screen frame rotated toward the Traveler, without health (DECISION-0109).
    /// </summary>
    public readonly struct TravelerHudItem
    {
        public Guid LifeId { get; }
        public float HealthFraction { get; }
        /// <summary>Normalized screen position, x right and y down (0..1).</summary>
        public Vector2 Position { get; }
        public bool Offscreen { get; }
        /// <summary>Screen-space direction to the Traveler, degrees clockwise from «right» (y down).</summary>
        public float AngleDegrees { get; }
        public TravelerHudItem(Guid lifeId, float healthFraction, Vector2 position, bool offscreen, float angleDegrees)
        { LifeId = lifeId; HealthFraction = healthFraction; Position = position; Offscreen = offscreen; AngleDegrees = angleDegrees; }
    }
}
