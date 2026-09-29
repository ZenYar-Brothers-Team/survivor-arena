using UnityEngine;

namespace Game.Movement.Tests
{
    public sealed class FixedMovementInputSource : IMovementInputSource
    {
        public Vector2 Direction { get; set; }
        public Vector2 ReadDirection() => Direction;
    }
}
