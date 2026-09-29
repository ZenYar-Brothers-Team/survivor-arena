using System;
using System.Collections.Generic;
using Game.Character;
using Game.Enemy;
using Game.Progression;
using Game.Run;
using Newtonsoft.Json.Linq;

namespace Game.Bootstrap.Automation
{
    /// <summary>Bounded, main-thread, typed history. Counters survive event-buffer overflow.</summary>
    public sealed class AutomationRunRecorder : IDisposable
    {
        private readonly RunModel _run;
        private readonly LevelUpDraftRuntime _draft;
        private readonly PlayerCharacterRuntime _player;
        private readonly PlayerExperienceRuntime _xp;
        private readonly WaveDirector _waves;
        private readonly AutomationEventBuffer _events = new AutomationEventBuffer(2048);
        private int _offers;
        private int _selections;
        private int _phaseEntries;
        private bool _disposed;
        public AutomationRunRecorder(RunModel run, LevelUpDraftRuntime draft, PlayerCharacterRuntime player,
            PlayerExperienceRuntime xp, WaveDirector waves)
        {
            _run = run ?? throw new ArgumentNullException(nameof(run));
            _draft = draft ?? throw new ArgumentNullException(nameof(draft));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _xp = xp ?? throw new ArgumentNullException(nameof(xp));
            _waves = waves ?? throw new ArgumentNullException(nameof(waves));
            _draft.DraftOpened += OnOffer;
            _draft.SelectionApplied += OnSelection;
            _waves.PhaseChanged += OnPhase;
            OnPhase(_waves.CurrentPhase, _waves.CurrentPhaseIndex);
        }
        private void OnPhase(WavePhaseDefinition phase, int index)
        {
            _phaseEntries++;
            _events.Add(new JObject { ["type"] = "phase", ["simulationSeconds"] = _run.Elapsed,
                ["phaseId"] = phase.Id.ToString(), ["phaseIndex"] = index,
                ["hp"] = _player.Health.CurrentHealth, ["maxHp"] = _player.Health.MaxHealth,
                ["level"] = _xp.Progression.Level, ["experience"] = _xp.Progression.CurrentExperience });
        }
        private void OnOffer(IReadOnlyList<DraftOption> options)
        {
            _offers++;
            var choices = new JArray();
            foreach (var option in options)
                choices.Add(new JObject { ["id"] = option.Definition.Id.ToString(), ["level"] = option.ResultingLevel,
                    ["kind"] = option.Definition.Kind.ToString() });
            _events.Add(new JObject { ["type"] = "draftOffer", ["simulationSeconds"] = _run.Elapsed,
                ["requestId"] = _draft.CurrentRequest?.Id.ToString("N"),
                ["origin"] = _draft.CurrentRequest?.Origin.ToString(), ["revision"] = _draft.Revision.ToString("N"),
                ["options"] = choices });
        }
        private void OnSelection(BuildSelectionResult result)
        {
            _selections++;
            _events.Add(new JObject { ["type"] = "draftSelection", ["simulationSeconds"] = _run.Elapsed,
                ["id"] = result.Entry.Definition.Id.ToString(), ["level"] = result.Entry.Level,
                ["kind"] = result.Entry.Definition.Kind.ToString(), ["newEntry"] = result.WasNewEntry });
        }
        public JObject Snapshot()
        {
            var build = new JArray();
            foreach (var entry in _draft.Build.Entries)
                build.Add(new JObject { ["id"] = entry.Definition.Id.ToString(), ["level"] = entry.Level,
                    ["kind"] = entry.Definition.Kind.ToString() });
            return new JObject { ["events"] = _events.Snapshot(), ["droppedEvents"] = _events.DroppedCount,
                ["offerCount"] = _offers, ["selectionCount"] = _selections, ["phaseEntryCount"] = _phaseEntries,
                ["reachedPhaseId"] = _waves.CurrentPhase.Id.ToString(),
                ["reachedPhaseIndex"] = _waves.CurrentPhaseIndex,
                ["terminalHp"] = _player.Health.CurrentHealth, ["terminalLevel"] = _xp.Progression.Level,
                ["terminalExperience"] = _xp.Progression.CurrentExperience, ["build"] = build };
        }
        public void Dispose()
        {
            if (_disposed) return;
            _draft.DraftOpened -= OnOffer;
            _draft.SelectionApplied -= OnSelection;
            _waves.PhaseChanged -= OnPhase;
            _disposed = true;
        }
    }
}
