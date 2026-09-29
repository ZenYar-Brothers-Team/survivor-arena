using UnityEngine;

namespace Game.Movement
{
    /// <summary>Optional direction source evaluated by PlayerMover before its normal speed/knockback/physics path.</summary>
    public interface IMovementInputSource
    {
        Vector2 ReadDirection();
    }
}
