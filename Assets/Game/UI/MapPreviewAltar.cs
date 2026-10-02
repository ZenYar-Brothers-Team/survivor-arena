using UnityEngine;

namespace Game.UI
{
    /// <summary>Read-only altar projection: actual run radius, polarity and cycle state, including off-screen altars.</summary>
    public readonly struct MapPreviewAltar
    {
        public Vector2 Center { get; }
        public float Radius { get; }
        public Color Color { get; }
        public bool Negative { get; }
        public bool Active { get; }

        public MapPreviewAltar(Vector2 center, float radius, Color color, bool negative, bool active)
        {
            Center = center; Radius = radius; Color = color; Negative = negative; Active = active;
        }
    }
}
