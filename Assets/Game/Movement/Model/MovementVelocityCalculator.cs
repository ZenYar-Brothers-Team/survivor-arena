using UnityEngine;

namespace Game.Movement
{
    public static class MovementVelocityCalculator
    {
        public static Vector2 DirectionFromPointer(Vector2 playerPosition, Vector2 pointerPosition, float deadzoneWorldUnits)
        {
            if (deadzoneWorldUnits < 0f)
                throw new System.ArgumentOutOfRangeException(nameof(deadzoneWorldUnits));

            var offset = pointerPosition - playerPosition;
            return offset.sqrMagnitude <= deadzoneWorldUnits * deadzoneWorldUnits ? Vector2.zero : offset.normalized;
        }

        public static Vector2 Calculate(Vector2 rawInput, float speed, bool isRunning)
        {
            if (!isRunning)
                return Vector2.zero;

            return Vector2.ClampMagnitude(rawInput, 1f) * speed;
        }
    }
}
