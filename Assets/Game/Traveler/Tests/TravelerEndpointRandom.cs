using System;
namespace Game.Traveler.Tests
{
    public sealed class TravelerEndpointRandom : Random
    {
        private readonly double _value;
        public TravelerEndpointRandom(double value) { _value=value; }
        public override double NextDouble() => _value;
        public override int Next(int maxValue) => 0;
    }
}
