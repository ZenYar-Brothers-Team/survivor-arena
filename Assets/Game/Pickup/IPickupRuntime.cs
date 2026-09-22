using System;
namespace Game.Pickup
{
    public interface IPickupRuntime
    {
        event Action Changed;
        event Action<PickupEvent> Resolved;
        PickupSnapshot Snapshot { get; }
        void DropDevelopmentPickup(PickupRewardKind kind);
    }
}
