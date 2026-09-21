using System;
using Game.Content;
namespace Game.Pickup
{
    /// <summary>DECISION-0033. Claims before callbacks; rejection preserves the same drop for a later attempt.</summary>
    public sealed class PickupLife
    {
        public PickupDefinition Definition { get; }
        public PickupIdentity Identity { get; }
        public PickupLifeState State { get; private set; } = PickupLifeState.Available;
        public float Elapsed { get; private set; }
        public PickupLife(PickupDefinition definition, PickupIdentity identity)
        { Definition = definition ?? throw new ArgumentNullException(nameof(definition)); Identity = identity; }
        public bool Tick(float deltaTime, bool running)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            if (!running || State != PickupLifeState.Available || !Definition.LifetimeSeconds.HasValue) return false;
            Elapsed += deltaTime;
            if (Elapsed < Definition.LifetimeSeconds.Value) return false;
            State = PickupLifeState.Expired;
            return true;
        }
        public PickupRewardResult TryCollect(IPickupRewardTarget target)
        {
            if (State != PickupLifeState.Available || !target.CanCollect(Identity)) return default;
            State = PickupLifeState.Claiming;
            try
            {
                var result = target.TryApply(Definition, Identity);
                if (result.Accepted) State = PickupLifeState.Collected;
                else if (State == PickupLifeState.Claiming) State = PickupLifeState.Available;
                return result;
            }
            catch
            {
                // A callback may have applied a reward before throwing. Never retry an uncertain reward.
                State = PickupLifeState.Cancelled;
                throw;
            }
        }
        public void Cancel()
        { if (State == PickupLifeState.Available || State == PickupLifeState.Claiming) State = PickupLifeState.Cancelled; }
    }
}
