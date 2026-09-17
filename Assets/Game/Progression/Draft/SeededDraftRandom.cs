using System;

namespace Game.Progression
{
    public sealed class SeededDraftRandom : IDraftRandom
    {
        private readonly Random _random;

        public SeededDraftRandom(int seed)
        {
            _random = new Random(seed);
        }

        public int NextInt(int exclusiveMaximum)
        {
            if (exclusiveMaximum <= 0)
                throw new ArgumentOutOfRangeException(nameof(exclusiveMaximum));
            return _random.Next(exclusiveMaximum);
        }

        public float NextFloat01()
        {
            return (float)_random.NextDouble();
        }
    }
}
