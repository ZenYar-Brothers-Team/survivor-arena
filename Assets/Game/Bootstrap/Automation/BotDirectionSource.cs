using Game.Movement;
using UnityEngine;

namespace Game.Bootstrap.Automation
{
    /// <summary>Development-only input bridge. Clearing it returns zero intent; PlayerMover owns actual motion.</summary>
    public sealed class BotDirectionSource : IMovementInputSource
    {
        public Vector2 Direction { get; private set; }
        public void SetDirection(Vector2 direction) => Direction = direction.sqrMagnitude > 1f ? direction.normalized : direction;
        public void Clear() => Direction = Vector2.zero;
        public Vector2 ReadDirection() => Direction;
    }
}
