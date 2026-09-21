using System;
using Game.Bootstrap;
using Game.Character;
using Game.Progression;
using Game.Run;
using UnityEngine;

namespace Game.Telemetry.Tests
{
    internal sealed class TelemetryRuntimeFixture : IDisposable
    {
        public GameObject Root { get; } = new GameObject("Telemetry test");
        public RunController Run { get; }
        public PlayerCharacterRuntime Player { get; }
        public PlayerExperienceRuntime Xp { get; }
        public LevelUpDraftRuntime Draft { get; }
        public FakePlaytestExportSink Sink { get; } = new FakePlaytestExportSink();
        public PlaytestSession Session { get; }
        public TelemetryRuntimeFixture(bool recording = true)
        {
            Run = Root.AddComponent<RunController>(); if (!Run.IsInitialized) Run.Initialize();
            Player = Root.AddComponent<PlayerCharacterRuntime>();
            Player.Initialize(new CharacterBaseStats(10, 1, pickedUpXpMultiplier: 2, disappearingXpRecovery: .25f), Run);
            Xp = Root.AddComponent<PlayerExperienceRuntime>();
            Xp.Initialize(Player, Run, new ExperienceSettings(5, 1000f));
            Draft = Root.AddComponent<LevelUpDraftRuntime>();
            var catalog = FixtureRuntimeContentCatalog.Create();
            Draft.Initialize(Xp, Run, catalog.BuildEntries, catalog.BuildEntries[0], 3, new SeededDraftRandom(12), 1, 1, catalog.Sets, new FixtureSetExtraAbilityFactory(), 1);
            if (recording) Session = new PlaytestSession(Run.Model, Player, Xp, Draft, null, null,
                TelemetryTestData.Provenance(), Sink, () => 0, DateTime.UtcNow);
            Run.Model.Start();
        }
        public void Dispose()
        {
            Run.Shutdown(); Session?.Dispose();
            Session?.PendingExport.GetAwaiter().GetResult();
            Draft.Shutdown(); Xp.Shutdown(); Player.Shutdown();
            UnityEngine.Object.DestroyImmediate(Root);
        }
    }
}
