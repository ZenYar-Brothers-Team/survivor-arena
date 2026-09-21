using System;
using Game.Diagnostics;
using Game.Pickup;
using UnityEngine;
namespace Game.Traveler
{
    public sealed class TravelerPlacement
    {
        private readonly IPickupPlacement _reachable;
        public TravelerPlacement(IPickupPlacement reachable) { _reachable = reachable ?? throw new ArgumentNullException(nameof(reachable)); }
        public Vector2 Project(Vector2 point) => _reachable.TryPlace(point, out var result) ? result : throw new InvalidOperationException("No reachable Traveler point.");
        public bool Contains(Vector2 point) => (Project(point) - point).sqrMagnitude < .000001f;
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
