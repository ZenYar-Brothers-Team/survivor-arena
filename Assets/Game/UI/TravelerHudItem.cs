using System;
using UnityEngine;
namespace Game.UI
{
    public readonly struct TravelerHudItem
    {
        public Guid LifeId { get; }
        public string Name { get; }
        public float HealthFraction { get; }
        public Vector2 Position { get; }
        public bool Offscreen { get; }
        public string Arrow { get; }
        public TravelerHudItem(Guid lifeId, string name, float healthFraction, Vector2 position, bool offscreen, string arrow)
        { LifeId = lifeId; Name = name; HealthFraction = healthFraction; Position = position; Offscreen = offscreen; Arrow = arrow; }
    }
}
