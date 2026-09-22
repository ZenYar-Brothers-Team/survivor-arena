using System;
using System.Collections.Generic;
using Game.Traveler;
using Game.Combat;
namespace Game.Telemetry.Tests
{
    internal sealed class TravelerTelemetrySource : ITravelerRuntime
    {
        public IReadOnlyList<TravelerSnapshot> Snapshot { get; set; }
        public IReadOnlyList<TravelerScheduleEntry> Schedule { get; set; }
        public string DevelopmentObservation => "fixture";
        public event Action<TravelerEvent> LifeEvent;
        public event Action<CombatResult> CombatResolved;
        public void SpawnDevelopmentTraveler() { }
        public void Publish(TravelerEvent item) => LifeEvent?.Invoke(item);
        public void Hit(CombatResult result) => CombatResolved?.Invoke(result);
    }
}
