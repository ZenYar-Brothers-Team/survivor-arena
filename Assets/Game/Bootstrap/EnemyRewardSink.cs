using Game.Enemy;
namespace Game.Bootstrap
{
    /// <summary>Composition fan-out keeps Enemy independent of XP and pickup ownership.</summary>
    public sealed class EnemyRewardSink : IEnemyLifecycleSink
    {
        private readonly IEnemyLifecycleSink _experience, _pickups;
        public EnemyRewardSink(IEnemyLifecycleSink experience, IEnemyLifecycleSink pickups) { _experience = experience; _pickups = pickups; }
        public void OnEnemyLifeEvent(EnemyLifeEvent snapshot)
        { _experience.OnEnemyLifeEvent(snapshot); _pickups.OnEnemyLifeEvent(snapshot); }
    }
}
