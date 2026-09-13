using System;

namespace Game.Run
{
    public sealed class RunModel
    {
        public const float DefaultDurationSeconds = 15f * 60f;

        public float Duration { get; }
        public float Elapsed { get; private set; }
        public RunState State { get; private set; } = RunState.NotStarted;

        public event Action Won;
        public event Action Lost;
        public event Action<RunState> StateChanged;

        public RunModel(float duration = DefaultDurationSeconds)
        {
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
            if (State != RunState.Running)
                return;

            SetState(RunState.Paused);
        }

        public void Resume()
        {
            if (State != RunState.Paused)
                return;

            SetState(RunState.Running);
        }

        public void Tick(float deltaTime)
        {
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
    }
}
