using System;
using System.Collections.Generic;
using Game.Combat;
using Game.Content;
using Game.Run;
using Newtonsoft.Json.Linq;

namespace Game.Telemetry.Tests
{
    internal static class TelemetryTestData
    {
        public static JObject Provenance() => TelemetryProvenance.Capture(
            new Dictionary<string, string> { ["Content/Fixture"] = "{\"damage\":10}" },
            new { seed = 12, duration = 60 }, "fixture-commit", true, "test", "fixture");
        public static RunTelemetryRecorder Recorder(RunModel run, TelemetryLimits limits = null) =>
            new RunTelemetryRecorder(run, limits ?? new TelemetryLimits(), Provenance(), () => 0, new DateTime(2026, 9, 21, 0, 0, 0, DateTimeKind.Utc));
        public static CombatResult Damage(RunModel run, float requested = 100, float applied = 10, int level = 1) =>
            new CombatResult(new CombatSource(default, new ContentId("FIXTURE-SKILL"), CombatSourceOrigin.ActiveSkill, level),
                new CombatIdentity(Guid.NewGuid(), run.RunId, new ContentId("FIXTURE-ENEMY"), CombatEntityCategory.OrdinaryEnemy),
                new HealthChange(requested, requested, applied, false));
    }
}
