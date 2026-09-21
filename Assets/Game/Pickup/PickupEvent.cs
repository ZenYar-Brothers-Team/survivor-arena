using Game.Combat;
using Game.Content;
using UnityEngine;
namespace Game.Pickup
{
    /// <summary>Immutable observer/telemetry payload; survives pooled reuse.</summary>
    public sealed class PickupEvent
    {
        public PickupIdentity Identity { get; }
        public ContentId ContentId { get; }
        public PickupRewardKind Kind { get; }
        public PickupLifeState State { get; }
        public Vector2 Position { get; }
        public HealthChange Healing { get; }
        public PickupEvent(PickupLife life, Vector2 position, HealthChange healing = default)
        { Identity = life.Identity; ContentId = life.Definition.Id; Kind = life.Definition.Kind; State = life.State; Position = position; Healing = healing; }
    }
}
