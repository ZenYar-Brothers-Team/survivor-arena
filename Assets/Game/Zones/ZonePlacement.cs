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
        public bool IsActive(float runSeconds) => Visibility(runSeconds) >= ZoneEffectDefinition.ActivationThreshold;

        public bool Contains(Vector2 point) => Effect.Contains(Center, point);
    }
}
