using Game.Content;
using UnityEngine;

namespace Game.Zones
{
    /// <summary>
    /// Allocation-free view of the living enemies for one zone tick: <see cref="Refresh"/> snapshots them, then the
    /// indexed members address that snapshot.
    /// </summary>
    public interface IZoneEnemySource
    {
        /// <summary>Snapshots the living enemies and returns how many there are.</summary>
        int Refresh();
        Vector2 Position(int index);
        /// <summary>Slows by fraction for seconds; zero seconds means area-only, cleared by Refresh/teardown without a status.</summary>
        void Slow(int index, float fraction, float seconds, ContentId source);
        void Damage(int index, float amount, ContentId source);
        /// <summary>One-off damage that lands immediately (a strike or blast), unlike the throttled <see cref="Damage"/>.</summary>
        void Strike(int index, float amount, ContentId source);
        /// <summary>Combined area-only modifiers; zeroes clear them. Regeneration applies for this tick only.</summary>
        void SetArea(int index, float movementBonus, float actionBonus, float regeneration, float defense, float deltaTime);
        void SpeedBurst(int index, float bonus, float seconds);
        bool Teleport(int index, Vector2 destination, float cooldownSeconds, float runSeconds);
    }
}
