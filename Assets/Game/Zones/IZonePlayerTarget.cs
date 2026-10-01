using Game.Character;
using Game.Content;
using UnityEngine;

namespace Game.Zones
{
    /// <summary>What zones need from the player; implemented by a thin adapter over the player runtime.</summary>
    public interface IZonePlayerTarget
    {
        Vector2 Position { get; }
        bool IsAlive { get; }
        /// <summary>Sets (replaces) the stat modifier held under <paramref name="key"/>.</summary>
        void SetStatModifier(string key, CharacterStatModifier modifier);
        void RemoveStatModifier(string key);
        /// <summary>Damage over time from a zone effect (already scaled by the frame time).</summary>
        void Damage(float amount, ContentId source);
        /// <summary>One-off damage that lands immediately (a strike hit), unlike the throttled <see cref="Damage"/>.</summary>
        void Hit(float amount, ContentId source);
        /// <summary>Heals the player by a fraction of their maximum health.</summary>
        void HealFraction(float fractionOfMaxHealth);
        /// <summary>Moves the player instantly; velocity and knockback are the adapter's concern.</summary>
        void TeleportTo(Vector2 position);
    }
}
