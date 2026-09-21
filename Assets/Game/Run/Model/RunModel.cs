using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Run
{
    public sealed class RunModel
    {
        public const float DefaultDurationSeconds = 15f * 60f;

        public float Duration { get; }
        public float Elapsed { get; private set; }
        public RunState State { get; private set; } = RunState.NotStarted;
        public int PauseReasonCount => _pauseReasons.Count;
        public Guid RunId { get; } = Guid.NewGuid();
        public RunOutcome Outcome { get; private set; }

        private readonly HashSet<string> _pauseReasons = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, IRunOutcomeContributor> _contributors =
            new Dictionary<string, IRunOutcomeContributor>(StringComparer.Ordinal);
        private bool _completing;

        public event Action Won;
        public event Action Lost;
        public event Action<RunState> StateChanged;
        public event Action<RunOutcome> Completed;
        /// <summary>Accepted pause ownership transitions, including additional reasons while paused.</summary>
        public event Action<string, bool> PauseChanged;

        public RunModel(float duration = DefaultDurationSeconds)
        {
            NumericValidation.ValidatePositive(duration, nameof(duration));
            Duration = duration;
        }

        public void Start()
        {
            if (State != RunState.NotStarted || _completing)
                return;

            SetState(RunState.Running);
        }

        public void Pause()
        {
            RequestPause(RunPauseReasons.Manual);
        }

        public void Resume()
        {
            ReleasePause(RunPauseReasons.Manual);
        }

        public bool RequestPause(string reason)
        {
            ValidatePauseReason(reason);
            if (_completing || (State != RunState.Running && State != RunState.Paused))
                return false;
            if (!_pauseReasons.Add(reason))
                return false;
            if (State == RunState.Running)
                SetState(RunState.Paused);
            PauseChanged?.Invoke(reason, true);
            return true;
        }

        public bool ReleasePause(string reason)
        {
            ValidatePauseReason(reason);
            if (_completing) return false;
            if (!_pauseReasons.Remove(reason))
                return false;
            if (State == RunState.Paused && _pauseReasons.Count == 0)
                SetState(RunState.Running);
            PauseChanged?.Invoke(reason, false);
            return true;
        }

        public bool IsPausedBy(string reason)
        {
            ValidatePauseReason(reason);
            return _pauseReasons.Contains(reason);
        }

        public void Tick(float deltaTime)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            if (State != RunState.Running || _completing)
                return;

            Elapsed += deltaTime;

            if (Elapsed >= Duration)
            {
                Elapsed = Duration;
                Complete(RunCompletionReason.Victory, RunState.Won);
            }
        }

        public void Kill()
        {
            if (State != RunState.Running)
                return;

            Complete(RunCompletionReason.Defeat, RunState.Lost);
        }

        /// <summary>Register before completion; the owner must unregister when it shuts down.</summary>
        public void RegisterOutcomeContributor(IRunOutcomeContributor contributor)
        {
            if (contributor == null) throw new ArgumentNullException(nameof(contributor));
            if (string.IsNullOrWhiteSpace(contributor.Key)) throw new ArgumentException("Contributor key is required.", nameof(contributor));
            if (Outcome != null || _completing) throw new InvalidOperationException("The run is already completing.");
            _contributors.Add(contributor.Key, contributor);
        }

        public bool UnregisterOutcomeContributor(IRunOutcomeContributor contributor)
        {
            if (contributor == null) return false;
            return _contributors.TryGetValue(contributor.Key, out var current) &&
                ReferenceEquals(current, contributor) && _contributors.Remove(contributor.Key);
        }

        /// <summary>Stops even a paused/unstarted session without victory, defeat, or reward policy.</summary>
        public RunOutcome Stop(RunCompletionReason reason = RunCompletionReason.Aborted)
        {
            if (reason != RunCompletionReason.Aborted && reason != RunCompletionReason.Retry && reason != RunCompletionReason.Error)
                throw new ArgumentOutOfRangeException(nameof(reason));
            Complete(reason, RunState.Stopped);
            return Outcome;
        }

        private void Complete(RunCompletionReason reason, RunState state)
        {
            if (Outcome != null || _completing) return;
            _completing = true;
            var contributions = new Dictionary<string, RunOutcomeContribution>(StringComparer.Ordinal);
            var failures = new List<string>();
            // Copy registrations: a producer cannot invalidate enumeration during capture.
            foreach (var pair in new Dictionary<string, IRunOutcomeContributor>(_contributors))
            {
                try
                {
                    var value = pair.Value.Capture();
                    if (value == null) failures.Add(pair.Key);
                    else contributions.Add(pair.Key, value);
                }
                catch (Exception) { failures.Add(pair.Key); }
            }
            Outcome = new RunOutcome(RunId, reason, Elapsed, Duration, contributions, failures);
            _contributors.Clear();
            _pauseReasons.Clear();
            // Preserve legacy ordering: StateChanged precedes Won/Lost; snapshot is already available.
            SetState(state);
            if (state == RunState.Won) Won?.Invoke();
            else if (state == RunState.Lost) Lost?.Invoke();
            Completed?.Invoke(Outcome);
        }

        private void SetState(RunState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }

        private static void ValidatePauseReason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Pause reason cannot be empty.", nameof(reason));
        }
    }
}
