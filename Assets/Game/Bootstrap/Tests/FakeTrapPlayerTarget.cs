using System.Collections.Generic;
using Game.Content;
using Game.Traps;
using UnityEngine;

namespace Game.Bootstrap.Tests
{
    /// <summary>Records trap hits instead of touching a player runtime.</summary>
    internal sealed class FakeTrapPlayerTarget : ITrapPlayerTarget
    {
        public Vector2 Position { get; set; }
        public bool IsAlive { get; set; } = true;
        public readonly List<(float amount, ContentId source)> Hits = new List<(float amount, ContentId source)>();

        public void Hit(float amount, ContentId source) => Hits.Add((amount, source));
    }
}
