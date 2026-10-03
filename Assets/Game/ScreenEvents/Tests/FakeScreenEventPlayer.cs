using System.Collections.Generic;
using Game.Content;
using UnityEngine;

namespace Game.ScreenEvents.Tests
{
    internal sealed class FakeScreenEventPlayer : IScreenEventPlayerTarget
    {
        public Vector2 Position { get; set; }
        public bool IsAlive { get; set; } = true;
        public readonly List<(float fraction, string source)> Hits = new List<(float, string)>();

        public void HitFraction(float fractionOfMaxHealth, ContentId source) => Hits.Add((fractionOfMaxHealth, source.ToString()));
    }
}
