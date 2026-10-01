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
        /// <summary>Slows an enemy by <paramref name="fraction"/> for <paramref name="seconds"/> (re-applied while it stays in the zone).</summary>
        void Slow(int index, float fraction, float seconds, ContentId source);
        void Damage(int index, float amount, ContentId source);
    }
}
