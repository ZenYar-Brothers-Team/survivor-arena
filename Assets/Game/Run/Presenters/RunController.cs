using UnityEngine;

namespace Game.Run
{
    public sealed class RunController : MonoBehaviour
    {
        [SerializeField]
        private float _durationSeconds = RunModel.DefaultDurationSeconds;

        public RunModel Model { get; private set; }

        private void Awake()
        {
            Model = new RunModel(_durationSeconds);
        }

        private void Start()
        {
            Model.Start();
        }

        private void Update()
        {
            Model.Tick(Time.deltaTime);
        }

        public void TogglePause()
        {
            if (Model.State == RunState.Running)
                Model.Pause();
            else if (Model.State == RunState.Paused)
                Model.Resume();
        }
    }
}
