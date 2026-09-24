using UnityEngine;

namespace Game.Run
{
    public sealed class RunController : MonoBehaviour
    {
        [SerializeField]
        private float _durationSeconds = RunModel.DefaultDurationSeconds;

        public RunModel Model { get; private set; }
        public bool IsInitialized { get; private set; }
        private bool _ownsTimeScale;

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            if (IsInitialized) Model.Start();
        }

        private void Update()
        {
            if (IsInitialized) Model.Tick(Time.deltaTime);
        }

        /// <summary>Creates a fresh session. Tear down all consumers before replacing the old model.</summary>
        public void Initialize()
        {
            if (IsInitialized) throw new System.InvalidOperationException("Run controller is already initialized.");
            Model = new RunModel(_durationSeconds);
            Model.SpeedChanged += HandleSpeedChanged;
            Model.StateChanged += HandleStateChanged;
            IsInitialized = true;
        }

        /// <summary>Captures before owners tear down. Retains the terminal result until the next Initialize.</summary>
        public void Shutdown()
        {
            if (!IsInitialized) return;
            Model.Stop();
            Model.SpeedChanged -= HandleSpeedChanged;
            Model.StateChanged -= HandleStateChanged;
            RestoreTimeScale();
            IsInitialized = false;
        }

        private void OnDestroy() => Shutdown();

        public void TogglePause()
        {
            if (!IsInitialized) return;
            if (Model.IsPausedBy(RunPauseReasons.Manual))
                Model.ReleasePause(RunPauseReasons.Manual);
            else
                Model.RequestPause(RunPauseReasons.Manual);
        }

        public bool SetSpeed(int multiplier) => IsInitialized && Model.SetSpeed(multiplier);

        private void HandleSpeedChanged(int _) => ApplyTimeScale();

        private void HandleStateChanged(RunState _) => ApplyTimeScale();

        private void ApplyTimeScale()
        {
            if (Model.State == RunState.Running)
            {
                Time.timeScale = Model.SpeedMultiplier;
                _ownsTimeScale = true;
            }
            else RestoreTimeScale();
        }

        private void RestoreTimeScale()
        {
            if (!_ownsTimeScale) return;
            Time.timeScale = 1f;
            _ownsTimeScale = false;
        }
    }
}
