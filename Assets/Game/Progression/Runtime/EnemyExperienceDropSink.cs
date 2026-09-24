using System;
using System.Collections.Generic;
using Game.Enemy;
using Game.Run;

namespace Game.Progression
{
    /// <summary>Preserves existing XP-on-death behavior without making Enemy depend on Progression.</summary>
    public sealed class EnemyExperienceDropSink : IEnemyLifecycleSink
    {
        private readonly PlayerExperienceRuntime _owner;
        private readonly RunController _run;
        private readonly HashSet<Guid> _rewardedLives = new HashSet<Guid>();

        public EnemyExperienceDropSink(PlayerExperienceRuntime owner, RunController run)
        {
            _owner = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
            _run = run != null ? run : throw new ArgumentNullException(nameof(run));
        }

        public void OnEnemyLifeEvent(EnemyLifeEvent snapshot)
        {
            if (snapshot.Kind != EnemyLifeEventKind.Died || snapshot.ExperienceReward <= 0f) return;
            if (_run.Model == null || _run.Model.Outcome != null || snapshot.RunId != _run.Model.RunId ||
                _rewardedLives.Contains(snapshot.LifeId)) return;
            ExperienceDropFactory.Spawn(snapshot.ExperienceReward, _owner.ScatterDropPosition(snapshot.Position),
                _owner.DropLifetimeSeconds, _owner, _run, pool: _owner.DropPool,
                sourceLifeId: snapshot.LifeId, sourceContentId: snapshot.ContentId);
            _rewardedLives.Add(snapshot.LifeId);
        }
    }
}
