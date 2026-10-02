using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>One immutable platform network: connected round platforms and the straight bridges between them.</summary>
    public sealed class FieldPlatformLayout
    {
        public FieldPlatformLayoutDefinition Profile { get; }
        public IReadOnlyList<FieldPlatformDisc> Platforms { get; }
        public IReadOnlyList<FieldPlatformBridge> Bridges { get; }
        public int StartIndex { get; }
        public int Seed { get; }

        public Vector2 SpawnPosition => Platforms[StartIndex].Center;

        public FieldPlatformLayout(FieldPlatformLayoutDefinition profile, IEnumerable<FieldPlatformDisc> platforms,
            IEnumerable<FieldPlatformBridge> bridges, int startIndex, int seed)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            Platforms = Array.AsReadOnly(platforms.ToArray());
            Bridges = Array.AsReadOnly(bridges.ToArray());
            if (Platforms.Count < 2 || startIndex < 0 || startIndex >= Platforms.Count)
                throw new ArgumentException("Incomplete platform network.");
            foreach (var bridge in Bridges)
                if (bridge.From == bridge.To || bridge.From < 0 || bridge.To < 0 || bridge.From >= Platforms.Count ||
                    bridge.To >= Platforms.Count)
                    throw new ArgumentException("Bridge references a missing platform.");
            StartIndex = startIndex;
            Seed = seed;
        }

        /// <summary>True when the point stands on a platform or on a bridge; everything else is the damaging void.</summary>
        public bool IsWalkable(Vector2 point)
        {
            for (var i = 0; i < Platforms.Count; i++)
                if ((point - Platforms[i].Center).sqrMagnitude <= Platforms[i].Radius * Platforms[i].Radius) return true;
            var half = Profile.BridgeWidth * .5f;
            for (var i = 0; i < Bridges.Count; i++)
            {
                var a = Platforms[Bridges[i].From].Center;
                var b = Platforms[Bridges[i].To].Center;
                var edge = b - a;
                var t = Mathf.Clamp01(Vector2.Dot(point - a, edge) / edge.sqrMagnitude);
                if ((point - (a + edge * t)).sqrMagnitude <= half * half) return true;
            }
            return false;
        }

        /// <summary>Number of bridges touching each platform.</summary>
        public int[] Degrees()
        {
            var result = new int[Platforms.Count];
            foreach (var bridge in Bridges) { result[bridge.From]++; result[bridge.To]++; }
            return result;
        }
    }
}
