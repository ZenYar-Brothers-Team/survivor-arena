namespace Game.Progression
{
    public sealed class FixtureSetExtraAbility : ISetExtraAbility
    {
        public int RunningTickCount { get; private set; }
        public float RunningSeconds { get; private set; }
        public bool IsDisposed { get; private set; }

        public void Tick(float deltaTime, bool isRunning)
        {
            if (!isRunning || IsDisposed)
                return;
            RunningTickCount++;
            RunningSeconds += deltaTime;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
