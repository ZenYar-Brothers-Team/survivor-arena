using System;
using System.Collections.Generic;

namespace Game.Run
{
    public sealed class RunModel
    {
        public const float DefaultDurationSeconds = 15f * 60f;

        public float Duration { get; }
        public float Elapsed { get; private set; }
        public RunState State { get; private set; } = RunState.NotStarted;
        public int PauseReasonCount => _pauseReasons.Count;

        private readonly HashSet<string> _pauseReasons = new HashSet<string>(StringComparer.Ordinal);

        public event Action Won;
        public event Action Lost;
        public event Action<RunState> StateChanged;

        public RunModel(float duration = DefaultDurationSeconds)
        {
            if (float.IsNaN(duration) || float.IsInfinity(duration) || duration <= 0f)
                throw new ArgumentOutOfRangeException(nameof(duration), "Run duration must be finite and greater than zero.");
            Duration = duration;
        }

        public void Start()
        {
            if (State != RunState.NotStarted)
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
            if (State != RunState.Running && State != RunState.Paused)
                return false;
            if (!_pauseReasons.Add(reason))
                return false;
            if (State == RunState.Running)
                SetState(RunState.Paused);
            return true;
        }

        public bool ReleasePause(string reason)
        {
            ValidatePauseReason(reason);
            if (!_pauseReasons.Remove(reason))
                return false;
            if (State == RunState.Paused && _pauseReasons.Count == 0)
                SetState(RunState.Running);
            return true;
        }

        public bool IsPausedBy(string reason)
        {
            ValidatePauseReason(reason);
            return _pauseReasons.Contains(reason);
        }

        public void Tick(float deltaTime)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta time must be finite and non-negative.");
            if (State != RunState.Running)
                return;

            Elapsed += deltaTime;

            if (Elapsed >= Duration)
            {
                Elapsed = Duration;
                SetState(RunState.Won);
                Won?.Invoke();
            }
        }

        public void Kill()
        {
            if (State != RunState.Running)
                return;

            SetState(RunState.Lost);
            Lost?.Invoke();
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
