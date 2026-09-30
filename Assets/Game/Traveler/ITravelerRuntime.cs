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
        /// <summary>Development only: every Traveler of the current field pool, empty before the encounter is initialized.</summary>
        IReadOnlyList<TravelerChoice> DevelopmentChoices { get; }
        void SpawnDevelopmentTraveler();
        /// <summary>Development only: launches the chosen Traveler now, scaled like a scheduled one; unknown ids do nothing.</summary>
        void SpawnDevelopmentTraveler(string id);
    }
}
