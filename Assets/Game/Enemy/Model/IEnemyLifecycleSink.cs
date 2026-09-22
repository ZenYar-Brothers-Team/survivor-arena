namespace Game.Enemy
{
    /// <summary>Optional lifecycle consumer, supplied by composition before Spawn is published.</summary>
    public interface IEnemyLifecycleSink
    {
        void OnEnemyLifeEvent(EnemyLifeEvent snapshot);
    }
}
