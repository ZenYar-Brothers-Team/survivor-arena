namespace Game.Enemy
{
    /// <summary>Owned temporary actor state must unwind before life replacement or pooling.</summary>
    public interface IEnemyLifeTransient { void Shutdown(); }
}
