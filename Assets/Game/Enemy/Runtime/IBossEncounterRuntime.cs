using System;

namespace Game.Enemy
{
    public interface IBossEncounterRuntime
    {
        EnemyRuntime FinalBoss { get; }
        BossEncounterDefinition FinalDefinition { get; }
        string DevelopmentObservation { get; }
        event Action<EnemyLifeEvent> LifeEvent;
        event Action<BossPhaseEvent> PhaseChanged;
        event Action Changed;
        void Shutdown();
    }
}
