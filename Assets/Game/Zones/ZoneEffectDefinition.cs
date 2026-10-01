using System;
using Game.Content;
using Game.Zones.Json;
using UnityEngine;

namespace Game.Zones
{
    /// <summary>
    /// Validated values of one zone effect. Distances are world units, durations seconds, bonuses fractions (0.4 = +40%).
    /// Each <see cref="ZoneEffectKind"/> requires its own subset; unrelated values must be absent so data cannot silently
    /// carry numbers the kind ignores.
    /// </summary>
    public sealed class ZoneEffectDefinition
    {
        public ContentId Id { get; }
        public string DisplayName { get; }
        public ZoneEffectKind Kind { get; }
        public float Radius { get; }
        public Color Color { get; }
        public ZoneLifetimeMode Lifetime { get; }
        /// <summary>Pulsing: length of one appear-and-vanish cycle.</summary>
        public float PulsePeriodSeconds { get; }
        /// <summary>Pulsing: how long the zone is shown per cycle, both fades included.</summary>
        public float PulseVisibleSeconds { get; }
        public float PulseFadeSeconds { get; }
        /// <summary>A zone's effect works while its visibility is at least this (fading zones are half there).</summary>
        public const float ActivationThreshold = 0.5f;
        /// <summary>Slow: negative; Haste: positive. Added to the player's movement-speed bonus while inside.</summary>
        public float PlayerMovementBonus { get; }
        /// <summary>Slow: fraction of speed enemies lose while inside (0 = enemies unaffected).</summary>
        public float EnemySlowFraction { get; }
        /// <summary>Slow: how long a slow applied to an enemy lasts after a tick (kept short so it fades on leaving).</summary>
        public float EnemySlowSeconds { get; }
        public float PlayerRegenerationPerSecond { get; }
        public float PlayerSkillDamageBonus { get; }
        public float PlayerActionSpeedBonus { get; }
        public float PlayerDamagePerSecond { get; }
        public float EnemyDamagePerSecond { get; }
        /// <summary>Portal: time before the player can use any portal again.</summary>
        public float PortalCooldownSeconds { get; }
        /// <summary>Portal: how far outside the partner's rim the player arrives.</summary>
        public float PortalExitDistance { get; }
        /// <summary>Portal: minimum distance between the two portals of a pair.</summary>
        public float PortalMinPairDistance { get; }

        public ZoneEffectDefinition(ZoneEffectData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(data.Id)) throw new ArgumentException("Zone effect id is required.");
            Id = new ContentId(data.Id);
            DisplayName = string.IsNullOrWhiteSpace(data.DisplayName) ? data.Id : data.DisplayName;
            Kind = data.Kind ?? throw new ArgumentException($"Zone effect '{data.Id}' requires a kind.");
            Radius = Required(data.Radius, data.Id, "radius");
            NumericValidation.ValidatePositive(Radius, nameof(Radius));
            if (string.IsNullOrWhiteSpace(data.Color) || !ColorUtility.TryParseHtmlString(data.Color, out var color))
                throw new ArgumentException($"Zone effect '{data.Id}' color must be an HTML color.");
            Color = color;
            Lifetime = data.Lifetime ?? throw new ArgumentException($"Zone effect '{data.Id}' requires a lifetime.");
            if (Lifetime == ZoneLifetimeMode.Pulsing)
            {
                PulsePeriodSeconds = Required(data.PulsePeriodSeconds, data.Id, "pulsePeriodSeconds");
                PulseVisibleSeconds = Required(data.PulseVisibleSeconds, data.Id, "pulseVisibleSeconds");
                PulseFadeSeconds = Required(data.PulseFadeSeconds, data.Id, "pulseFadeSeconds");
                NumericValidation.ValidatePositive(PulsePeriodSeconds, nameof(PulsePeriodSeconds));
                NumericValidation.ValidatePositive(PulseFadeSeconds, nameof(PulseFadeSeconds));
                if (PulseVisibleSeconds > PulsePeriodSeconds) throw new ArgumentException($"Zone effect '{data.Id}' is shown longer than its period.");
                if (PulseVisibleSeconds < 2f * PulseFadeSeconds) throw new ArgumentException($"Zone effect '{data.Id}' must stay shown at least as long as both fades.");
            }
            else if (data.PulsePeriodSeconds.HasValue || data.PulseVisibleSeconds.HasValue || data.PulseFadeSeconds.HasValue)
                throw new ArgumentException($"Permanent zone effect '{data.Id}' carries pulse values.");

