using UnityEngine;

namespace Game.Enemy
{
    /// <summary>Summon markers of one boss call waiting for their telegraph.</summary>
    internal sealed class BossSummonCall
    {
        public BossSummonProfile Profile;
        public Vector2[] Positions;
        public float Remaining;
    }
}
