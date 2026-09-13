using UnityEngine;

namespace Game.Movement
{
    public static class MovementVelocityCalculator
    {
        public static Vector2 Calculate(Vector2 rawInput, float speed, bool isRunning)
        {
            if (!isRunning)
                return Vector2.zero;

            return Vector2.ClampMagnitude(rawInput, 1f) * speed;
        }
    }
}
