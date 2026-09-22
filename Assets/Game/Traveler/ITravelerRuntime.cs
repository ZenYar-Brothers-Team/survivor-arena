using System;
using System.Collections.Generic;
namespace Game.Traveler
{
    public interface ITravelerRuntime
    {
        IReadOnlyList<TravelerSnapshot> Snapshot { get; }
        IReadOnlyList<TravelerScheduleEntry> Schedule { get; }
        string DevelopmentObservation { get; }
        event Action<TravelerEvent> LifeEvent;
        event Action<Game.Combat.CombatResult> CombatResolved;
        void SpawnDevelopmentTraveler();
    }
}
