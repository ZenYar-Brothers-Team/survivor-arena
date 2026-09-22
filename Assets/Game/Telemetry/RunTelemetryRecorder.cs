using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Game.Combat;
using Game.Content;
using Game.Diagnostics;
using Game.Run;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Game.Telemetry
{
    /// <summary>Main-thread-only bounded consumer. Does not own gameplay, RNG, rewards, or RunOutcome.</summary>
    public sealed class RunTelemetryRecorder
    {
        private readonly RunModel _run;
        private readonly TelemetryLimits _limits;
        private readonly JObject _provenance;
        private readonly Func<double> _clock;
        private readonly double _started;
        private readonly string _created;
        private readonly List<TelemetryEvent> _timeline;
        private readonly Dictionary<TelemetryCombatKey, TelemetryCombatTotals> _combat;
        private readonly Dictionary<string, double> _counters;
        private readonly HashSet<Guid> _identities;
        private readonly Dictionary<ContentId, float> _equipped;
        private RunState _lastState;
        private double _lastWall, _pauseSeconds, _terminalWall;
        private long _sequence, _markers;
        private PlaytestReport _final;
        public string ReportId { get; } = Guid.NewGuid().ToString("N");
        public long DroppedEvents { get; private set; }
        public long DroppedKeys { get; private set; }
        public long DroppedIdentities { get; private set; }
        public bool IsSealed => _final != null;

        public RunTelemetryRecorder(RunModel run, TelemetryLimits limits, JObject provenance,
            Func<double> clock, DateTime createdUtc)
        {
            _run = run ?? throw new ArgumentNullException(nameof(run));
            _limits = limits ?? throw new ArgumentNullException(nameof(limits));
            _provenance = (JObject)(provenance ?? throw new ArgumentNullException(nameof(provenance))).DeepClone();
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _started = _lastWall = clock(); _created = createdUtc.ToUniversalTime().ToString("O");
            _lastState = run.State;
            _timeline = new List<TelemetryEvent>(limits.Timeline);
            _combat = new Dictionary<TelemetryCombatKey, TelemetryCombatTotals>(limits.Aggregates);
            _counters = new Dictionary<string, double>(limits.Aggregates, StringComparer.Ordinal);
            _identities = new HashSet<Guid>();
            _equipped = new Dictionary<ContentId, float>();
            Event("recording-start", run.State.ToString());
        }

        /// <summary>Invoke for actual producer transitions. Records wall pause time, never DPS time.</summary>
        public void StateChanged(RunState state)
        {
            if (IsSealed) return;
            var now = _clock();
            if (_lastState == RunState.Paused) _pauseSeconds += Math.Max(0, now - _lastWall);
            _lastWall = now; _lastState = state;
            if (_run.Outcome != null) _terminalWall = now;
            Event("run-state", state.ToString());
        }

        public void Event(string kind, string detail)
        {
            if (IsSealed) return;
            if (_timeline.Count == _limits.Timeline) { DroppedEvents++; return; }
            _timeline.Add(new TelemetryEvent(++_sequence, _run.Elapsed, Clip(kind), Clip(detail)));
        }
        public void Marker(string text) => Event("marker", (++_markers).ToString(CultureInfo.InvariantCulture) + ": " + text);
        private string Clip(string text) => text == null ? "unknown" : text.Length <= _limits.Text ? text : text.Substring(0, _limits.Text);

        /// <summary>Deduplicates stable producer identities without eviction; saturation is explicit.</summary>
        public bool Claim(Guid id)
        {
            if (IsSealed || id == Guid.Empty || _identities.Contains(id)) return false;
            if (_identities.Count >= _limits.Identities) { DroppedIdentities++; return false; }
            _identities.Add(id); return true;
        }
        public void Count(string key, double amount = 1)
        {
            if (IsSealed) return;
            if (!_counters.ContainsKey(key) && _counters.Count >= _limits.Aggregates) { DroppedKeys++; return; }
            _counters.TryGetValue(key, out var previous);
            _counters[key] = previous + amount;
        }
        public void Equip(ContentId id)
        {
            if (IsSealed || _equipped.ContainsKey(id)) return;
            if (_equipped.Count >= _limits.Aggregates) { DroppedKeys++; return; }
            _equipped.Add(id, _run.Elapsed);
        }

        /// <summary>Called once by each measured-result producer, including callbacks unwinding after terminal.</summary>
        public void Combat(CombatResult result)
        {
            if (IsSealed || result.Target.RunId != _run.RunId) return;
            var key = new TelemetryCombatKey(result);
            if (!_combat.TryGetValue(key, out var totals) && _combat.Count == _limits.Aggregates)
            { DroppedKeys++; return; }
            totals.Results++; totals.Attempted += result.Health.Requested;
            totals.Applied += result.Health.Actual; totals.Overkill += result.Health.Overkill;
            _combat[key] = totals;
        }

        /// <summary>Call at a safe update/export boundary, after synchronous producer callbacks have unwound.</summary>
        public PlaytestReport Snapshot(object producerSnapshot, bool seal)
        {
            if (_final != null) return _final;
            if (seal && _run.Outcome == null) throw new InvalidOperationException("Cannot finalize a live run.");
            using var guard = PerfGuard.Measure("Telemetry.Snapshot", 20f);
            var serializer = TelemetryJson.CreateSerializer();
            var rows = new JArray();
            double damageDealt = 0, damageTaken = 0, healing = 0;
            foreach (var pair in _combat)
            {
                var key = pair.Key; var value = pair.Value;
                double? equippedSeconds = key.Source.HasValue && _equipped.TryGetValue(key.Source.Value, out var start)
                    ? _run.Elapsed - start : (double?)null;
                rows.Add(new JObject
                {
                    ["source"] = key.Source?.ToString() ?? "unknown", ["level"] = key.Level,
                    ["origin"] = key.Origin.ToString(), ["targetCategory"] = key.Target.ToString(),
                    ["healing"] = key.Healing, ["results"] = value.Results, ["attempted"] = value.Attempted,
                    ["applied"] = value.Applied, ["overkill"] = key.Healing ? JValue.CreateNull() : new JValue(value.Overkill),
                    ["equippedRunningSeconds"] = equippedSeconds,
                    ["observedEquippedDps"] = !key.Healing && equippedSeconds > 0 ? value.Applied / equippedSeconds : null
                });
                if (key.Healing) healing += value.Applied;
                else if (key.Target == CombatEntityCategory.Player) damageTaken += value.Applied;
                else damageDealt += value.Applied;
            }
            var now = _clock();
            var paused = _pauseSeconds + (_lastState == RunState.Paused ? Math.Max(0, now - _lastWall) : 0);
            var reason = _run.Outcome?.Reason.ToString() ?? "incomplete";
            var completion = reason == "Victory" || reason == "Defeat" ? "completed" : reason.ToLowerInvariant();
            var data = new JObject
            {
                ["schemaVersion"] = 1, ["recorderVersion"] = "IP-31/1", ["reportId"] = ReportId,
                ["runId"] = _run.RunId.ToString("N"), ["createdUtc"] = _created,
                ["completionReason"] = completion, ["outcome"] = _run.Outcome == null ? JValue.CreateNull() : JToken.FromObject(_run.Outcome, serializer),
                ["provenance"] = _provenance.DeepClone(), ["runningSeconds"] = _run.Elapsed,
                ["elapsedSimulationSeconds"] = _run.Elapsed, ["wallSeconds"] = Math.Max(0, (_run.Outcome != null ? _terminalWall : now) - _started),
                ["pauseWallSeconds"] = paused,
                ["combat"] = rows, ["appliedDamageDealt"] = damageDealt, ["appliedDamageTaken"] = damageTaken,
                ["actualHealing"] = healing, ["observedRunDps"] = _run.Elapsed > 0 ? new JValue(damageDealt / _run.Elapsed) : JValue.CreateNull(),
                ["counters"] = JObject.FromObject(new SortedDictionary<string, double>(_counters, StringComparer.Ordinal)),
                ["timeline"] = JArray.FromObject(_timeline, serializer),
                ["producers"] = producerSnapshot == null ? JValue.CreateNull() : JToken.FromObject(producerSnapshot, serializer),
                ["quality"] = new JObject
                {
                    ["droppedTimelineEvents"] = DroppedEvents, ["droppedAggregateUpdates"] = DroppedKeys,
                    ["droppedIdentities"] = DroppedIdentities, ["limits"] = JObject.FromObject(_limits, serializer),
                    ["unsupported"] = new JArray("boss", "traveler", "meta", "character baseline-relative details", "set effect attribution", "wave cap decisions", "phase combat attribution", "per-projectile counts", "FPS/p95"),
                    ["incompleteReason"] = _run.Outcome == null ? "Run has not ended" : null,
                    ["combatCoverage"] = "Player adapters and ordinary spawner enemies; direct Health mutations are uninstrumented",
                    ["exportErrors"] = "Reported by Playtest UI; I/O may prevent any file from being written"
                }
            };
            var json = data.ToString(Formatting.Indented);
            if (Encoding.UTF8.GetByteCount(json) > _limits.ExportBytes) throw new InvalidOperationException("Report exceeds export byte budget.");
            var summary = FormattableString.Invariant($"# Fixture playtest\n\nReport: {ReportId}\nRun: {_run.RunId:N}\nCompletion: {completion} / {reason}\nConfig: {_provenance["configHash"]}\nRunning: {_run.Elapsed:0.###} s; pause wall: {paused:0.###} s\nApplied damage dealt/taken: {damageDealt:0.###} / {damageTaken:0.###} HP\nDropped timeline/keys/identities: {DroppedEvents}/{DroppedKeys}/{DroppedIdentities}\n\nSee run.json quality/capabilities before comparing runs. One fixture run does not establish balance quality.\n");
            var report = new PlaytestReport(ReportId, json, summary,
                $"# Tester feedback\n\nReport: {ReportId}\nRun: {_run.RunId:N}\nConfig: {_provenance["configHash"]}\n\nScenario / experience:\nSimulation time → observed → expected:\nDifficulty / readability:\nDraft choice wanted / offered:\nDEV interventions:\nEnd / abort reason:\n");
            if (seal) _final = report;
            return report;
        }
    }
}
