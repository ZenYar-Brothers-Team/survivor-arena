using System;
using Game.Content;
using Game.Traps.Json;

namespace Game.Traps
{
    /// <summary>Barrels look the same; the explosive share blows up when the player gets close (DECISION-0156).</summary>
    public sealed class TrapBarrelDefinition
    {
        public ContentId Id { get; }
        public int Count { get; }
        public float ExplosiveShare { get; }
        public float BodyRadius { get; }
        public float TriggerRadius { get; }
        public float FuseSeconds { get; }
        public float BlastRadius { get; }
        public float Damage { get; }
        public int ExplosiveCount => (int)Math.Round(Count * ExplosiveShare, MidpointRounding.AwayFromZero);

        public TrapBarrelDefinition(TrapBarrelData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(data.Id)) throw new ArgumentException("Barrel id is required.");
            Id = data.Id;
            Count = data.Count ?? throw new ArgumentException("barrels.count is required.");
            ExplosiveShare = data.ExplosiveShare ?? throw new ArgumentException("barrels.explosiveShare is required.");
            BodyRadius = data.BodyRadius ?? throw new ArgumentException("barrels.bodyRadius is required.");
            TriggerRadius = data.TriggerRadius ?? throw new ArgumentException("barrels.triggerRadius is required.");
            FuseSeconds = data.FuseSeconds ?? throw new ArgumentException("barrels.fuseSeconds is required.");
            BlastRadius = data.BlastRadius ?? throw new ArgumentException("barrels.blastRadius is required.");
            Damage = data.Damage ?? throw new ArgumentException("barrels.damage is required.");
            NumericValidation.ValidateNonNegative(Count, nameof(Count));
            NumericValidation.ValidateRange(ExplosiveShare, 0f, 1f, nameof(ExplosiveShare));
            NumericValidation.ValidatePositive(BodyRadius, nameof(BodyRadius));
            NumericValidation.ValidatePositive(TriggerRadius, nameof(TriggerRadius));
            NumericValidation.ValidatePositive(FuseSeconds, nameof(FuseSeconds));
            NumericValidation.ValidatePositive(BlastRadius, nameof(BlastRadius));
            NumericValidation.ValidateNonNegative(Damage, nameof(Damage));
        }
    }
}
