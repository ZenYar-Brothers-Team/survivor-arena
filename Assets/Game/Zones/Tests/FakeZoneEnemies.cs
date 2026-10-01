using System.Collections.Generic;
using Game.Content;
using UnityEngine;

namespace Game.Zones.Tests
{
    internal sealed class FakeZoneEnemies : IZoneEnemySource
    {
        public readonly List<Vector2> Positions = new List<Vector2>();
        public readonly List<float> SlowFractions = new List<float>();
        public readonly List<float> DamageTaken = new List<float>();
        public readonly List<float> StrikeTaken = new List<float>();
        public int Refreshes;

        public FakeZoneEnemies Add(Vector2 position)
        {
            Positions.Add(position); SlowFractions.Add(0f); DamageTaken.Add(0f); StrikeTaken.Add(0f);
            return this;
        }

        public int Refresh() { Refreshes++; return Positions.Count; }
        public Vector2 Position(int index) => Positions[index];
        public void Slow(int index, float fraction, float seconds, ContentId source) => SlowFractions[index] = fraction;
        public void Damage(int index, float amount, ContentId source) => DamageTaken[index] += amount;
        public void Strike(int index, float amount, ContentId source) => StrikeTaken[index] += amount;
    }
}
