using System;
using System.Collections.Generic;
using Game.Traveler;
namespace Game.UI.Tests
{
    public sealed class TravelerUiHarness : ITravelerRuntime, ITravelerView
    {
        public IReadOnlyList<TravelerSnapshot> Snapshot { get; set; } = Array.Empty<TravelerSnapshot>();
        public IReadOnlyList<TravelerScheduleEntry> Schedule => Array.Empty<TravelerScheduleEntry>();
        public string DevelopmentObservation => "fixture observation";
        public event Action<TravelerEvent> LifeEvent { add { } remove { } }
        public event Action<Game.Combat.CombatResult> CombatResolved { add { } remove { } }
        public event Action SpawnRequested;
        public IReadOnlyList<TravelerHudItem> Rendered { get; private set; }
        public bool Development { get; private set; }
        public int SpawnCount { get; private set; }
        public void SpawnDevelopmentTraveler() => SpawnCount++;
        public void RequestSpawn() => SpawnRequested?.Invoke();
        public void Render(IReadOnlyList<TravelerHudItem> travelers, string observation, bool development)
        { Rendered = travelers; Development = development; }
    }
}
