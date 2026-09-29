using System;
using System.Threading.Tasks;
using Game.Automation;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Game.Bootstrap.Automation
{
    /// <summary>AB-14 run-owned subscription. Capture failures never escape into PlayerMover's physics callback.</summary>
    public sealed class DemonstrationRecordingSession : IDisposable
    {
        private readonly AutomationRuntimeBindings _bindings;
        private readonly DemonstrationObservationAdapter _observation;
        private readonly DemonstrationClock _clock;
        private readonly DemonstrationWriter _writer;
        private Vector2 _previousAction;
        private bool _closed;
        private int _incompleteSamples;
        private int _truncatedSamples;
        public string Error => _writer.Error;
        public int Samples => _writer.AcceptedSamples;
        public Task Completion => _writer.Completion;
        // A later profile/telemetry failure cannot rewrite the already-closed recording's result.
        public void SetError(string reason) { if (!_closed) _writer.SetError(reason); }

        public DemonstrationRecordingSession(AutomationRuntimeBindings bindings, DemonstrationConfigData settings,
            float observationRadius, string path, JObject header)
        {
            _bindings = bindings ?? throw new ArgumentNullException(nameof(bindings));
            settings.Validate();
            _clock = new DemonstrationClock(settings.SampleIntervalSeconds.Value);
            _observation = new DemonstrationObservationAdapter(bindings, observationRadius, settings.MaxEntitiesPerCollection.Value);
            _writer = DemonstrationWriter.CreateFile(path, settings, header);
            _bindings.Mover.MovementIntentApplied += OnMovement;
        }

        private void OnMovement(Vector2 action, float stepSeconds)
        {
            if (_closed || Error != null) return;
            try
            {
                var periodic = _clock.Advance(stepSeconds);
                var actionChanged = (action - _previousAction).sqrMagnitude > 0.0000000001f;
                if (periodic || actionChanged)
                {
                    var observation = _observation.Capture();
                    if (!_observation.CoverageComplete) _incompleteSamples++;
                    if (_observation.TruncatedEntities > 0) _truncatedSamples++;
                    _writer.TryWriteSample(new JObject
                    {
                        ["physicsStep"] = _clock.StepIndex, ["physicsSeconds"] = _clock.StepStartSeconds,
                        ["runSeconds"] = _bindings.Run.Model.Elapsed, ["stepSeconds"] = stepSeconds,
                        ["runSpeed"] = _bindings.Run.Model.SpeedMultiplier,
                        ["captureReason"] = periodic
                            ? actionChanged ? "periodicAndActionChange" : "periodic"
                            : "actionChange",
                        ["previousAction"] = new JArray(_previousAction.x, _previousAction.y),
                        ["action"] = new JArray(action.x, action.y), ["observation"] = observation
                    });
                }
                _previousAction = action;
            }
            catch (Exception error) { _writer.SetError("captureFailure: " + error.Message); }
        }

        public Task Complete(string reason, string outcome)
        {
            if (_closed) return Completion;
            _closed = true;
            if (_bindings.Mover != null) _bindings.Mover.MovementIntentApplied -= OnMovement;
            return _writer.Complete(new JObject { ["reason"] = reason, ["outcome"] = outcome,
                ["physicsSteps"] = _clock.StepIndex + 1, ["incompleteObservationSamples"] = _incompleteSamples,
                ["truncatedObservationSamples"] = _truncatedSamples });
        }

        public JObject Summary() => new JObject { ["schemaVersion"] = 1,
            ["file"] = _writer.RecordingComplete ? "demonstration.jsonl" : "demonstration.jsonl.partial",
            ["recordingComplete"] = _writer.RecordingComplete, ["samples"] = _writer.WrittenSamples,
            ["bytes"] = _writer.WrittenBytes, ["rejectedSamples"] = _writer.RejectedSamples,
            ["incompleteObservationSamples"] = _incompleteSamples, ["truncatedObservationSamples"] = _truncatedSamples,
            ["error"] = Error };

        public void Dispose()
        {
            if (_closed) return;
            _writer.SetError("hostDestroyedBeforeCompletion");
            Complete("hostDestroyed", _bindings.Run.Model.Outcome?.Reason.ToString());
        }
    }
}
