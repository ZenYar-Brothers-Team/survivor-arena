using UnityEngine;

namespace Game.Enemy
{
    /// <summary>An ordinary enemy a boss summon marker releases at <see cref="Position"/> (DECISION-0066, F3).</summary>
    public readonly struct BossSummonSpawn
    {
        public BossSummonProfile Profile { get; }
        public Vector2 Position { get; }

        public BossSummonSpawn(BossSummonProfile profile, Vector2 position)
        {
            Profile = profile;
            Position = position;
        }
    }
}
