using Game.Content;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>State of one placed boss zone (one circle, or every safe circle of a safe-circles attack).</summary>
    internal sealed class BossZoneInstance
    {
        public BossZoneProfile Profile;
        public ContentId SourceId;
        public Vector2[] Centers;
        public BossZoneStage Stage;
        public float WaitRemaining;
        public float FillElapsed;
        public float SettleElapsed;
        public float NextBurnAt;
    }
}
