using Game.Traveler;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Game.ActiveSkill;
using Game.Character;
using Game.Combat;
using Game.Enemy;
using Game.Progression;
using Game.Pickup;
using Game.Run;
using Newtonsoft.Json.Linq;

namespace Game.Telemetry
{
    /// <summary>Main-thread adapter; snapshot in Tick, worker-only export. Dispose before gameplay owners.</summary>
    public sealed class PlaytestSession : IPlaytestSession, IDisposable
    {
        private readonly RunModel _run;
        private readonly PlayerCharacterRuntime _player;
        private readonly PlayerExperienceRuntime _xp;
        private readonly LevelUpDraftRuntime _draft;
        private readonly ContinuousFixtureEnemySpawner _spawner;
        private readonly PlayerActiveSkillSetRuntime _skills;
        private readonly IPlaytestExportSink _sink;
        private readonly IPickupRuntime _pickups;
        private readonly ITravelerRuntime _travelers;
        private Task<string> _export;
        private bool _disposed, _finalExportQueued;
        private bool _exportRequested;
        private string _status = "Recording";
        public RunTelemetryRecorder Recorder { get; }
        /// <summary>Worker completion for hosts/tests that must finish I/O before shutdown.</summary>
        public Task PendingExport => _export ?? Task.CompletedTask;
        public bool Enabled => !_disposed;
        public string Summary => $"{_status}\nSession: {Recorder.ReportId}\nDropped: {Recorder.DroppedEvents + Recorder.DroppedKeys + Recorder.DroppedIdentities}";

