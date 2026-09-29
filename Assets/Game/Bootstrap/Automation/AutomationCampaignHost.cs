using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Game.Automation;
using Game.Meta;
using Game.Run;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Game.Bootstrap.Automation
{
    /// <summary>One independent profile chain. The external worker starts a new process/store for each chain.</summary>
    public sealed class AutomationCampaignHost : MonoBehaviour
    {
        private GameplayCompositionRoot _root;
        private ExperimentConfig _config;
        private ExperimentConfigData _settings;
        private MemoryProfileStore _store;
        private AutomationRunHost _run;
        private Task<PurchasePolicyResult> _purchases;
        private string _chainId;
        private string _terminalRunFolder;
        private int _fieldIndex;
        private int _runIndex;
        private bool _pendingIntermission;
        private double _startedAt;
        private double _lastProgressAt;
        private Guid? _lastProgressRunId;
        public bool IsFinished { get; private set; }
        public string StopReason { get; private set; }
        public int StartedRuns => _runIndex;
        public event Action<AutomationCampaignHost> Finished;

        public void Initialize(GameplayCompositionRoot root, ExperimentConfig config, MemoryProfileStore store, string chainId)
        {
            if (_root != null) throw new InvalidOperationException("Campaign already initialized.");
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _settings = config.Data;
            if (string.IsNullOrWhiteSpace(chainId) || chainId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("Invalid chain ID.", nameof(chainId));
            _chainId = chainId;
            _startedAt = Time.realtimeSinceStartupAsDouble;
            var folder = Path.Combine(_config.OutputDirectory, "chains", _chainId);
            if (Directory.Exists(folder)) throw new IOException("Chain output already exists: " + folder);
            Directory.CreateDirectory(folder);
            AutomationPlaytestSink.WriteAtomic(Path.Combine(folder, "initial-profile.json"),
                _store.Main ?? throw new InvalidOperationException("Chain profile not loaded."));
        }

        private void Update()
        {
            if (_root == null || IsFinished) return;
            try
            {
                if (Time.realtimeSinceStartupAsDouble - _lastProgressAt >= 2 || _run?.CurrentRunId != _lastProgressRunId)
                    WriteProgress();
                if (Time.realtimeSinceStartupAsDouble - _startedAt >= _settings.MaxExperimentWallSeconds.Value)
                {
                    if (_run != null && !_run.IsFinished) _run.RequestStop("experimentWallBudget");
                    else if (!_pendingIntermission) Finish("experimentWallBudget");
                }
                if (_pendingIntermission) { ProcessIntermission(); return; }
                if (_run != null || !_root.AtMainMenu || !_root.Profile.CanStart) return;
                if (_runIndex >= _settings.MaxRunsPerChain.Value) { Finish("maxRunsPerChain"); return; }
                _runIndex++;
                _run = gameObject.AddComponent<AutomationRunHost>();
                _run.Finished += OnRunFinished;
                _run.Initialize(_root, _config, _store, _chainId, _runIndex, _settings.FieldRoute[_fieldIndex]);
            }
            catch (Exception error) { Finish("campaignException: " + error.Message); }
        }

        private void OnRunFinished(AutomationRunHost run)
        {
            // No scene teardown or purchase inside an outcome/gameplay callback.
            _pendingIntermission = true;
        }

        private void WriteProgress()
        {
            var folder = Path.Combine(_config.OutputDirectory, "chains", _chainId);
            AutomationPlaytestSink.WriteAtomic(Path.Combine(folder, "chain-progress.json"),
                new JObject { ["chainId"] = _chainId, ["startedRuns"] = _runIndex,
                    ["currentRunId"] = _run?.CurrentRunId?.ToString("N"),
                    ["currentRunState"] = _run?.State.ToString(),
                    ["fieldId"] = _settings.FieldRoute[_fieldIndex],
                    ["updatedUtc"] = DateTime.UtcNow.ToString("O") }.ToString());
            _lastProgressAt = Time.realtimeSinceStartupAsDouble;
            _lastProgressRunId = _run?.CurrentRunId;
        }

        private void ProcessIntermission()
        {
            if (_run == null || !_run.IsFinished) return;
            if (_terminalRunFolder == null && _run.Outcome != null)
                _terminalRunFolder = Path.Combine(_config.OutputDirectory, "chains", _chainId, "runs", _run.Outcome.RunId.ToString("N"));
            if (_run.State != AutomationRunState.Completed)
            {
                FinalizePurchases(new PurchasePolicyResult());
                Finish("run" + _run.State + ":" + _run.TerminalReason);
                return;
            }
            if (_purchases == null)
            {
                _purchases = new AutomationPurchasePolicy(_settings.PurchasePolicy)
                    .ExecuteAsync(_root.Profile, _settings.CharacterId);
                return;
            }
            if (!_purchases.IsCompleted) return;
            if (_purchases.IsFaulted) { Finish("purchaseException: " + _purchases.Exception.GetBaseException().Message); return; }
            var result = _purchases.Result;
            FinalizePurchases(result);
            if (result.Error != null) { Finish(result.Error); return; }
            if (Time.realtimeSinceStartupAsDouble - _startedAt >= _settings.MaxExperimentWallSeconds.Value)
            { Finish("experimentWallBudget"); return; }
            var playable = _root.Catalog.Fields.Roster.AllFields.Select(item => item.Id.ToString()).ToArray();
            var next = AutomationRoutePolicy.NextIndex(_fieldIndex, _run.Outcome.Reason, _settings,
                _root.Profile, playable, out var stop);
            if (stop != null) { Finish(stop); return; }
            _fieldIndex = next;
            _root.MainMenu();
            _run.Finished -= OnRunFinished;
            Destroy(_run);
            _run = null;
            _purchases = null;
            _terminalRunFolder = null;
            _pendingIntermission = false;
        }

        private void FinalizePurchases(PurchasePolicyResult result)
        {
            if (_terminalRunFolder == null) return;
            AutomationPlaytestSink.WriteAtomic(Path.Combine(_terminalRunFolder, "profile-after-purchases.json"),
                _store.Main ?? throw new InvalidOperationException("Profile disappeared after run."));
            var sidecarPath = Path.Combine(_terminalRunFolder, "automation.json");
            if (!File.Exists(sidecarPath)) return;
            var sidecar = JObject.Parse(File.ReadAllText(sidecarPath));
            sidecar["purchases"] = JArray.FromObject(result.Purchases);
            sidecar["purchaseRefusals"] = JObject.FromObject(result.Refusals);
            sidecar["currencyAfterPurchases"] = _root.Profile.Currency;
            sidecar["purchaseError"] = result.Error;
            AutomationPlaytestSink.WriteAtomic(sidecarPath, sidecar.ToString());
        }

        private void Finish(string reason)
        {
            if (IsFinished) return;
            StopReason = reason;
            IsFinished = true;
            Finished?.Invoke(this);
        }

        private void OnDestroy()
        {
            if (_run != null) _run.Finished -= OnRunFinished;
        }
    }
}
