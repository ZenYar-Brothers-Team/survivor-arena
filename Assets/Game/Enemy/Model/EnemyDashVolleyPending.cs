using UnityEngine;

namespace Game.Enemy
{
    /// <summary>A dash-end entry waiting for its delay (<see cref="EnemyDashVolleyController"/>).</summary>
    internal struct EnemyDashVolleyPending
    {
        public float Remaining;
        public EnemyDashVolleyEntry Entry;
        public Vector2 Direction;
        public float RotationDegrees;
    }
}
