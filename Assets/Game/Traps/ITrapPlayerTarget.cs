using Game.Content;
using UnityEngine;

namespace Game.Traps
{
    /// <summary>What traps need from the player; implemented by a thin adapter over the player runtime.</summary>
    public interface ITrapPlayerTarget
    {
        Vector2 Position { get; }
        bool IsAlive { get; }
        /// <summary>One-off damage that lands immediately; <paramref name="source"/> names the trap type.</summary>
        void Hit(float amount, ContentId source);
    }
}
