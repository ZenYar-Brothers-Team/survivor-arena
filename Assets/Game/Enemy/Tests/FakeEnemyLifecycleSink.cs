using System;
using System.Collections.Generic;

namespace Game.Enemy.Tests
{
    internal sealed class FakeEnemyLifecycleSink : IEnemyLifecycleSink
    {
        public List<EnemyLifeEvent> Events { get; } = new List<EnemyLifeEvent>();
        public Action<EnemyLifeEvent> OnEvent { get; set; }

        public void OnEnemyLifeEvent(EnemyLifeEvent snapshot)
        {
            Events.Add(snapshot);
            OnEvent?.Invoke(snapshot);
        }
    }
}