            if (Kind == ZoneEffectKind.Portal && Lifetime == ZoneLifetimeMode.Pulsing)
                throw new ArgumentException($"Portal effect '{data.Id}' must be permanent: a pair cannot relocate together.");

            switch (Kind)
            {
                case ZoneEffectKind.Slow:
                    PlayerMovementBonus = Required(data.PlayerMovementBonus, data.Id, "playerMovementBonus");
                    EnemySlowFraction = Required(data.EnemySlowFraction, data.Id, "enemySlowFraction");
                    EnemySlowSeconds = Required(data.EnemySlowSeconds, data.Id, "enemySlowSeconds");
                    NumericValidation.ValidateRange(PlayerMovementBonus, -0.9f, 0f, nameof(PlayerMovementBonus));
                    NumericValidation.ValidateRange(EnemySlowFraction, 0f, 0.95f, nameof(EnemySlowFraction));
                    NumericValidation.ValidatePositive(EnemySlowSeconds, nameof(EnemySlowSeconds));
                    if (PlayerMovementBonus == 0f && EnemySlowFraction == 0f)
                        throw new ArgumentException($"Slow zone '{data.Id}' must slow the player or the enemies.");
                    Forbid(data, ValueGroup.Regeneration | ValueGroup.Arcane | ValueGroup.Rift | ValueGroup.Portal);
                    break;
                case ZoneEffectKind.Haste:
                    PlayerMovementBonus = Required(data.PlayerMovementBonus, data.Id, "playerMovementBonus");
                    NumericValidation.ValidateRange(PlayerMovementBonus, 0f, 2f, nameof(PlayerMovementBonus));
                    if (PlayerMovementBonus <= 0f) throw new ArgumentException($"Haste zone '{data.Id}' needs a positive playerMovementBonus.");
                    Forbid(data, ValueGroup.SlowOnly | ValueGroup.Regeneration | ValueGroup.Arcane | ValueGroup.Rift | ValueGroup.Portal);
                    break;
                case ZoneEffectKind.Regeneration:
                    PlayerRegenerationPerSecond = Required(data.PlayerRegenerationPerSecond, data.Id, "playerRegenerationPerSecond");
                    NumericValidation.ValidatePositive(PlayerRegenerationPerSecond, nameof(PlayerRegenerationPerSecond));
                    Forbid(data, ValueGroup.SlowOnly | ValueGroup.Movement | ValueGroup.Arcane | ValueGroup.Rift | ValueGroup.Portal);
                    break;
                case ZoneEffectKind.ArcanePower:
                    PlayerSkillDamageBonus = Required(data.PlayerSkillDamageBonus, data.Id, "playerSkillDamageBonus");
                    PlayerActionSpeedBonus = Required(data.PlayerActionSpeedBonus, data.Id, "playerActionSpeedBonus");
                    NumericValidation.ValidateNonNegativeFinite(PlayerSkillDamageBonus, nameof(PlayerSkillDamageBonus));
                    NumericValidation.ValidateNonNegativeFinite(PlayerActionSpeedBonus, nameof(PlayerActionSpeedBonus));
                    if (PlayerSkillDamageBonus <= 0f && PlayerActionSpeedBonus <= 0f)
                        throw new ArgumentException($"Arcane zone '{data.Id}' must raise skill damage or action speed.");
                    Forbid(data, ValueGroup.SlowOnly | ValueGroup.Movement | ValueGroup.Regeneration | ValueGroup.Rift | ValueGroup.Portal);
                    break;
                case ZoneEffectKind.Rift:
                    PlayerDamagePerSecond = Required(data.PlayerDamagePerSecond, data.Id, "playerDamagePerSecond");
                    EnemyDamagePerSecond = Required(data.EnemyDamagePerSecond, data.Id, "enemyDamagePerSecond");
                    NumericValidation.ValidateNonNegativeFinite(PlayerDamagePerSecond, nameof(PlayerDamagePerSecond));
                    NumericValidation.ValidateNonNegativeFinite(EnemyDamagePerSecond, nameof(EnemyDamagePerSecond));
                    if (PlayerDamagePerSecond <= 0f && EnemyDamagePerSecond <= 0f)
                        throw new ArgumentException($"Rift zone '{data.Id}' must damage the player or the enemies.");
                    Forbid(data, ValueGroup.SlowOnly | ValueGroup.Movement | ValueGroup.Regeneration | ValueGroup.Arcane | ValueGroup.Portal);
                    break;
                default:
                    PortalCooldownSeconds = Required(data.PortalCooldownSeconds, data.Id, "portalCooldownSeconds");
                    PortalExitDistance = Required(data.PortalExitDistance, data.Id, "portalExitDistance");
                    PortalMinPairDistance = Required(data.PortalMinPairDistance, data.Id, "portalMinPairDistance");
                    NumericValidation.ValidatePositive(PortalCooldownSeconds, nameof(PortalCooldownSeconds));
                    NumericValidation.ValidatePositive(PortalExitDistance, nameof(PortalExitDistance));
                    NumericValidation.ValidatePositive(PortalMinPairDistance, nameof(PortalMinPairDistance));
                    Forbid(data, ValueGroup.SlowOnly | ValueGroup.Movement | ValueGroup.Regeneration | ValueGroup.Arcane | ValueGroup.Rift);
                    break;
            }
        }

