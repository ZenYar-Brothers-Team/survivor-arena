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
            if (_run.SpeedMultiplier != 1) throw new ArgumentException("Recording must start at 1x speed.", nameof(run));
            _run.SpeedChanged += RestoreSpeed;
        }

        private void RestoreSpeed(int speed)
        {
            if (speed != 1) _run.SetSpeed(1);
        }

        public void Dispose() => _run.SpeedChanged -= RestoreSpeed;
    }
}
