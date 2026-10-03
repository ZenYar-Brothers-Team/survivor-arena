using UnityEngine;

namespace Game.UI
{
    /// <summary>Read-only trap projection for the development map: a projectile turret or a barrel, with the turret's body radius.</summary>
    public readonly struct MapPreviewTrap
    {
        public Vector2 Center { get; }
        public float Radius { get; }
        public bool Barrel { get; }
        public bool Explosive { get; }
        /// <summary>False for a barrel that already blew up.</summary>
        public bool Intact { get; }

        public MapPreviewTrap(Vector2 center, float radius, bool barrel, bool explosive, bool intact)
        {
            Center = center; Radius = radius; Barrel = barrel; Explosive = explosive; Intact = intact;
        }
    }
}
