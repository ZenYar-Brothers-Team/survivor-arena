using UnityEngine;

namespace Game.Zones
{
    /// <summary>
    /// One zone on the field during a run. Permanent zones keep their center; a pulsing zone is relocated by
    /// <see cref="ZoneRuntime"/> each time a new cycle begins (while it is invisible), so it reappears somewhere else.
    /// </summary>
    public sealed class ZonePlacement
    {
        public int Index { get; }
        public ZoneEffectDefinition Effect { get; }
        public Vector2 Center { get; private set; }
        /// <summary>Index of the paired portal; -1 for every other kind.</summary>
        public int PartnerIndex { get; private set; } = -1;
        /// <summary>Seconds added to the run clock when computing this zone's pulse (0 for permanent zones).</summary>
        public float PhaseSeconds { get; }
        /// <summary>Pulse cycle the current center was chosen for (0 for the first).</summary>
        public int Cycle { get; private set; }
        /// <summary>True while the zone lies within the player's active window; only such zones work, show and relocate.</summary>
        public bool IsNear { get; private set; }

        public void SetNear(bool near) => IsNear = near;

        /// <summary>Charging zones: how full the player's charge of this altar is, 0..1.</summary>
        public float Charge { get; private set; }

        public void SetCharge(float charge) => Charge = Mathf.Clamp01(charge);

        /// <summary>Shrines: seconds left of the cooldown after the shrine fired (0 when ready).</summary>
        public float ShrineCooldownRemaining { get; private set; }

        public void SetShrineCooldown(float seconds) => ShrineCooldownRemaining = Mathf.Max(0f, seconds);

        /// <summary>
        /// Progress 0..1 of the rest until the altar works again (the ring that fills around it); -1 while it works or never rests.
        /// A shrine's cooldown after firing takes precedence over its cycle rest.
        /// </summary>
        public float RestProgress(float runSeconds)
        {
            if (Effect.Kind == ZoneEffectKind.Shrine && ShrineCooldownRemaining > 0f)
                return 1f - ShrineCooldownRemaining / Effect.ShrineCooldownSeconds;
            return Effect.RestProgress(PhaseSeconds, runSeconds);
        }

        public ZonePlacement(int index, ZoneEffectDefinition effect, Vector2 center, float phaseSeconds = 0f)
        {
            if (effect == null) throw new System.ArgumentNullException(nameof(effect));
            Index = index; Effect = effect; Center = center; PhaseSeconds = phaseSeconds;
        }

        public void LinkPartner(int partnerIndex) => PartnerIndex = partnerIndex;

        /// <summary>Moves a pulsing zone to the spot chosen for a new cycle.</summary>
        public void Relocate(Vector2 center, int cycle)
        {
            Center = center;
            Cycle = cycle;
        }

        /// <summary>Pulse cycle number at a run time (always 0 for permanent zones).</summary>
        public int CycleAt(float runSeconds) => Effect.Lifetime == ZoneLifetimeMode.Permanent
            ? 0
            : Mathf.FloorToInt((runSeconds + PhaseSeconds) / Effect.PulsePeriodSeconds);

        /// <summary>Visibility 0..1 at a run time.</summary>
        public float Visibility(float runSeconds) => Effect.Visibility(PhaseSeconds, runSeconds);

        /// <summary>Drawn size as a fraction of the radius (swelling for a burst zone).</summary>
        public float RadiusScale(float runSeconds) => Effect.RadiusScale(PhaseSeconds, runSeconds);

        /// <summary>True when a burst zone goes off in (<paramref name="from"/>, <paramref name="to"/>].</summary>
        public bool BurstFiresBetween(float from, float to) => Effect.BurstFiresBetween(PhaseSeconds, from, to);

        /// <summary>True when the zone is shown enough for its effect to work.</summary>
        public bool IsActive(float runSeconds) => Visibility(runSeconds) >= ZoneEffectDefinition.ActivationThreshold &&
                                                  ShrineCooldownRemaining <= 0f;

        public bool Contains(Vector2 point) => Effect.Contains(Center, point);
    }
}
