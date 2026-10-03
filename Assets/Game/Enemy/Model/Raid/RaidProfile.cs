using System;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>Roundup trigger, timing and shared formation settings (DECISION-0155): a crowd inside one screen width of
    /// the player starts the first roundup at once, then runs a random 60-120 s countdown that restarts only after a roundup ends.</summary>
    public sealed class RaidProfile
    {
        public string Id { get; }
        /// <summary>Crowd radius around the player, in camera widths.</summary>
        public float TriggerRadiusScreenWidths { get; }
        /// <summary>The crowd exists while AT LEAST this many ordinary enemies are inside the radius.</summary>
        public int TriggerEnemyCount { get; }
        public float CountCheckSeconds { get; }
        public float MinIntervalSeconds { get; }
        public float MaxIntervalSeconds { get; }
        /// <summary>Enemies this close to the player at launch never join; a participant that gets this close leaves the roundup.</summary>
        public float InnerExemptRadius { get; }
        /// <summary>Minimum distance between neighbouring slots; a full row spills into the next one.</summary>
        public float SlotSpacing { get; }
        public float RetargetSeconds { get; }
        public int MaxParticipants { get; }
        /// <summary>Radius of the fixed random offset every participant adds to its slot, so formation edges stay fuzzy.</summary>
        public float SlotJitter { get; }
        /// <summary>Extra movement speed, as a fraction of the base speed, while a participant heads for its formation slot.</summary>
        public float FormationSpeedBonus { get; }
        /// <summary>Within this distance of its slot a participant eases to a stop; farther away it keeps full speed.</summary>
        public float ArrivalSlowDistance { get; }
        public RaidRingProfile Ring { get; }
        public RaidWallProfile Wall { get; }
        public RaidContractionProfile Contraction { get; }
        public RaidPincerProfile Pincer { get; }

        public RaidProfile(string id, float triggerRadiusScreenWidths, int triggerEnemyCount, float countCheckSeconds,
            float minIntervalSeconds, float maxIntervalSeconds, float innerExemptRadius, float slotSpacing,
            float retargetSeconds, int maxParticipants, float slotJitter, float formationSpeedBonus, float arrivalSlowDistance, RaidRingProfile ring, RaidWallProfile wall,
            RaidContractionProfile contraction, RaidPincerProfile pincer)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Raid profile id is required.", nameof(id));
            NumericValidation.ValidatePositive(triggerRadiusScreenWidths, nameof(triggerRadiusScreenWidths));
            NumericValidation.ValidateCount(triggerEnemyCount, nameof(triggerEnemyCount));
            NumericValidation.ValidatePositive(countCheckSeconds, nameof(countCheckSeconds));
            NumericValidation.ValidatePositive(minIntervalSeconds, nameof(minIntervalSeconds));
            NumericValidation.ValidatePositive(maxIntervalSeconds, nameof(maxIntervalSeconds));
            if (maxIntervalSeconds < minIntervalSeconds)
                throw new ArgumentOutOfRangeException(nameof(maxIntervalSeconds), "Maximum interval cannot be shorter than the minimum.");
            NumericValidation.ValidateNonNegative(innerExemptRadius, nameof(innerExemptRadius));
            NumericValidation.ValidatePositive(slotSpacing, nameof(slotSpacing));
            NumericValidation.ValidatePositive(retargetSeconds, nameof(retargetSeconds));
            NumericValidation.ValidateCount(maxParticipants, nameof(maxParticipants));
            NumericValidation.ValidateNonNegative(slotJitter, nameof(slotJitter));
            NumericValidation.ValidateNonNegative(formationSpeedBonus, nameof(formationSpeedBonus));
            NumericValidation.ValidatePositive(arrivalSlowDistance, nameof(arrivalSlowDistance));
            Id = id;
            TriggerRadiusScreenWidths = triggerRadiusScreenWidths;
            TriggerEnemyCount = triggerEnemyCount;
            CountCheckSeconds = countCheckSeconds;
            MinIntervalSeconds = minIntervalSeconds;
            MaxIntervalSeconds = maxIntervalSeconds;
            InnerExemptRadius = innerExemptRadius;
            SlotSpacing = slotSpacing;
            RetargetSeconds = retargetSeconds;
            MaxParticipants = maxParticipants;
            SlotJitter = slotJitter;
            FormationSpeedBonus = formationSpeedBonus;
            ArrivalSlowDistance = arrivalSlowDistance;
            Ring = ring ?? throw new ArgumentNullException(nameof(ring));
            Wall = wall ?? throw new ArgumentNullException(nameof(wall));
            Contraction = contraction ?? throw new ArgumentNullException(nameof(contraction));
            Pincer = pincer ?? throw new ArgumentNullException(nameof(pincer));
        }

        public float DurationOf(RaidTemplateKind kind) => kind switch
        {
            RaidTemplateKind.Ring => Ring.DurationSeconds,
            RaidTemplateKind.Wall => Wall.DurationSeconds,
            RaidTemplateKind.Contraction => Contraction.DurationSeconds,
            RaidTemplateKind.Pincer => Pincer.DurationSeconds,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        /// <summary>Seconds after launch during which participants still move as usual (the wall samples the heading of the player).</summary>
        public float SampleSecondsOf(RaidTemplateKind kind) => kind == RaidTemplateKind.Wall ? Wall.SampleSeconds : 0f;
    }
}