        /// <summary>
        /// Visibility 0..1 at a run time for a zone with the given phase: 1 for permanent zones, otherwise a trapezoid
        /// (fade in, fully shown, fade out, hidden) repeating every <see cref="PulsePeriodSeconds"/>.
        /// </summary>
        public float Visibility(float phaseSeconds, float runSeconds)
        {
            if (Lifetime == ZoneLifetimeMode.Permanent) return 1f;
            var t = (runSeconds + phaseSeconds) % PulsePeriodSeconds;
            if (t < 0f) t += PulsePeriodSeconds;
            if (t < PulseFadeSeconds) return t / PulseFadeSeconds;
            if (t < PulseVisibleSeconds - PulseFadeSeconds) return 1f;
            if (t < PulseVisibleSeconds) return (PulseVisibleSeconds - t) / PulseFadeSeconds;
            return 0f;
        }

        /// <summary>True when the world point lies inside the zone disc centered at <paramref name="center"/>.</summary>
        public bool Contains(Vector2 center, Vector2 point) => (point - center).sqrMagnitude <= Radius * Radius;

        private static float Required(float? value, string id, string name) =>
            value ?? throw new ArgumentException($"Zone effect '{id}' requires {name}.");

        [Flags]
        private enum ValueGroup { SlowOnly = 1, Movement = 2, Regeneration = 4, Arcane = 8, Rift = 16, Portal = 32 }

        // A kind's data must not carry another kind's numbers.
        private static void Forbid(ZoneEffectData data, ValueGroup groups)
        {
            var carried = ((groups & ValueGroup.SlowOnly) != 0 && (data.EnemySlowFraction.HasValue || data.EnemySlowSeconds.HasValue)) ||
                          ((groups & ValueGroup.Movement) != 0 && data.PlayerMovementBonus.HasValue) ||
                          ((groups & ValueGroup.Regeneration) != 0 && data.PlayerRegenerationPerSecond.HasValue) ||
                          ((groups & ValueGroup.Arcane) != 0 && (data.PlayerSkillDamageBonus.HasValue || data.PlayerActionSpeedBonus.HasValue)) ||
                          ((groups & ValueGroup.Rift) != 0 && (data.PlayerDamagePerSecond.HasValue || data.EnemyDamagePerSecond.HasValue)) ||
                          ((groups & ValueGroup.Portal) != 0 && (data.PortalCooldownSeconds.HasValue || data.PortalExitDistance.HasValue ||
                                                                 data.PortalMinPairDistance.HasValue));
            if (carried) throw new ArgumentException($"Zone effect '{data.Id}' carries values that its kind does not use.");
        }
    }
}
