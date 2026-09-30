using System;
using UnityEngine;

namespace Game.UI
{
    /// <summary>A world actor that shows its own HP bar above its head (DECISION-0110: mid-bosses).</summary>
    public readonly struct OverheadHealthSource
    {
        public Guid LifeId { get; }
        /// <summary>World point just above the body's top.</summary>
        public Vector3 WorldTop { get; }
        public float Health01 { get; }
        public OverheadHealthSource(Guid lifeId, Vector3 worldTop, float health01)
        { LifeId = lifeId; WorldTop = worldTop; Health01 = Mathf.Clamp01(health01); }
    }
}
