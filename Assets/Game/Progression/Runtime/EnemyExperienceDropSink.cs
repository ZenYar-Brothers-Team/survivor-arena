using System;
using Game.Enemy;
using Game.Run;

namespace Game.Progression
{
    /// <summary>Preserves existing XP-on-death behavior without making Enemy depend on Progression.</summary>
    public sealed class EnemyExperienceDropSink : IEnemyLifecycleSink
    {
        private readonly PlayerExperienceRuntime _owner;
        private readonly RunController _run;

        public EnemyExperienceDropSink(PlayerExperienceRuntime owner, RunController run)
        {
            _owner = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
            _run = run != null ? run : throw new ArgumentNullException(nameof(run));
        }

        public void OnEnemyLifeEvent(EnemyLifeEvent snapshot)
        {
            if (snapshot.Kind != EnemyLifeEventKind.Died || snapshot.ExperienceReward <= 0f) return;
            ExperienceDropFactory.Spawn(snapshot.ExperienceReward, snapshot.Position,
                _owner.DropLifetimeSeconds, _owner, _run, pool: _owner.DropPool);
        }
    }
}
