using UnityEngine;

namespace Game.Zones
{
    /// <summary>
    /// One zone on the field during a run. Permanent zones keep their center; a pulsing zone is relocated by
    /// <see cref="ZoneRuntime"/> each time a new cycle begins (while inactive), so it reappears somewhere else.
    /// </summary>
    public sealed class ZonePlacement
    {
        public int Index { get; }
        public ZoneEffectDefinition Effect { get; }
        public Vector2 Center { get; private set; }
        /// <summary>Actual radius of this occurrence, shared by presentation, containment and clearance.</summary>
        public float Radius { get; private set; }
        public bool IsScheduled { get; private set; }
        public bool IsPresent => !IsScheduled || _occurrenceStart.HasValue;
        private readonly float _initialPhase;
        private float? _occurrenceStart;
        public float? OccurrenceEndSeconds => _occurrenceStart.HasValue ? _occurrenceStart.Value +
            (Effect.Lifetime == ZoneLifetimeMode.Burst ? Effect.TelegraphSeconds + Effect.FlashSeconds : Effect.PulseVisibleSeconds) : null;
        /// <summary>Index of the paired portal; -1 for every other kind.</summary>
        public int PartnerIndex { get; private set; } = -1;
        /// <summary>Seconds added to the run clock when computing this zone's pulse (0 for permanent zones).</summary>
        public float PhaseSeconds => _occurrenceStart.HasValue ? -_occurrenceStart.Value : _initialPhase;
        /// <summary>Pulse cycle the current center was chosen for (0 for the first).</summary>
        public int Cycle { get; private set; }
        /// <summary>True while the zone lies within the player's active window; only such zones work, show and relocate.</summary>
        public bool IsNear { get; private set; }

        /// <summary>Authoritative time of the last one-shot application; null before the first use (DECISION-0142).</summary>
        public float? LastApplicationSeconds { get; private set; }

        public void RecordApplication(float runSeconds) => LastApplicationSeconds = runSeconds;

        public void SetNear(bool near) => IsNear = near && IsPresent;

        public void ManageOccurrences() { IsScheduled = true; EndOccurrence(); }
        public void BeginOccurrence(Vector2 center, float radius, float runSeconds)
        {
            Game.Content.NumericValidation.ValidateRange(radius, Effect.MinRadius, Effect.Radius, nameof(radius));
            Center = center; Radius = radius; _occurrenceStart = runSeconds; Cycle++;
            LastApplicationSeconds = null;
        }
        public void EndOccurrence() { _occurrenceStart = null; IsNear = false; }

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

        public ZonePlacement(int index, ZoneEffectDefinition effect, Vector2 center, float phaseSeconds = 0f, float? radius = null)
        {
            if (effect == null) throw new System.ArgumentNullException(nameof(effect));
            Index = index; Effect = effect; Center = center; _initialPhase = phaseSeconds;
            Radius = radius ?? effect.Radius;
            Game.Content.NumericValidation.ValidateRange(Radius, effect.MinRadius, effect.Radius, nameof(radius));
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
        public float Visibility(float runSeconds) => ExistsAt(runSeconds) ? Effect.Visibility(PhaseSeconds, runSeconds) : 0f;

        /// <summary>Drawn size as a fraction of the radius (swelling for a burst zone).</summary>
        public float RadiusScale(float runSeconds) => ExistsAt(runSeconds) ? Effect.RadiusScale(PhaseSeconds, runSeconds) : 0f;

        /// <summary>True when a burst zone goes off in (<paramref name="from"/>, <paramref name="to"/>].</summary>
        public bool BurstFiresBetween(float from, float to) => IsScheduled
            ? _occurrenceStart.HasValue && Effect.Lifetime == ZoneLifetimeMode.Burst &&
              from < _occurrenceStart.Value + Effect.TelegraphSeconds && to >= _occurrenceStart.Value + Effect.TelegraphSeconds
            : Effect.BurstFiresBetween(PhaseSeconds, from, to);

        /// <summary>True in the authoritative active phase and outside shrine cooldown.</summary>
        public bool IsActive(float runSeconds) => ExistsAt(runSeconds) && Effect.IsActive(PhaseSeconds, runSeconds) &&
                                                  ShrineCooldownRemaining <= 0f;

        private bool ExistsAt(float runSeconds) => !IsScheduled ||
            (_occurrenceStart.HasValue && runSeconds >= _occurrenceStart.Value && runSeconds < OccurrenceEndSeconds.Value);

        public bool Contains(Vector2 point) => Effect.Contains(Center, Center + (point - Center) * (Effect.Radius / Radius));
    }
}
