using Game.Combat;
namespace Game.Pickup
{
    public readonly struct PickupRewardResult
    {
        public bool Accepted { get; }
        public HealthChange Healing { get; }
        public PickupRewardResult(bool accepted, HealthChange healing = default) { Accepted = accepted; Healing = healing; }
    }
}
