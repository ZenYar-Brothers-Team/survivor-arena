using System;
using Game.Run;

namespace Game.Automation
{
    /// <summary>Keeps a human demonstration at real-time speed even if the development HUD is clicked.</summary>
    public sealed class HumanRecordingSpeedGuard : IDisposable
    {
        private readonly RunModel _run;

        public HumanRecordingSpeedGuard(RunModel run)
        {
            _run = run ?? throw new ArgumentNullException(nameof(run));
            if (_run.SpeedMultiplier != 1f) throw new ArgumentException("Recording must start at 1x speed.", nameof(run));
            _run.SpeedChanged += RestoreSpeed;
        }

        private void RestoreSpeed(float speed)
        {
            if (speed != 1f) _run.SetSpeed(1f);
        }

        public void Dispose() => _run.SpeedChanged -= RestoreSpeed;
    }
}
