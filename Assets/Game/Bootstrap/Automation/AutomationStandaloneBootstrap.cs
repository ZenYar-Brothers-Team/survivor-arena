#if BALANCE_AUTOMATION || UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using Game.Automation;
using Game.Meta;
using Game.Settings;
using Game.Telemetry;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Game.Bootstrap.Automation
{
    /// <summary>Opt-in development player entry. Hooks the scene before its root Start opens any store.</summary>
    public static class AutomationStandaloneBootstrap
    {
        private static string _chainId;
        private static ExperimentConfig _config;
        private static MemoryProfileStore _store;
        private static MetaCatalog _catalog;
        private static string _failure;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            var args = Environment.GetCommandLineArgs();
            var configPath = Value(args, "--balance-experiment=");
            if (configPath == null) return;
            if (Array.IndexOf(args, "--balance-audio") < 0) AudioListener.volume = 0f;
            SceneManager.sceneLoaded += Configure;
            try
            {
                if (!Debug.isDebugBuild) throw new InvalidOperationException("Balance worker requires a development build.");
                var outputRoot = Value(args, "--balance-output-root=")
                    ?? throw new ArgumentException("Missing balance output root.");
                _chainId = Value(args, "--balance-chain=")
                    ?? throw new ArgumentException("Missing balance chain ID.");
                _catalog = MetaCatalog.Load();
                var fields = RuntimeContentCatalog.CreateProduction().Fields.Roster.AllFields.Select(item => item.Id.ToString());
                _config = new ExperimentConfigLoader(_catalog, fields, outputRoot, allowExistingOutput: true)
                    .LoadFile(configPath);
                var marker = JObject.Parse(File.ReadAllText(Path.Combine(_config.OutputDirectory, "experiment.json")));
                if ((string)marker["sourceConfigSha256"] != TelemetryProvenance.Hash(File.ReadAllText(configPath)))
                    throw new InvalidOperationException("Experiment configuration changed after runner validation.");
                var factory = new IsolatedProfileFactory(_catalog, _config);
                _store = factory.CreateChainStoreAsync().GetAwaiter().GetResult();
            }
            catch (Exception error) { _failure = error.ToString(); Debug.LogError("Balance bootstrap: " + _failure); }
        }

        private static void Configure(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= Configure;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            if (root == null) { Debug.LogError("Balance bootstrap: Gameplay root missing."); Application.Quit(3); return; }
            if (_failure != null)
            {
                root.enabled = false; // Fail closed: never enter production profile/settings fallback.
                Application.Quit(3);
                return;
            }
            try
            {
                var settings = SettingsConfig.Load();
                root.ConfigureSettings(new SettingsService(settings, new MemorySettingsStore(), new UnityVideoDevice(settings)));
                root.ConfigureProfile(new ProfileService(_catalog, _store));
                var campaign = root.gameObject.AddComponent<AutomationCampaignHost>();
                campaign.Initialize(root, _config, _store, _chainId);
                campaign.Finished += finished =>
                {
                    var folder = Path.Combine(_config.OutputDirectory, "chains", _chainId);
                    AutomationPlaytestSink.WriteAtomic(Path.Combine(folder, "chain-summary.json"),
                        new JObject { ["chainId"] = _chainId, ["startedRuns"] = finished.StartedRuns,
                            ["stopReason"] = finished.StopReason, ["endedUtc"] = DateTime.UtcNow.ToString("O") }.ToString());
                    var normal = finished.StopReason == "maxRunsPerChain" || finished.StopReason == "routeCleared" ||
                        finished.StopReason == "experimentWallBudget" ||
                        finished.StopReason.StartsWith("routeBlocked:", StringComparison.Ordinal) ||
                        finished.StopReason.StartsWith("runStopped:", StringComparison.Ordinal);
                    root.QuitAutomation(normal ? 0 : 3);
                };
            }
            catch (Exception error)
            {
                root.enabled = false;
                Debug.LogError("Balance bootstrap: " + error);
                Application.Quit(3);
            }
        }

        private static string Value(string[] args, string prefix)
        {
            foreach (var value in args)
                if (value.StartsWith(prefix, StringComparison.Ordinal)) return value.Substring(prefix.Length);
            return null;
        }
    }
}
#endif