        public PlaytestSession(RunModel run, PlayerCharacterRuntime player, PlayerExperienceRuntime xp,
            LevelUpDraftRuntime draft, ContinuousFixtureEnemySpawner spawner, PlayerActiveSkillSetRuntime skills,
            JObject provenance, IPlaytestExportSink sink, Func<double> clock, DateTime createdUtc, TelemetryLimits limits = null, IPickupRuntime pickups = null, ITravelerRuntime travelers = null)
        {
            _run = run ?? throw new ArgumentNullException(nameof(run));
            if (player != null && player.Health == null) throw new ArgumentException("Player must be initialized.", nameof(player));
            if (xp != null && !xp.IsInitialized) throw new ArgumentException("XP must be initialized.", nameof(xp));
            if (draft != null && draft.Build == null) throw new ArgumentException("Draft must be initialized.", nameof(draft));
            _player = player; _xp = xp; _draft = draft; _spawner = spawner; _skills = skills;
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            Recorder = new RunTelemetryRecorder(run, limits ?? new TelemetryLimits(), provenance, clock, createdUtc);
            _travelers = travelers;
            if (_travelers != null) { _travelers.LifeEvent += OnTraveler; _travelers.CombatResolved += Recorder.Combat; }
            _pickups = pickups;
            if (_pickups != null) _pickups.Resolved += OnPickup;
            // All failure-prone construction precedes subscriptions, so rollback cannot leak listeners.
            _run.StateChanged += Recorder.StateChanged;
            _run.PauseChanged += OnPause;
            if (_player != null) _player.CombatResolved += Recorder.Combat;
            if (_xp != null) { _xp.ExperienceResolved += OnExperience; _xp.LevelUp += OnLevel; }
            if (_draft != null)
            {
                _draft.DraftOpened += OnOffer; _draft.RequestResolved += OnResolution;
                _draft.ControlAttempted += OnControl; _draft.RequestQueued += OnQueued;
                _draft.SelectionApplied += OnSelection;
                foreach (var entry in _draft.Build.Entries) Recorder.Equip(entry.Definition.Id);
            }
            if (_spawner != null)
            {
                _spawner.LifeEvent += OnLife; _spawner.CombatResolved += Recorder.Combat;
                if (_spawner.Director != null)
                {
                    _spawner.Director.PhaseChanged += OnPhase;
                    _spawner.Director.HookTriggered += OnHook;
                }
            }
        }
        private void OnTraveler(TravelerEvent item)
        {
            if (item.Traveler.RunId != _run.RunId) return;
            Recorder.Count("traveler." + item.Outcome);
            Recorder.Event("traveler", $"{item.Traveler.LifeId:N} {item.Traveler.Id} {item.Outcome} role {item.Traveler.Role} spawn {item.Traveler.SpawnTime} until {item.Traveler.Deadline} scale {item.Traveler.Scale}");
        }
        private void OnPickup(PickupEvent snapshot)
        {
            if (snapshot.Identity.RunId != _run.RunId || !Recorder.Claim(snapshot.Identity.DropId)) return;
            Recorder.Count("pickup." + snapshot.Kind + "." + snapshot.State);
            Recorder.Event("pickup", $"{snapshot.Identity.DropId:N} {snapshot.ContentId} {snapshot.State} run {snapshot.Identity.RunId:N} source {snapshot.Identity.SourceLifeId} {snapshot.Identity.SourceContentId} requested {snapshot.Healing.Requested} actual {snapshot.Healing.Actual}");
        }
        private void OnPause(string reason, bool added) => Recorder.Event(added ? "pause-acquired" : "pause-released", reason);
        private void OnLevel(int level) => Recorder.Event("level", level.ToString());
        private void OnPhase(WavePhaseDefinition phase, int index) => Recorder.Event("legacy-wave-phase", $"{index}: {phase.Id}");
        private void OnHook(WaveHookDefinition hook) => Recorder.Event("legacy-wave-hook-not-spawn", hook.Kind.ToString());
        private void OnQueued(DraftRequest request) => Recorder.Event("draft-queued", $"{request.Id:N} {request.Origin} L{request.EarnedLevel} pickup {request.PickupId}");
        private void OnSelection(BuildSelectionResult result) => Recorder.Equip(result.Entry.Definition.Id);
        private void OnOffer(IReadOnlyList<DraftOption> options)
        {
            var text = new StringBuilder().Append(_draft.CurrentRequest?.Id).Append(" revision ").Append(_draft.Revision);
            foreach (var option in options) text.Append(" | ").Append(option.Definition.Id).Append(" L").Append(option.ResultingLevel);
            Recorder.Event("draft-offered", text.ToString());
        }
        private void OnResolution(DraftResolution resolution)
        {
            if (resolution.Request.RunId != _run.RunId || !Recorder.Claim(resolution.Request.Id)) return;
            Recorder.Count("draft." + resolution.Kind);
            Recorder.Event("draft-resolved", $"{resolution.Request.Id:N} {resolution.Request.Origin} {resolution.Kind} {resolution.SelectedId}");
        }
        private void OnControl(DraftControlAttempt attempt)
        {
            Recorder.Count("draft." + attempt.Action + (attempt.Succeeded ? ".success" : ".failed"));
            Recorder.Event("draft-control", $"{attempt.Action} {attempt.Succeeded} request {attempt.RequestId:N} revision {attempt.Revision:N} {attempt.SelectedId}");
        }
        private void OnExperience(ExperienceAwardEvent award)
        {
            if (award.RunId != _run.RunId || (award.Drop.HasValue && !Recorder.Claim(award.Drop.Value.LifeId))) return;
            Recorder.Count("xp." + award.Kind + ".base", award.BaseAmount);
            Recorder.Count("xp." + award.Kind + ".awarded", award.AwardedAmount);
            if (award.Kind == ExperienceEventKind.DevelopmentIntervention)
                Recorder.Event("intervention-xp", award.BaseAmount.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        private void OnLife(EnemyLifeEvent life)
        {
            if (life.RunId != _run.RunId) return;
            // Claim only deaths; spawn+death share a life ID. Cleanup/escape cannot become kills.
            if (life.Kind == EnemyLifeEventKind.Died && Recorder.Claim(life.LifeId))
                Recorder.Count("kills." + life.ContentId);
        }
        public void AddMarker(string text)
        {
            if (_disposed) return;
            if (Recorder.IsSealed) { _status = "Final report sealed; add notes to feedback.md"; return; }
            Recorder.Marker(text);
        }
        /// <summary>Queues export; never snapshots during a reentrant hit/end callback.</summary>
        public void Export() { if (!_disposed) _exportRequested = true; }

        public void Tick()
        {
            if (_disposed) return;
            if (_export != null && _export.IsCompleted)
            {
                _status = _export.IsFaulted ? "Export error: " + _export.Exception.GetBaseException().Message : "Saved: " + _export.Result;
                _export = null;
            }
            var needsFinal = _run.Outcome != null && !_finalExportQueued;
            if (!needsFinal && (!_exportRequested || _export != null)) return;
            _exportRequested = false;
            var final = _run.Outcome != null;
            if (final) _finalExportQueued = true;
            try
            {
                var report = Recorder.Snapshot(CaptureProducers(), final);
                _status = "Exporting…";
                var previous = _export;
                _export = Task.Run(async () =>
                {
                    // A live export may still be writing when teardown arrives. Preserve ordering
                    // so that it cannot overwrite the final packet; capture before owners disappear.
                    if (previous != null) { try { await previous.ConfigureAwait(false); } catch { /* latest export remains retryable */ } }
                    return _sink.Write(report);
                });
            }
            catch (Exception error) { _status = "Export error: " + error.Message; }
        }
        private object CaptureProducers()
        {
            var activations = new SortedDictionary<string, long>(StringComparer.Ordinal);
            if (_skills != null)
                foreach (var skill in _skills.Skills) activations.Add(skill.Definition.Id.ToString(), skill.TriggerCount);
            return new
            {
                xp = _xp == null ? null : new { totals = _xp.Totals, droppedBase = _xp.DroppedBase, groundBase = _xp.GroundBase,
                    level = _xp.Progression.Level, remainingExperience = _xp.Progression.CurrentExperience, progress01 = _xp.Progression.Progress01 },
                draft = _draft != null && _draft.Build != null ? _draft.Capture() : null,
                ordinaryKills = _spawner != null ? _spawner.Capture() : null,
                pickups = _pickups != null ? (PickupSnapshot?)_pickups.Snapshot : null,
                travelers = _travelers?.Snapshot.Select(item => new { item.LifeId, item.RunId, id = item.Id.ToString(), item.Role,
                    x = item.Position.x, y = item.Position.y, item.Health, item.MaxHealth, item.SpawnTime, item.Deadline, item.Scale }).ToArray(),
                travelerSchedule = _travelers?.Schedule.Select(item => new { id = item.Id.ToString(), item.Time, item.Sequence, item.Scale }).ToArray(),
                activeSkillActivations = _skills != null ? activations : null,
                capabilities = new { playerCombat = _player != null, xp = _xp != null, draft = _draft != null, worldPickups = _pickups != null, travelers = _travelers != null,
                    ordinaryEnemyCombat = _spawner != null, legacyContinuousWave = _spawner != null && _spawner.Director != null,
                    activeSkillActivations = _skills != null, setDetails = "unsupported", characterDetails = "unsupported" }
            };
        }
        public void Dispose()
        {
            if (_disposed) return;
            // Owner must stop/capture the run first. Queued I/O owns only immutable strings.
            Tick();
            _run.StateChanged -= Recorder.StateChanged; _run.PauseChanged -= OnPause;
            if (_travelers != null) { _travelers.LifeEvent -= OnTraveler; _travelers.CombatResolved -= Recorder.Combat; }
            if (_pickups != null) _pickups.Resolved -= OnPickup;
            if (_player != null) _player.CombatResolved -= Recorder.Combat;
            if (_xp != null) { _xp.ExperienceResolved -= OnExperience; _xp.LevelUp -= OnLevel; }
            if (_draft != null)
            {
                _draft.DraftOpened -= OnOffer; _draft.RequestResolved -= OnResolution;
                _draft.ControlAttempted -= OnControl; _draft.RequestQueued -= OnQueued; _draft.SelectionApplied -= OnSelection;
            }
            if (_spawner != null)
            {
                _spawner.LifeEvent -= OnLife; _spawner.CombatResolved -= Recorder.Combat;
                if (_spawner.Director != null)
                { _spawner.Director.PhaseChanged -= OnPhase; _spawner.Director.HookTriggered -= OnHook; }
            }
            _disposed = true;
        }
    }
}
