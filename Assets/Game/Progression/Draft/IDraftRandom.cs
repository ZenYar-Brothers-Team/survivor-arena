namespace Game.Progression
{
    public interface IDraftRandom
    {
        int NextInt(int exclusiveMaximum);
        float NextFloat01();
    }
}
