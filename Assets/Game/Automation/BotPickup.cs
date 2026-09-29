using UnityEngine;

namespace Game.Automation
{
    /// <summary>Currently visible collectible position and relative priority, never a future drop.</summary>
    public readonly struct BotPickup
    {
        public Vector2 Position { get; }
        public float Priority { get; }

        public BotPickup(Vector2 position, float priority)
        {
            Position = position;
            Priority = priority;
        }
    }
}
