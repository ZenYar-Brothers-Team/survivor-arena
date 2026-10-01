using UnityEngine;

namespace Game.Zones
{
    /// <summary>One warning circle of a strike altar that is currently drawn: <see cref="Telegraph"/> fills 0..1, then <see cref="Flash"/> runs 0..1.</summary>
    public readonly struct StrikeCircle
    {
        public readonly ZonePlacement Zone;
        public readonly Vector2 Center;
        public readonly float Radius;
        /// <summary>0..1 while the warning fills; 1 afterwards.</summary>
        public readonly float Telegraph;
        /// <summary>0 before the strike, then 0..1 over the flash.</summary>
        public readonly float Flash;

        public StrikeCircle(ZonePlacement zone, Vector2 center, float radius, float telegraph, float flash)
        {
            Zone = zone; Center = center; Radius = radius; Telegraph = telegraph; Flash = flash;
        }
    }
}
