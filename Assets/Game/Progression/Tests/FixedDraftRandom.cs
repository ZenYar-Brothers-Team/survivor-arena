namespace Game.Progression.Tests
{
    public sealed class FixedDraftRandom : IDraftRandom
    {
        private readonly float _value;

        public FixedDraftRandom(float value)
        {
            _value = value;
        }

        public int NextInt(int exclusiveMaximum)
        {
            return 0;
        }

        public float NextFloat01()
        {
            return _value;
        }
    }
}
