using System;
using Game.Diagnostics;
using Game.Pickup;
using UnityEngine;
namespace Game.Traveler
{
    public sealed class TravelerPlacement
    {
        private readonly BoxPickupPlacement _placement;
        public TravelerPlacement(BoxPickupPlacement placement) { _placement = placement ?? throw new ArgumentNullException(nameof(placement)); }
        public Vector2 ClampToBounds(Vector2 point) => _placement.ClampToBounds(point);
        public bool Contains(Vector2 point) => _placement.Contains(point);
        public bool TrySpawn(Vector2 player, float distance, int attempts, System.Random random, out Vector2 point)
        {
            using var guard = PerfGuard.Measure("Traveler.Placement", 5f);
            // Uniform independent directions; never reduce the configured distance by clamping.
            for (var i = 0; i < attempts; i++)
            {
                var angle = (float)random.NextDouble() * Mathf.PI * 2;
                point = player + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                if (Contains(point)) return true;
            }
            point = default; return false;
        }
    }
}
