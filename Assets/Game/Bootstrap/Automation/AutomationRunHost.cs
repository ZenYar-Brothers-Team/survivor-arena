using System;
using System.Collections.Generic;
using System.IO;
using Game.Automation;
using Game.Content;
using Game.Meta;
using Game.Run;
using Game.Telemetry;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Game.Bootstrap.Automation
{
    /// <summary>Explicitly attached development host for one ordinary production run (IP-34 AB-03).</summary>
    public sealed class AutomationRunHost : MonoBehaviour
    {
        private GameplayCompositionRoot _root;
        private ExperimentConfigData _settings;
        private AutomationRuntimeBindings _bindings;
        private BotDirectionSource _direction;
        private BotMovementPolicy _movement;
        private RandomLegalDraftPolicy _draftChoice;
        private RunModel _run;
        private MemoryProfileStore _profileStore;
        private AutomationRunRecorder _recorder;
        private ExperimentConfig _config;
        private string _runFolder;
        private string _profileBefore;
        private string _chainId = "chain-0001";
        private string _fieldId;
        private int _runIndex = 1;
        private DateTime _runStartedUtc;
        private bool _reportWritten;
        private double _experimentStartedAt;
        private double _stateStartedAt;
        private double _runStartedAt;
        private double _manualPauseStartedAt;
        private float _nextMovementAt;
        private bool _requestedStop;
        private readonly List<string> _offered = new List<string>(3);

        public AutomationRunState State { get; private set; }
        public string TerminalReason { get; private set; }
        public RunOutcome Outcome { get; private set; }
        public MetaRunReceipt Receipt { get; private set; }
        public bool BotStuck { get; private set; }
        public bool CoverageIncomplete { get; private set; }
        public bool IsFinished => State == AutomationRunState.Completed || State == AutomationRunState.Stopped ||
            State == AutomationRunState.Failed;
        public event Action<AutomationRunHost> Finished;

        public void Initialize(GameplayCompositionRoot root, ExperimentConfig config, MemoryProfileStore profileStore = null,
            string chainId = "chain-0001", int runIndex = 1, string fieldId = null)
        {
            if (_root != null) throw new InvalidOperationException("Automation host already initialized.");
            _root = root ?? throw new ArgumentNullException(nameof(root));
            if (!_root.DevelopmentTools) throw new InvalidOperationException("Automation requires Editor or Development Build.");
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _settings = _config.Data;
            _profileStore = profileStore;
            if (string.IsNullOrWhiteSpace(chainId) || chainId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || runIndex < 1)
                throw new ArgumentException("Invalid chain identity or run index.");
            _chainId = chainId;
            _runIndex = runIndex;
            _fieldId = fieldId ?? _settings.FieldRoute[0];
            if (!_settings.FieldRoute.Contains(_fieldId)) throw new ArgumentException("Field is outside configured route.");
            _movement = new BotMovementPolicy(_settings.MovementPolicy);
            // Separate random stream: policy choices never advance the gameplay draft/wave/pickup RNG.
            _draftChoice = new RandomLegalDraftPolicy(new System.Random(Guid.NewGuid().GetHashCode()));
            _experimentStartedAt = Time.realtimeSinceStartupAsDouble;
            Transition(AutomationRunState.WaitingForProfile);
        }

        private void Update()
        {
            if (_root == null || IsFinished) return;
            try
            {
                var now = Time.realtimeSinceStartupAsDouble;
                if (now - _experimentStartedAt >= _settings.MaxExperimentWallSeconds.Value)
                {
                    RequestStop("experimentWallBudget");
                    return;
                }
                switch (State)
                {
                    case AutomationRunState.WaitingForProfile:
                        if (_root.AtMainMenu && _root.Profile?.CanStart == true)
                            Transition(AutomationRunState.SelectRun);
                        else if (StateTimedOut(now)) Fail("profileOrMenuTimeout");
                        break;
                    case AutomationRunState.SelectRun:
                        StartRun(now);
                        break;
                    case AutomationRunState.Running:
                    case AutomationRunState.ResolveDraft:
                        DriveRun(now);
                        break;
                    case AutomationRunState.AwaitResultSave:
                        AwaitSave(now);
                        break;
                }
            }
            catch (Exception error) { Fail("automationException: " + error.Message); }
        }

        private void StartRun(double now)
        {
            if (!_root.AtMainMenu || !_root.Profile.CanStart) { Fail("profileNotReady"); return; }
            if (_profileStore != null)
            {
                _profileBefore = _profileStore.Main ?? throw new InvalidOperationException("Isolated profile missing.");
                _root.ConfigureAutomationExportSink(id =>
                {
                    _runFolder = Path.Combine(_config.OutputDirectory, "chains", _chainId, "runs", id.ToString("N"));
                    if (Directory.Exists(_runFolder)) throw new IOException("Run output already exists: " + _runFolder);
                    return new AutomationPlaytestSink(_runFolder);
                });
            }
            _root.Play();
            if (!_root.AtCharacterSelection || !_root.TryStartCharacter(new ContentId(_settings.CharacterId)) ||
                !_root.TryStartField(new ContentId(_fieldId)))
            { Fail("characterOrFieldUnavailable"); return; }
            _bindings = _root.CreateAutomationRuntimeBindings(_settings.MovementPolicy.ObservationRadius.Value);
            _run = _bindings.Run.Model;
            _runStartedUtc = DateTime.UtcNow;
            _run.Completed += HandleCompleted;
            _recorder = new AutomationRunRecorder(_run, _bindings.Draft, _bindings.Player,
                _bindings.Experience, _bindings.Spawner.Director);
            _direction = new BotDirectionSource();
            _bindings.Mover.ConfigureInputSource(_direction);
            if (!_bindings.Run.SetSpeed(_settings.RunSpeed.Value)) { Fail("runSpeedRejected"); return; }
            _runStartedAt = now;
            _nextMovementAt = 0f;
            Transition(AutomationRunState.Running);
        }

        private void DriveRun(double now)
        {
            if (_run == null || _bindings == null) { Fail("runBindingsMissing"); return; }
            if (Outcome != null || _run.Outcome != null || _run.State == RunState.Won ||
                _run.State == RunState.Lost || _run.State == RunState.Stopped)
            {
                Outcome ??= _run.Outcome;
                _direction.Clear();
                Transition(AutomationRunState.AwaitResultSave);
                return;
            }
            if (now - _runStartedAt >= _settings.RunWallTimeoutSeconds.Value)
            {
                RequestStop("runWallTimeout");
                return;
            }
            if (_bindings.Draft.IsDraftOpen)
            {
                _direction.Clear();
                if (State != AutomationRunState.ResolveDraft) Transition(AutomationRunState.ResolveDraft);
                ResolveOneDraft();
                return;
            }
            if (_run.State != RunState.Running)
            {
                _direction.Clear();
                if (_manualPauseStartedAt <= 0) _manualPauseStartedAt = now;
                if (now - _manualPauseStartedAt >= _settings.TransitionTimeoutSeconds.Value)
                    RequestStop(_run.IsPausedBy(RunPauseReasons.Manual) ? "manualPauseTimeout" : "unresolvedPauseTimeout");
                return;
            }
            _manualPauseStartedAt = 0;
            if (State == AutomationRunState.ResolveDraft) Transition(AutomationRunState.Running);
            if (_run.Elapsed < _nextMovementAt) return;
            var decision = _movement.Decide(_bindings.Observation.Capture(), _settings.MovementPolicy.DecisionIntervalSeconds.Value);
            _direction.SetDirection(decision.Direction);
            if (decision.Stuck) BotStuck = true;
            if (decision.CoverageIncomplete) CoverageIncomplete = true;
            _nextMovementAt = _run.Elapsed + _settings.MovementPolicy.DecisionIntervalSeconds.Value;
        }

        private void ResolveOneDraft()
        {
            if (!_bindings.Draft.IsDraftOpen) return;
            if (StateTimedOut(Time.realtimeSinceStartupAsDouble)) { RequestStop("draftTimeout"); return; }
            var session = _bindings.Draft.CurrentDraft;
            _offered.Clear();
            foreach (var option in session.Options) _offered.Add(option.Definition.Id.ToString());
            var selected = _draftChoice.Choose(_offered);
            if (selected == null) return; // The owner resolves empty requests without an open session.
            // Stale revisions are not retried in a loop: the next Update captures the new session.
            _bindings.Draft.Select(new ContentId(selected), session.Revision);
        }

        private void AwaitSave(double now)
        {
            if (Outcome == null) { Fail("missingAuthoritativeOutcome"); return; }
            var save = _root.ProfileSaveTask;
            if (!save.IsCompleted)
            {
                if (StateTimedOut(now)) Fail("profileSaveTimeout");
                return;
            }
            if (!save.GetAwaiter().GetResult() || !_root.Profile.CanStart)
            { Fail("profileSaveFailed"); return; }
            Receipt = _root.Profile.LastReceipt;
            if (Receipt == null || Receipt.RunId != Outcome.RunId.ToString())
            { Fail("receiptMissingOrMismatched"); return; }
            if (_profileStore != null)
            {
                var telemetry = _root.Playtest as PlaytestSession;
                if (telemetry == null) { Fail("telemetryUnavailable"); return; }
                if (telemetry.PendingExport.IsFaulted || telemetry.LastExportError != null)
                { Fail("telemetryExportFailed: " + telemetry.LastExportError); return; }
                if (!telemetry.FinalExportQueued || telemetry.FinalExportPath == null)
                {
                    if (StateTimedOut(now)) Fail("telemetryExportTimeout");
                    return;
                }
                WriteReport(true, null);
            }
            if (_requestedStop || Outcome.Reason != RunCompletionReason.Victory && Outcome.Reason != RunCompletionReason.Defeat)
                Transition(AutomationRunState.Stopped, TerminalReason ?? "administrativeStop");
            else Transition(AutomationRunState.Completed, Outcome.Reason.ToString());
        }

        public void RequestStop(string reason)
        {
            if (IsFinished) return;
            _requestedStop = true;
            TerminalReason = reason ?? "requestedStop";
            _direction?.Clear();
            if (_run != null && _run.Outcome == null)
            {
                _root.QuitProfileRun();
                Outcome = _run.Outcome;
                Transition(AutomationRunState.AwaitResultSave);
            }
            else if (Outcome != null || _run?.Outcome != null)
            {
                Outcome ??= _run.Outcome;
                Transition(AutomationRunState.AwaitResultSave);
            }
            else Transition(AutomationRunState.Stopped, TerminalReason);
        }

        private void HandleCompleted(RunOutcome outcome) => Outcome = outcome;

        private bool StateTimedOut(double now) => now - _stateStartedAt >= _settings.TransitionTimeoutSeconds.Value;

        private void Fail(string reason)
        {
            if (IsFinished) return;
            _direction?.Clear();
            if (_run != null && _run.Outcome == null) _root?.QuitProfileRun();
            if (_profileStore != null && _runFolder != null)
            {
                try { WriteReport(false, reason); }
                catch (Exception error) { reason += "; partialReportError: " + error.Message; }
            }
            Transition(AutomationRunState.Failed, reason);
        }

        private void WriteReport(bool complete, string error)
        {
            if (_reportWritten || _runFolder == null) return;
            Directory.CreateDirectory(_runFolder);
            AutomationPlaytestSink.WriteAtomic(Path.Combine(_runFolder, "profile-before.json"), _profileBefore);
            var rewardProfile = _profileStore.Main;
            if (rewardProfile != null)
                AutomationPlaytestSink.WriteAtomic(Path.Combine(_runFolder, "profile-after-reward.json"), rewardProfile);
            var recorder = _recorder?.Snapshot();
            var payload = new JObject
            {
                ["schemaVersion"] = 1, ["experimentId"] = _config.ExperimentId,
                ["chainId"] = _chainId, ["runId"] = _run.RunId.ToString("N"), ["runIndex"] = _runIndex,
                ["configSha256"] = TelemetryProvenance.Hash(_config.ToString()),
                ["initialProfileSha256"] = _config.InitialProfileSha256,
                ["template"] = _settings.Template, ["characterId"] = _settings.CharacterId,
                ["fieldId"] = _fieldId, ["runSpeed"] = _settings.RunSpeed,
                ["startedUtc"] = _runStartedUtc.ToString("O"), ["endedUtc"] = DateTime.UtcNow.ToString("O"),
                ["simulationSeconds"] = _run.Elapsed, ["outcome"] = Outcome?.Reason.ToString(),
                ["completionReason"] = complete && !_requestedStop &&
                    (Outcome?.Reason == RunCompletionReason.Victory || Outcome?.Reason == RunCompletionReason.Defeat)
                    ? "completed" : "incomplete",
                ["telemetryComplete"] = complete,
                ["receipt"] = Receipt == null ? null : JObject.FromObject(Receipt),
                ["seeds"] = new JObject { ["draft"] = _root.DraftSeed, ["wave"] = _root.WaveSeed,
                    ["layout"] = _root.LayoutSeed, ["traveler"] = _root.TravelerSeed, ["pickup"] = _root.PickupSeed },
                ["rngCoverage"] = "gameplay seeds captured; policy seed not replayable",
                ["botStuck"] = BotStuck, ["coverageIncomplete"] = CoverageIncomplete,
                ["recorder"] = recorder, ["error"] = error,
                ["capabilities"] = new JObject { ["damageAndHealing"] = _root.Playtest is PlaytestSession,
                    ["setDamageAttribution"] = "unsupported", ["phaseEvents"] = recorder != null }
            };
            AutomationPlaytestSink.WriteAtomic(Path.Combine(_runFolder, "automation.json"), payload.ToString());
            _reportWritten = true;
        }

        private void Transition(AutomationRunState state, string reason = null)
        {
            State = state;
            _stateStartedAt = Time.realtimeSinceStartupAsDouble;
            if (reason != null) TerminalReason = reason;
            if (IsFinished) Finished?.Invoke(this);
        }

        private void OnDestroy()
        {
            if (_run != null) _run.Completed -= HandleCompleted;
            _recorder?.Dispose();
            if (_bindings?.Mover != null) _bindings.Mover.ConfigureInputSource(null);
            _direction?.Clear();
        }
    }
}
