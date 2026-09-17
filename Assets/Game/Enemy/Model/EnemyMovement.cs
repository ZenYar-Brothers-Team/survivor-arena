using UnityEngine;

namespace Game.Enemy
{
    public static class EnemyMovement
    {
        public static Vector2 CalculateSeekVelocity(
            Vector2 currentPosition,
            Vector2 targetPosition,
            float movementSpeed,
            bool isSimulating)
        {
            if (!isSimulating || movementSpeed <= 0f)
                return Vector2.zero;

            var offset = targetPosition - currentPosition;
            if (offset.sqrMagnitude <= Mathf.Epsilon)
                return Vector2.zero;

            return offset.normalized * movementSpeed;
        }
    }
}
