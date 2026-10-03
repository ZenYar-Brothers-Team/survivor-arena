using System;
using Game.Content;
using Game.Traps.Json;

namespace Game.Traps
{
    /// <summary>One projectile kind a trap can fire. Distances are world units, speed is units per second.</summary>
    public sealed class TrapProjectileDefinition
    {
        public string Id { get; }
        public string Visual { get; }
        public float Radius { get; }
        public float Speed { get; }
        public float Damage { get; }
        public float SpinDegreesPerSecond { get; }
        /// <summary>Seconds after which a boomerang flies back to its trap; 0 = it never returns.</summary>
        public float ReturnAfterSeconds { get; }
        /// <summary>Range after which the projectile ends; 0 = limited only by the scene cutoff.</summary>
        public float MaxDistance { get; }

        public TrapProjectileDefinition(TrapProjectileData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(data.Id)) throw new ArgumentException("Trap projectile id is required.");
            if (string.IsNullOrWhiteSpace(data.Visual)) throw new ArgumentException($"Trap projectile '{data.Id}' needs a visual key.");
            Id = data.Id;
            Visual = data.Visual;
            Radius = data.Radius ?? throw new ArgumentException($"{Id}: radius is required.");
            Speed = data.Speed ?? throw new ArgumentException($"{Id}: speed is required.");
            Damage = data.Damage ?? throw new ArgumentException($"{Id}: damage is required.");
            SpinDegreesPerSecond = data.SpinDegreesPerSecond ?? throw new ArgumentException($"{Id}: spinDegreesPerSecond is required.");
            ReturnAfterSeconds = data.ReturnAfterSeconds ?? 0f;
            MaxDistance = data.MaxDistance ?? 0f;
            NumericValidation.ValidatePositive(Radius, nameof(Radius));
            NumericValidation.ValidatePositive(Speed, nameof(Speed));
            NumericValidation.ValidateNonNegative(Damage, nameof(Damage));
            NumericValidation.ValidateFinite(SpinDegreesPerSecond, nameof(SpinDegreesPerSecond));
            NumericValidation.ValidateNonNegative(ReturnAfterSeconds, nameof(ReturnAfterSeconds));
            NumericValidation.ValidateNonNegative(MaxDistance, nameof(MaxDistance));
        }
    }
}
