using System;
using UnityEngine;

namespace Game.UI
{
    public readonly struct OverheadHealthBarItem
    {
        public Guid LifeId { get; }
        /// <summary>Normalized screen position, x right and y down (0..1).</summary>
        public Vector2 Position { get; }
        public float Health01 { get; }
        public OverheadHealthBarItem(Guid lifeId, Vector2 position, float health01)
        { LifeId = lifeId; Position = position; Health01 = health01; }
    }
}
