using System;

namespace Game.Progression.Tests
{
    public sealed class SequenceDraftRandom : IDraftRandom
    {
        private readonly float[] _values;
        private int _index;

        public SequenceDraftRandom(params float[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("At least one random value is required.", nameof(values));
            _values = (float[])values.Clone();
        }

        public int NextInt(int exclusiveMaximum) => 0;

        public float NextFloat01()
        {
            var value = _values[Math.Min(_index, _values.Length - 1)];
            _index++;
            return value;
        }
    }
}
