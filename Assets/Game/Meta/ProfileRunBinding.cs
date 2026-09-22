using System;
using System.Threading.Tasks;
using Game.Run;
namespace Game.Meta
{
    /// <summary>Captures model events without telemetry. Dispose after terminal capture, before replacing the run.</summary>
    public sealed class ProfileRunBinding : IDisposable
    {
        private readonly RunModel _run;
        private readonly IProfileService _profile;
        private bool _started;
        public Task<bool> SaveTask { get; private set; } = Task.FromResult(true);
        public ProfileRunBinding(RunModel run, IProfileService profile)
        {
            _run = run ?? throw new ArgumentNullException(nameof(run)); _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _started = run.State == RunState.Running || run.State == RunState.Paused;
            if (_started) _profile.SetRunActive(true);
            _run.StateChanged += StateChanged; _run.Completed += Completed;
        }
        private void StateChanged(RunState state)
        { if (!_started && state == RunState.Running) { _profile.SetRunActive(true); _started = true; } }
        private void Completed(RunOutcome outcome)
        { _profile.SetRunActive(false); SaveTask = _profile.ApplyAsync(outcome, _started); }
        public void Dispose() { _run.StateChanged -= StateChanged; _run.Completed -= Completed; }
    }
}
