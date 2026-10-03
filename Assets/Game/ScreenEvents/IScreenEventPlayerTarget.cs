using Game.Content;
using UnityEngine;

namespace Game.ScreenEvents
{
    /// <summary>What screen events need from the player; implemented by a thin adapter over the player runtime.</summary>
    public interface IScreenEventPlayerTarget
    {
        Vector2 Position { get; }
        bool IsAlive { get; }
        /// <summary>One-off damage that lands immediately: <paramref name="fractionOfMaxHealth"/> of the player's maximum health.</summary>
        void HitFraction(float fractionOfMaxHealth, ContentId source);
    }
}
