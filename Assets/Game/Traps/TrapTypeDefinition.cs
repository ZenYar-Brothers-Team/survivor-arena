using System;
using System.Collections.Generic;
using Game.Content;
using Game.Traps.Json;

namespace Game.Traps
{
    /// <summary>
    /// A projectile turret: an indestructible body that fires volleys while the player is near (DECISION-0156).
    /// Expressed as data: heading rule, optional per-volley heading change, telegraph, cooldown and a list of shots.
    /// </summary>
    public sealed class TrapTypeDefinition
    {
        private const int MaxShotsPerVolley = 64;

        public ContentId Id { get; }
        public string Name { get; }
        public int Tier { get; }
        public int Count { get; }
        /// <summary>Key of the 3D model its placements are drawn with, or null for the placeholder shape.</summary>
        public string ModelKey { get; }
        public float BodyRadius { get; }
        public TrapHeadingMode Heading { get; }
        public IReadOnlyList<float> RotationsDegrees { get; }
        public float HeadingStepDegrees { get; }
        public int HeadingCycleVolleys { get; }
        /// <summary>Degrees per second the head turns while the trap rests (0 = it does not turn).</summary>
        public float SpinDegreesPerSecond { get; }
        /// <summary>Seconds an aimed head stays in its firing pose after a volley before it tracks the player again.</summary>
        public float AimResumeDelaySeconds { get; }
        public float TelegraphSeconds { get; }
        /// <summary>Length of the aim line shown during the telegraph; 0 = no line.</summary>
        public float TelegraphLineLength { get; }
        public float CooldownSeconds { get; }
        public IReadOnlyList<TrapShot> Shots { get; }
        public float LastShotDelaySeconds { get; }

        public TrapTypeDefinition(TrapTypeData data, IReadOnlyDictionary<string, TrapProjectileDefinition> projectiles)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (projectiles == null) throw new ArgumentNullException(nameof(projectiles));
            if (string.IsNullOrWhiteSpace(data.Id)) throw new ArgumentException("Trap type id is required.");
            var id = data.Id;
            Id = id;
            Name = string.IsNullOrWhiteSpace(data.Name) ? throw new ArgumentException($"{id}: name is required.") : data.Name;
            Tier = data.Tier ?? throw new ArgumentException($"{id}: tier is required.");
            Count = data.Count ?? throw new ArgumentException($"{id}: count is required.");
            ModelKey = string.IsNullOrWhiteSpace(data.Model) ? null : data.Model;
            BodyRadius = data.BodyRadius ?? throw new ArgumentException($"{id}: bodyRadius is required.");
            TelegraphSeconds = data.TelegraphSeconds ?? throw new ArgumentException($"{id}: telegraphSeconds is required.");
            CooldownSeconds = data.CooldownSeconds ?? throw new ArgumentException($"{id}: cooldownSeconds is required.");
            TelegraphLineLength = data.TelegraphLineLength ?? 0f;
            HeadingStepDegrees = data.HeadingStepDegrees ?? 0f;
            HeadingCycleVolleys = data.HeadingCycleVolleys ?? 0;
            SpinDegreesPerSecond = data.SpinDegreesPerSecond ?? 0f;
            AimResumeDelaySeconds = data.AimResumeDelaySeconds ?? 0f;
            NumericValidation.ValidateNonNegative(AimResumeDelaySeconds, nameof(AimResumeDelaySeconds));
            NumericValidation.ValidateFinite(SpinDegreesPerSecond, nameof(SpinDegreesPerSecond));
            NumericValidation.ValidateRange(Tier, 1, 3, nameof(Tier));
            NumericValidation.ValidateCount(Count, nameof(Count));
            NumericValidation.ValidatePositive(BodyRadius, nameof(BodyRadius));
            NumericValidation.ValidatePositive(TelegraphSeconds, nameof(TelegraphSeconds));
            NumericValidation.ValidatePositive(CooldownSeconds, nameof(CooldownSeconds));
            NumericValidation.ValidateNonNegative(TelegraphLineLength, nameof(TelegraphLineLength));
            NumericValidation.ValidateFinite(HeadingStepDegrees, nameof(HeadingStepDegrees));
            NumericValidation.ValidateNonNegative(HeadingCycleVolleys, nameof(HeadingCycleVolleys));
            if (!Enum.TryParse<TrapHeadingMode>(data.Heading, true, out var heading) || !Enum.IsDefined(typeof(TrapHeadingMode), heading))
                throw new ArgumentException($"{id}: heading must be 'fixed' or 'aim'.");
            Heading = heading;
            if (SpinDegreesPerSecond != 0f && Heading != TrapHeadingMode.Fixed)
                throw new ArgumentException($"{id}: only a fixed-heading trap can spin its head.");
            var rotations = new List<float>(data.RotationsDegrees ?? Array.Empty<float>());
            if (Heading == TrapHeadingMode.Fixed && rotations.Count == 0)
                throw new ArgumentException($"{id}: a fixed-heading trap needs rotationsDegrees.");
            if (Heading == TrapHeadingMode.Aim && rotations.Count > 0)
                throw new ArgumentException($"{id}: an aimed trap does not use rotationsDegrees.");
            foreach (var rotation in rotations) NumericValidation.ValidateFinite(rotation, "rotationsDegrees");
            RotationsDegrees = rotations.AsReadOnly();

            var shots = new List<TrapShot>();
            foreach (var group in data.Shots ?? throw new ArgumentException($"{id}: shots are required."))
            {
                if (group == null || string.IsNullOrWhiteSpace(group.Projectile))
                    throw new ArgumentException($"{id}: every shot group needs a projectile.");
                if (!projectiles.TryGetValue(group.Projectile, out var projectile))
                    throw new ArgumentException($"{id}: unknown projectile '{group.Projectile}'.");
                var count = group.Count ?? throw new ArgumentException($"{id}: shot count is required.");
                NumericValidation.ValidateCount(count, "shot count");
                var delayStep = group.DelayStep ?? 0f;
                NumericValidation.ValidateNonNegative(delayStep, "delayStep");
                for (var i = 0; i < count; i++)
                    shots.Add(new TrapShot(projectile, (group.AngleStart ?? 0f) + i * (group.AngleStep ?? 0f),
                        (group.LateralStart ?? 0f) + i * (group.LateralStep ?? 0f), i * delayStep));
            }
            if (shots.Count == 0 || shots.Count > MaxShotsPerVolley)
                throw new ArgumentException($"{id}: a volley needs 1..{MaxShotsPerVolley} shots.");
            Shots = shots.AsReadOnly();
            var last = 0f;
            foreach (var shot in shots) last = Math.Max(last, shot.DelaySeconds);
            LastShotDelaySeconds = last;
        }

        /// <summary>Heading change of the given volley (0-based): the step accumulates, restarting every cycle when one is set.</summary>
        public float HeadingOffset(int volleyIndex)
        {
            var steps = HeadingCycleVolleys > 0 ? volleyIndex % HeadingCycleVolleys : volleyIndex;
            return HeadingStepDegrees * steps;
        }
    }
}
