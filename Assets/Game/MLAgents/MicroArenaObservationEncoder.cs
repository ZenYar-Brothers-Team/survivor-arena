using UnityEngine;

namespace Game.MLAgents
{
    /// <summary>
    /// Pure observation encoding for MicroArena. Deliberately smaller than the 32-float encoder the research
    /// snapshot shared with the full FIELD-001 integration: this rebuild only targets the self-contained
    /// seek/avoid task, so health/XP-progress/multi-threat slots that have no meaning here are left out rather
    /// than padded with zeros. See docs/research/2026-10-01-ml-agents-microarena-setup.md for the scope note.
    /// </summary>
    public static class MicroArenaObservationEncoder
    {
        public const int ObservationCount = 8;

        public static void Encode(
            float[] destination,
            Vector2 playerPosition,
            Vector2 xpPosition,
            Vector2 threatPosition,
            Vector2 threatVelocity,
            MicroArenaConfig config)
        {
            if (destination == null)
                throw new System.ArgumentNullException(nameof(destination));
            if (destination.Length != ObservationCount)
                throw new System.ArgumentException($"Destination must have length {ObservationCount}.", nameof(destination));
            if (config == null)
                throw new System.ArgumentNullException(nameof(config));

            var halfSize = config.ArenaHalfSize;
            var speed = config.PlayerSpeed;

            destination[0] = playerPosition.x / halfSize;
            destination[1] = playerPosition.y / halfSize;
            destination[2] = (xpPosition.x - playerPosition.x) / halfSize;
            destination[3] = (xpPosition.y - playerPosition.y) / halfSize;
            destination[4] = (threatPosition.x - playerPosition.x) / halfSize;
            destination[5] = (threatPosition.y - playerPosition.y) / halfSize;
            destination[6] = threatVelocity.x / speed;
            destination[7] = threatVelocity.y / speed;
        }
    }
}
