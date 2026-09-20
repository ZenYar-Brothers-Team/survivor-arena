namespace Game.Combat
{
    public readonly struct ControlMotion
    {
        public float KnockbackX { get; }
        public float KnockbackY { get; }
        public float MovementMultiplier { get; }
        public ControlMotion(float x, float y, float movementMultiplier)
        {
            KnockbackX = x;
            KnockbackY = y;
            MovementMultiplier = movementMultiplier;
        }
    }
}
