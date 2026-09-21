using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Game.ActiveSkill;
using Game.Character;
using Game.Enemy;
using Game.Progression;
using Game.Run;
using Game.Telemetry;
using UnityEngine;

namespace Game.Bootstrap
{
    /// <summary>Development-only composition/provenance. Release creates no recorder or listeners.</summary>
    public static class PlaytestComposition
    {
        public static IPlaytestSession Create(FixtureRuntimeContentCatalog catalog, RunModel run,
            PlayerCharacterRuntime player, PlayerExperienceRuntime xp, LevelUpDraftRuntime draft,
            ContinuousFixtureEnemySpawner spawner, PlayerActiveSkillSetRuntime skills)
        {
            if (!Application.isEditor && !UnityEngine.Debug.isDebugBuild) return new DisabledPlaytestSession();
            try
            {
                string commit = null; bool? dirty = null;
#if UNITY_EDITOR
                commit = Git("rev-parse HEAD");
                var status = Git("status --porcelain");
                if (status != null) dirty = status.Length != 0;
#endif
                var entries = new List<object>();
                foreach (var entry in draft.Build.Entries)
                    entries.Add(new { id = entry.Definition.Id.ToString(), level = entry.Level, kind = entry.Definition.Kind.ToString() });
                var provenance = TelemetryProvenance.Capture(catalog.SourceSnapshot, new
                {
                    character = draft.Character.Id.ToString(), field = "unsupported",
                    timeline = catalog.WaveTimeline.Id.ToString(), durationSeconds = run.Duration,
                    seeds = new { draft = catalog.RunSetup.Draft.Seed, wave = catalog.WaveTimeline.Seed },
                    initialStats = player.Stats, initialBuild = entries, runSetup = catalog.RunSetup,
                    overrides = new { durationSeconds = run.Duration, source = "RunController scene configuration" }
                }, commit, dirty, Application.platform.ToString(), Application.isEditor ? "Editor" : "Development");
                return new PlaytestSession(run, player, xp, draft, spawner, skills, provenance,
                    new LocalPlaytestExportSink(Path.Combine(Application.persistentDataPath, "Playtests")),
                    () => (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency, DateTime.UtcNow);
            }
            catch (Exception error)
            {
                UnityEngine.Debug.LogWarning("Playtest recorder unavailable: " + error.Message);
            }
            return new DisabledPlaytestSession();
        }
#if UNITY_EDITOR
        private static string Git(string arguments)
        {
            try
            {
                using var process = new Process { StartInfo = new ProcessStartInfo("git", arguments)
                {
                    WorkingDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..")),
                    UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
                } };
                process.Start();
                var output = process.StandardOutput.ReadToEndAsync();
                var errors = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(1500)) { process.Kill(); return null; }
                return process.ExitCode == 0 ? output.GetAwaiter().GetResult().Trim() : null;
            }
            catch { return null; }
        }
#endif
    }
}
