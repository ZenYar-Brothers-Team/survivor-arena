using UnityEngine;

namespace Game.Automation
{
    /// <summary>Axis-aligned broad-phase bounds of a player-only collider.</summary>
    public readonly struct BotObstacle
    {
        public Rect Bounds { get; }
        public BotObstacle(Rect bounds) { Bounds = bounds; }
    }
}
