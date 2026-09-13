using System;

namespace Game.Enemy
{
    public sealed class ContinuousSpawnTimer
    {
        private readonly float _intervalSeconds;
        private float _elapsedSeconds;

        public ContinuousSpawnTimer(float intervalSeconds)
        {
            if (float.IsNaN(intervalSeconds) || float.IsInfinity(intervalSeconds) || intervalSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds), "Spawn interval must be finite and greater than zero.");

            _intervalSeconds = intervalSeconds;
        }

        public int Tick(float deltaTime, bool isRunning)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta time must be finite and non-negative.");

            if (!isRunning || deltaTime == 0f)
                return 0;

            _elapsedSeconds += deltaTime;
            var spawnCount = (int)Math.Floor(_elapsedSeconds / _intervalSeconds);
            _elapsedSeconds -= spawnCount * _intervalSeconds;
            return spawnCount;
        }
    }
}
