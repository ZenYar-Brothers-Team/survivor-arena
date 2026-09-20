using UnityEngine;

namespace Game.Movement
{
    public interface IAdditionalMovementSource
    {
        Vector2 TickAdditionalMovement(float deltaTime, bool isRunning);
    }
}
