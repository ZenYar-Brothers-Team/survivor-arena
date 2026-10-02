using System;
using Game.Content;
using Game.Zones.Json;
using UnityEngine;

namespace Game.Zones
{
    /// <summary>
    /// Validated values of one zone effect. Distances are world units, durations seconds, bonuses fractions (0.4 = +40%).
    /// Each <see cref="ZoneEffectKind"/> requires its own subset and each <see cref="ZoneLifetimeMode"/> its own timing values;
    /// unrelated values must be absent so data cannot silently carry numbers the kind ignores.
    /// </summary>
    public sealed class ZoneEffectDefinition
    {
        /// <summary>A zone's continuous effect works while its visibility is at least this (fading zones are half there).</summary>
        public const float ActivationThreshold = 0.5f;
        /// <summary>A burst flash grows the disc by this fraction while it fades out.</summary>
        public const float BurstFlashGrowth = 0.2f;

        public ContentId Id { get; }
        public string DisplayName { get; }
        public ZoneEffectKind Kind { get; }
        public float Radius { get; }
        /// <summary>Ground projection shared by presentation and containment; omitted authoring preserves a circle.</summary>
        public float VerticalScale { get; }
        public Color Color { get; }
        public ZoneLifetimeMode Lifetime { get; }
        /// <summary>Whether a temporary zone picks a new place when its next preparation starts.</summary>
        public bool RelocatesBetweenCycles { get; }
        /// <summary>Pulsing and Burst: length of one cycle.</summary>
        public float PulsePeriodSeconds { get; }
        /// <summary>Pulsing: how long the zone is shown per cycle, both fades included.</summary>
        public float PulseVisibleSeconds { get; }
        public float PulseFadeSeconds { get; }
        public float PulsePrepareSeconds { get; }
        public float PulseIdleVisibility { get; }
        public bool HasPreparation => PulsePrepareSeconds > 0f;
        /// <summary>Burst: how long the disc swells before it goes off.</summary>
        public float TelegraphSeconds { get; }
        /// <summary>Burst: how long the flash lingers after it went off.</summary>
        public float FlashSeconds { get; }
        /// <summary>Slow: negative; Haste and SpeedBurst: positive. Added to the player's movement-speed bonus.</summary>
        public float PlayerMovementBonus { get; }
        /// <summary>Slow: fraction of speed enemies lose while inside (0 = enemies unaffected).</summary>
        public float EnemySlowFraction { get; }
        /// <summary>Slow: status duration after a tick; zero means strictly area-only with no status or unit presentation.</summary>
        public float EnemySlowSeconds { get; }
        public float PlayerRegenerationPerSecond { get; }
        public float PlayerSkillDamageBonus { get; }
        public float PlayerActionSpeedBonus { get; }
        public float PlayerDamagePerSecond { get; }
        public float EnemyDamagePerSecond { get; }
        /// <summary>Protection: fraction of incoming damage the player is spared while inside (0.8 = 80% less).</summary>
        public float PlayerIncomingDamageReduction { get; }
        /// <summary>SpeedBurst: how long the speed buff lasts after the burst, wherever the player goes.</summary>
        public float PlayerBuffSeconds { get; }
        /// <summary>Charge: seconds inside needed to reach full charge.</summary>
        public float ChargeSecondsToMax { get; }
        /// <summary>Charge: seconds a full charge takes to drain once the player is outside.</summary>
        public float ChargeDecaySeconds { get; }
        /// <summary>Charge: active-skill damage bonus at full charge.</summary>
        public float ChargeSkillDamageBonus { get; }
        /// <summary>Charge: action-speed bonus at full charge.</summary>
        public float ChargeActionSpeedBonus { get; }
        /// <summary>Altars and charging zones stay faintly drawn even while resting so the player can find them.</summary>
        public bool AlwaysShown => IsAltar;
        /// <summary>Whose side the altar is on; set for every altar and checked against the effect's real harm.</summary>
        public ZoneAltarPolarity? Polarity { get; }
        /// <summary>Strike: length of one volley cycle.</summary>
        public float StrikePeriodSeconds { get; }
        public float StrikeTelegraphSeconds { get; }
        public float StrikeFlashSeconds { get; }
        public int StrikeCount { get; }
        public float StrikeRadius { get; }
        public float StrikePlayerDamage { get; }
        public float StrikeEnemyDamage { get; }
        /// <summary>Shrine: seconds inside to fire, seconds outside to drain the progress, cooldown after firing.</summary>
        public float ShrineChargeSeconds { get; }
        public float ShrineDecaySeconds { get; }
        public float ShrineCooldownSeconds { get; }
        public float RewardBuffSeconds { get; }
        public float RewardMovementBonus { get; }
        public float RewardSkillDamageBonus { get; }
        public float RewardActionSpeedBonus { get; }
        public float RewardHealFraction { get; }
        public float RewardBlastDamage { get; }
        public float RewardBlastRadius { get; }
        public float RewardShieldSeconds { get; }
        public float RewardIncomingDamageReduction { get; }
        /// <summary>Cycling zones and the charge, strike and shrine kinds are altars: they need a polarity.</summary>
        public bool IsAltar => Lifetime == ZoneLifetimeMode.Cycling || Kind == ZoneEffectKind.Charge ||
                               Kind == ZoneEffectKind.Strike || Kind == ZoneEffectKind.Shrine;
        public bool HarmsPlayer => (Kind == ZoneEffectKind.Rift && PlayerDamagePerSecond > 0f) ||
                                   (Kind == ZoneEffectKind.Slow && PlayerMovementBonus < 0f) ||
                                   (Kind == ZoneEffectKind.Strike && StrikePlayerDamage > 0f);
        public bool HarmsEnemies => (Kind == ZoneEffectKind.Rift && EnemyDamagePerSecond > 0f) ||
                                    (Kind == ZoneEffectKind.Slow && EnemySlowFraction > 0f) ||
                                    (Kind == ZoneEffectKind.Strike && StrikeEnemyDamage > 0f);
        /// <summary>Timed buff a burst or shrine leaves on the player.</summary>
        public float TimedBuffSeconds => Kind == ZoneEffectKind.Shrine ? RewardBuffSeconds : PlayerBuffSeconds;
        public float TimedBuffMovementBonus => Kind == ZoneEffectKind.Shrine ? RewardMovementBonus : PlayerMovementBonus;
        /// <summary>Range of a random starting phase: the cycle for cycling and pulsing zones, the volley period for a permanent strike altar.</summary>
        public float PhaseRange => Lifetime != ZoneLifetimeMode.Permanent ? PulsePeriodSeconds : Kind == ZoneEffectKind.Strike ? StrikePeriodSeconds : 0f;
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
            VerticalScale = data.VerticalScale ?? 1f;
            NumericValidation.ValidateRange(VerticalScale, 0.1f, 1f, nameof(VerticalScale));
            if (string.IsNullOrWhiteSpace(data.Color) || !ColorUtility.TryParseHtmlString(data.Color, out var color))
                throw new ArgumentException($"Zone effect '{data.Id}' color must be an HTML color.");
            Color = color;
            Lifetime = data.Lifetime ?? throw new ArgumentException($"Zone effect '{data.Id}' requires a lifetime.");
            RelocatesBetweenCycles = data.RelocatesBetweenCycles ??
                (Lifetime == ZoneLifetimeMode.Pulsing || Lifetime == ZoneLifetimeMode.Burst);
            if (RelocatesBetweenCycles && Lifetime != ZoneLifetimeMode.Pulsing && Lifetime != ZoneLifetimeMode.Burst)
                throw new ArgumentException($"Zone effect '{data.Id}': only pulsing or burst zones can relocate.");
            if (data.PulsePrepareSeconds.HasValue || data.PulseIdleVisibility.HasValue)
            {
                if (Lifetime != ZoneLifetimeMode.Pulsing)
                    throw new ArgumentException($"Zone effect '{data.Id}': preparation belongs only to pulsing zones.");
                PulsePrepareSeconds = Required(data.PulsePrepareSeconds, data.Id, "pulsePrepareSeconds");
                PulseIdleVisibility = Required(data.PulseIdleVisibility, data.Id, "pulseIdleVisibility");
                NumericValidation.ValidatePositive(PulsePrepareSeconds, nameof(PulsePrepareSeconds));
                NumericValidation.ValidateRange(PulseIdleVisibility, 0f, 0.25f, nameof(PulseIdleVisibility));
            }

            switch (Lifetime)
            {
                case ZoneLifetimeMode.Pulsing:
                case ZoneLifetimeMode.Cycling:
                    PulsePeriodSeconds = Required(data.PulsePeriodSeconds, data.Id, "pulsePeriodSeconds");
                    PulseVisibleSeconds = Required(data.PulseVisibleSeconds, data.Id, "pulseVisibleSeconds");
                    PulseFadeSeconds = Required(data.PulseFadeSeconds, data.Id, "pulseFadeSeconds");
                    NumericValidation.ValidatePositive(PulsePeriodSeconds, nameof(PulsePeriodSeconds));
                    NumericValidation.ValidatePositive(PulseFadeSeconds, nameof(PulseFadeSeconds));
                    if (PulseVisibleSeconds > PulsePeriodSeconds) throw new ArgumentException($"Zone effect '{data.Id}' is shown longer than its period.");
                    var prepare = HasPreparation ? PulsePrepareSeconds : PulseFadeSeconds;
                    if (PulseVisibleSeconds < prepare + PulseFadeSeconds || (HasPreparation && PulseVisibleSeconds == prepare + PulseFadeSeconds))
                        throw new ArgumentException($"Zone effect '{data.Id}' needs room for preparation, active time and fading.");
                    if (data.TelegraphSeconds.HasValue || data.FlashSeconds.HasValue)
                        throw new ArgumentException($"Pulsing or cycling zone effect '{data.Id}' carries burst values.");
                    break;
                case ZoneLifetimeMode.Burst:
                    PulsePeriodSeconds = Required(data.PulsePeriodSeconds, data.Id, "pulsePeriodSeconds");
                    TelegraphSeconds = Required(data.TelegraphSeconds, data.Id, "telegraphSeconds");
                    FlashSeconds = Required(data.FlashSeconds, data.Id, "flashSeconds");
                    NumericValidation.ValidatePositive(PulsePeriodSeconds, nameof(PulsePeriodSeconds));
                    NumericValidation.ValidatePositive(TelegraphSeconds, nameof(TelegraphSeconds));
                    NumericValidation.ValidatePositive(FlashSeconds, nameof(FlashSeconds));
                    if (TelegraphSeconds + FlashSeconds > PulsePeriodSeconds)
                        throw new ArgumentException($"Burst zone effect '{data.Id}' telegraphs and flashes longer than its period.");
                    if (data.PulseVisibleSeconds.HasValue || data.PulseFadeSeconds.HasValue)
                        throw new ArgumentException($"Burst zone effect '{data.Id}' carries fade values.");
                    break;
                default:
                    if (data.PulsePeriodSeconds.HasValue || data.PulseVisibleSeconds.HasValue || data.PulseFadeSeconds.HasValue ||
                        data.TelegraphSeconds.HasValue || data.FlashSeconds.HasValue)
                        throw new ArgumentException($"Permanent zone effect '{data.Id}' carries cycle values.");
                    break;
            }
            if (Kind == ZoneEffectKind.Portal && (RelocatesBetweenCycles ||
                (Lifetime != ZoneLifetimeMode.Permanent && Lifetime != ZoneLifetimeMode.Pulsing)))
                throw new ArgumentException($"Portal effect '{data.Id}' must stay in place; its pair may pulse together.");
            if ((Kind == ZoneEffectKind.SpeedBurst) != (Lifetime == ZoneLifetimeMode.Burst))
                throw new ArgumentException($"Zone effect '{data.Id}': a burst lifetime belongs to the SpeedBurst kind and only to it.");
            if (Kind == ZoneEffectKind.Charge && Lifetime != ZoneLifetimeMode.Permanent && Lifetime != ZoneLifetimeMode.Cycling)
                throw new ArgumentException($"Charge effect '{data.Id}' stays in place: permanent or cycling.");

            if (Kind != ZoneEffectKind.Strike && (data.StrikePeriodSeconds.HasValue || data.StrikeTelegraphSeconds.HasValue ||
                    data.StrikeFlashSeconds.HasValue || data.StrikeCount.HasValue || data.StrikeRadius.HasValue ||
                    data.StrikePlayerDamage.HasValue || data.StrikeEnemyDamage.HasValue))
                throw new ArgumentException($"Zone effect '{data.Id}' carries strike values but is not a strike altar.");
            if (Kind != ZoneEffectKind.Shrine && (data.ShrineChargeSeconds.HasValue || data.ShrineDecaySeconds.HasValue ||
                    data.ShrineCooldownSeconds.HasValue || HasReward(data)))
                throw new ArgumentException($"Zone effect '{data.Id}' carries shrine values but is not a shrine.");
            if ((Kind == ZoneEffectKind.Strike || Kind == ZoneEffectKind.Shrine) &&
                Lifetime != ZoneLifetimeMode.Permanent && Lifetime != ZoneLifetimeMode.Cycling)
                throw new ArgumentException($"Altar effect '{data.Id}' stays in place: permanent or cycling.");

            switch (Kind)
            {
                case ZoneEffectKind.Strike:
                    StrikePeriodSeconds = Required(data.StrikePeriodSeconds, data.Id, "strikePeriodSeconds");
                    StrikeTelegraphSeconds = Required(data.StrikeTelegraphSeconds, data.Id, "strikeTelegraphSeconds");
                    StrikeFlashSeconds = Required(data.StrikeFlashSeconds, data.Id, "strikeFlashSeconds");
                    StrikeCount = data.StrikeCount ?? throw new ArgumentException($"Zone effect '{data.Id}' requires strikeCount.");
                    StrikeRadius = Required(data.StrikeRadius, data.Id, "strikeRadius");
                    StrikePlayerDamage = Required(data.StrikePlayerDamage, data.Id, "strikePlayerDamage");
                    StrikeEnemyDamage = Required(data.StrikeEnemyDamage, data.Id, "strikeEnemyDamage");
                    NumericValidation.ValidatePositive(StrikePeriodSeconds, nameof(StrikePeriodSeconds));
                    NumericValidation.ValidatePositive(StrikeTelegraphSeconds, nameof(StrikeTelegraphSeconds));
                    NumericValidation.ValidatePositive(StrikeFlashSeconds, nameof(StrikeFlashSeconds));
                    NumericValidation.ValidateCount(StrikeCount, nameof(StrikeCount));
                    NumericValidation.ValidatePositive(StrikeRadius, nameof(StrikeRadius));
                    NumericValidation.ValidateNonNegativeFinite(StrikePlayerDamage, nameof(StrikePlayerDamage));
                    NumericValidation.ValidateNonNegativeFinite(StrikeEnemyDamage, nameof(StrikeEnemyDamage));
                    if (StrikeTelegraphSeconds + StrikeFlashSeconds > StrikePeriodSeconds)
                        throw new ArgumentException($"Strike altar '{data.Id}' warns and flashes longer than its period.");
                    if (StrikeRadius >= Radius) throw new ArgumentException($"Strike altar '{data.Id}': strike circles must be smaller than the altar.");
                    if (StrikePlayerDamage <= 0f && StrikeEnemyDamage <= 0f)
                        throw new ArgumentException($"Strike altar '{data.Id}' must hit the player or the enemies.");
                    Forbid(data, ValueGroup.SlowOnly | ValueGroup.Movement | ValueGroup.Regeneration | ValueGroup.Arcane | ValueGroup.Rift | ValueGroup.Portal | ValueGroup.Protection | ValueGroup.Buff | ValueGroup.Charge);
                    break;
                case ZoneEffectKind.Shrine:
                    ShrineChargeSeconds = Required(data.ShrineChargeSeconds, data.Id, "shrineChargeSeconds");
                    ShrineDecaySeconds = Required(data.ShrineDecaySeconds, data.Id, "shrineDecaySeconds");
                    ShrineCooldownSeconds = Required(data.ShrineCooldownSeconds, data.Id, "shrineCooldownSeconds");
                    NumericValidation.ValidatePositive(ShrineChargeSeconds, nameof(ShrineChargeSeconds));
                    NumericValidation.ValidatePositive(ShrineDecaySeconds, nameof(ShrineDecaySeconds));
                    NumericValidation.ValidatePositive(ShrineCooldownSeconds, nameof(ShrineCooldownSeconds));
                    (RewardBuffSeconds, RewardMovementBonus, RewardSkillDamageBonus, RewardActionSpeedBonus, RewardHealFraction,
                        RewardBlastDamage, RewardBlastRadius, RewardShieldSeconds, RewardIncomingDamageReduction) = ReadRewards(data);
                    Forbid(data, ValueGroup.SlowOnly | ValueGroup.Movement | ValueGroup.Regeneration | ValueGroup.Arcane | ValueGroup.Rift | ValueGroup.Portal | ValueGroup.Protection | ValueGroup.Buff | ValueGroup.Charge);
                    break;
                case ZoneEffectKind.Slow:
                    PlayerMovementBonus = Required(data.PlayerMovementBonus, data.Id, "playerMovementBonus");
                    EnemySlowFraction = Required(data.EnemySlowFraction, data.Id, "enemySlowFraction");
                    EnemySlowSeconds = Required(data.EnemySlowSeconds, data.Id, "enemySlowSeconds");
                    NumericValidation.ValidateRange(PlayerMovementBonus, -0.9f, 0f, nameof(PlayerMovementBonus));
                    NumericValidation.ValidateRange(EnemySlowFraction, 0f, 0.95f, nameof(EnemySlowFraction));
                    NumericValidation.ValidateNonNegativeFinite(EnemySlowSeconds, nameof(EnemySlowSeconds));
                    if (PlayerMovementBonus == 0f && EnemySlowFraction == 0f)
                        throw new ArgumentException($"Slow zone '{data.Id}' must slow the player or the enemies.");
                    Forbid(data, ValueGroup.Regeneration | ValueGroup.Arcane | ValueGroup.Rift | ValueGroup.Portal | ValueGroup.Protection | ValueGroup.Buff | ValueGroup.Charge);
                    break;
                case ZoneEffectKind.Haste:
                    PlayerMovementBonus = Required(data.PlayerMovementBonus, data.Id, "playerMovementBonus");
                    NumericValidation.ValidateRange(PlayerMovementBonus, 0f, 2f, nameof(PlayerMovementBonus));
                    if (PlayerMovementBonus <= 0f) throw new ArgumentException($"Haste zone '{data.Id}' needs a positive playerMovementBonus.");
                    Forbid(data, ValueGroup.SlowOnly | ValueGroup.Regeneration | ValueGroup.Arcane | ValueGroup.Rift | ValueGroup.Portal | ValueGroup.Protection | ValueGroup.Buff | ValueGroup.Charge);
                    break;
                case ZoneEffectKind.Regeneration:
                    PlayerRegenerationPerSecond = Required(data.PlayerRegenerationPerSecond, data.Id, "playerRegenerationPerSecond");
                    NumericValidation.ValidatePositive(PlayerRegenerationPerSecond, nameof(PlayerRegenerationPerSecond));
                    Forbid(data, ValueGroup.SlowOnly | ValueGroup.Movement | ValueGroup.Arcane | ValueGroup.Rift | ValueGroup.Portal | ValueGroup.Protection | ValueGroup.Buff | ValueGroup.Charge);
                    break;
                case ZoneEffectKind.ArcanePower:
                    PlayerSkillDamageBonus = Required(data.PlayerSkillDamageBonus, data.Id, "playerSkillDamageBonus");
                    PlayerActionSpeedBonus = Required(data.PlayerActionSpeedBonus, data.Id, "playerActionSpeedBonus");
                    NumericValidation.ValidateNonNegativeFinite(PlayerSkillDamageBonus, nameof(PlayerSkillDamageBonus));
                    NumericValidation.ValidateNonNegativeFinite(PlayerActionSpeedBonus, nameof(PlayerActionSpeedBonus));
                    if (PlayerSkillDamageBonus <= 0f && PlayerActionSpeedBonus <= 0f)
                        throw new ArgumentException($"Arcane zone '{data.Id}' must raise skill damage or action speed.");
                    Forbid(data, ValueGroup.SlowOnly | ValueGroup.Movement | ValueGroup.Regeneration | ValueGroup.Rift | ValueGroup.Portal | ValueGroup.Protection | ValueGroup.Buff | ValueGroup.Charge);
                    break;
                case ZoneEffectKind.Rift:
                    PlayerDamagePerSecond = Required(data.PlayerDamagePerSecond, data.Id, "playerDamagePerSecond");
                    EnemyDamagePerSecond = Required(data.EnemyDamagePerSecond, data.Id, "enemyDamagePerSecond");
                    NumericValidation.ValidateNonNegativeFinite(PlayerDamagePerSecond, nameof(PlayerDamagePerSecond));
                    NumericValidation.ValidateNonNegativeFinite(EnemyDamagePerSecond, nameof(EnemyDamagePerSecond));
                    if (PlayerDamagePerSecond <= 0f && EnemyDamagePerSecond <= 0f)
                        throw new ArgumentException($"Rift zone '{data.Id}' must damage the player or the enemies.");
                    Forbid(data, ValueGroup.SlowOnly | ValueGroup.Movement | ValueGroup.Regeneration | ValueGroup.Arcane | ValueGroup.Portal | ValueGroup.Protection | ValueGroup.Buff | ValueGroup.Charge);
                    break;
                case ZoneEffectKind.Protection:
                    PlayerIncomingDamageReduction = Required(data.PlayerIncomingDamageReduction, data.Id, "playerIncomingDamageReduction");
                    NumericValidation.ValidateRange(PlayerIncomingDamageReduction, 0f, 0.95f, nameof(PlayerIncomingDamageReduction));
                    if (PlayerIncomingDamageReduction <= 0f) throw new ArgumentException($"Protection zone '{data.Id}' needs a positive playerIncomingDamageReduction.");
                    Forbid(data, ValueGroup.SlowOnly | ValueGroup.Movement | ValueGroup.Regeneration | ValueGroup.Arcane | ValueGroup.Rift | ValueGroup.Portal | ValueGroup.Buff | ValueGroup.Charge);
                    break;
                case ZoneEffectKind.SpeedBurst:
                    PlayerMovementBonus = Required(data.PlayerMovementBonus, data.Id, "playerMovementBonus");
                    PlayerBuffSeconds = Required(data.PlayerBuffSeconds, data.Id, "playerBuffSeconds");
                    NumericValidation.ValidateRange(PlayerMovementBonus, 0f, 2f, nameof(PlayerMovementBonus));
                    if (PlayerMovementBonus <= 0f) throw new ArgumentException($"Speed burst '{data.Id}' needs a positive playerMovementBonus.");
                    NumericValidation.ValidatePositive(PlayerBuffSeconds, nameof(PlayerBuffSeconds));
                    Forbid(data, ValueGroup.SlowOnly | ValueGroup.Regeneration | ValueGroup.Arcane | ValueGroup.Rift | ValueGroup.Portal | ValueGroup.Protection | ValueGroup.Charge);
                    break;
                case ZoneEffectKind.Charge:
                    ChargeSecondsToMax = Required(data.ChargeSecondsToMax, data.Id, "chargeSecondsToMax");
                    ChargeDecaySeconds = Required(data.ChargeDecaySeconds, data.Id, "chargeDecaySeconds");
                    ChargeSkillDamageBonus = Required(data.ChargeSkillDamageBonus, data.Id, "chargeSkillDamageBonus");
                    ChargeActionSpeedBonus = Required(data.ChargeActionSpeedBonus, data.Id, "chargeActionSpeedBonus");
                    NumericValidation.ValidatePositive(ChargeSecondsToMax, nameof(ChargeSecondsToMax));
                    NumericValidation.ValidatePositive(ChargeDecaySeconds, nameof(ChargeDecaySeconds));
                    NumericValidation.ValidateNonNegativeFinite(ChargeSkillDamageBonus, nameof(ChargeSkillDamageBonus));
                    NumericValidation.ValidateNonNegativeFinite(ChargeActionSpeedBonus, nameof(ChargeActionSpeedBonus));
                    if (ChargeSkillDamageBonus <= 0f && ChargeActionSpeedBonus <= 0f)
                        throw new ArgumentException($"Charge zone '{data.Id}' must raise skill damage or action speed at full charge.");
                    Forbid(data, ValueGroup.SlowOnly | ValueGroup.Movement | ValueGroup.Regeneration | ValueGroup.Arcane | ValueGroup.Rift | ValueGroup.Portal | ValueGroup.Protection | ValueGroup.Buff);
                    break;
                default:
                    PortalCooldownSeconds = Required(data.PortalCooldownSeconds, data.Id, "portalCooldownSeconds");
                    PortalExitDistance = Required(data.PortalExitDistance, data.Id, "portalExitDistance");
                    PortalMinPairDistance = Required(data.PortalMinPairDistance, data.Id, "portalMinPairDistance");
                    NumericValidation.ValidatePositive(PortalCooldownSeconds, nameof(PortalCooldownSeconds));
                    NumericValidation.ValidatePositive(PortalExitDistance, nameof(PortalExitDistance));
                    NumericValidation.ValidatePositive(PortalMinPairDistance, nameof(PortalMinPairDistance));
                    Forbid(data, ValueGroup.SlowOnly | ValueGroup.Movement | ValueGroup.Regeneration | ValueGroup.Arcane | ValueGroup.Rift | ValueGroup.Protection | ValueGroup.Buff | ValueGroup.Charge);
                    break;
            }
            Polarity = data.Polarity;
            if (IsAltar && !Polarity.HasValue) throw new ArgumentException($"Altar effect '{data.Id}' requires a polarity.");
            if (Polarity.HasValue)
            {
                var ok = Polarity.Value == ZoneAltarPolarity.Positive ? !HarmsPlayer
                    : Polarity.Value == ZoneAltarPolarity.Negative ? HarmsPlayer
                    : HarmsPlayer && HarmsEnemies;
                if (!ok) throw new ArgumentException($"Zone effect '{data.Id}' polarity {Polarity.Value} does not match what the effect does.");
            }
            if (Kind == ZoneEffectKind.Shrine && Polarity != ZoneAltarPolarity.Positive)
                throw new ArgumentException($"Shrine '{data.Id}' rewards the player, so it must be positive.");
        }

        /// <summary>
        /// Visibility 0..1 at a run time for a zone with the given phase. Permanent: always 1. Pulsing: a trapezoid (fade in,
        /// fully shown, fade out, hidden) repeating every period. Burst: 1 while it swells, fading 1 to 0 during the flash,
        /// then hidden until the next cycle.
        /// </summary>
        public float Visibility(float phaseSeconds, float runSeconds)
        {
            if (Lifetime == ZoneLifetimeMode.Permanent) return 1f;
            var t = CycleTime(phaseSeconds, runSeconds);
            if (Lifetime == ZoneLifetimeMode.Burst)
            {
                if (t < TelegraphSeconds) return 1f;
                return t < TelegraphSeconds + FlashSeconds ? 1f - (t - TelegraphSeconds) / FlashSeconds : 0f;
            }
            // Academy seals (2026-10-02 concept): stay dim at rest, prepare briefly, then work only at full light.
            if (HasPreparation)
            {
                if (t < PulsePrepareSeconds)
                    return Mathf.Lerp(PulseIdleVisibility, 1f, Mathf.SmoothStep(0f, 1f, t / PulsePrepareSeconds));
                if (t < PulseVisibleSeconds - PulseFadeSeconds) return 1f;
                if (t < PulseVisibleSeconds)
                    return Mathf.Lerp(PulseIdleVisibility, 1f, Mathf.SmoothStep(0f, 1f, (PulseVisibleSeconds - t) / PulseFadeSeconds));
                return PulseIdleVisibility;
            }
            if (t < PulseFadeSeconds) return t / PulseFadeSeconds;
            if (t < PulseVisibleSeconds - PulseFadeSeconds) return 1f;
            if (t < PulseVisibleSeconds) return (PulseVisibleSeconds - t) / PulseFadeSeconds;
            return 0f;
        }

        /// <summary>Authoritative activation: prepared zones never work during preparation or decorative fade.</summary>
        public bool IsActive(float phaseSeconds, float runSeconds)
        {
            if (!HasPreparation) return Visibility(phaseSeconds, runSeconds) >= ActivationThreshold;
            var t = CycleTime(phaseSeconds, runSeconds);
            return t >= PulsePrepareSeconds && t < PulseVisibleSeconds - PulseFadeSeconds;
        }

        public bool IsPreparing(float phaseSeconds, float runSeconds) =>
            HasPreparation && CycleTime(phaseSeconds, runSeconds) < PulsePrepareSeconds;

        /// <summary>
        /// Drawn size as a fraction of the radius: 1 for permanent and pulsing zones; a burst swells from 0 to 1 while it
        /// telegraphs, then grows a little more as it flashes away.
        /// </summary>
        public float RadiusScale(float phaseSeconds, float runSeconds)
        {
            if (Lifetime != ZoneLifetimeMode.Burst) return 1f;
            var t = CycleTime(phaseSeconds, runSeconds);
            if (t < TelegraphSeconds) return t / TelegraphSeconds;
            return t < TelegraphSeconds + FlashSeconds ? 1f + BurstFlashGrowth * (t - TelegraphSeconds) / FlashSeconds : 0f;
        }

        /// <summary>True when a burst zone's trigger moment (end of the telegraph) lies in (<paramref name="from"/>, <paramref name="to"/>].</summary>
        public bool BurstFiresBetween(float phaseSeconds, float from, float to)
        {
            if (Lifetime != ZoneLifetimeMode.Burst || to <= from) return false;
            var cycle = Mathf.Floor((to + phaseSeconds - TelegraphSeconds) / PulsePeriodSeconds);
            var fireTime = cycle * PulsePeriodSeconds + TelegraphSeconds - phaseSeconds;
            return fireTime > from && fireTime <= to;
        }

        private float CycleTime(float phaseSeconds, float runSeconds)
        {
            var t = (runSeconds + phaseSeconds) % PulsePeriodSeconds;
            return t < 0f ? t + PulsePeriodSeconds : t;
        }

        private static bool HasReward(ZoneEffectData data) =>
            data.RewardBuffSeconds.HasValue || data.RewardMovementBonus.HasValue || data.RewardSkillDamageBonus.HasValue ||
            data.RewardActionSpeedBonus.HasValue || data.RewardHealFraction.HasValue || data.RewardBlastDamage.HasValue ||
            data.RewardBlastRadius.HasValue || data.RewardShieldSeconds.HasValue || data.RewardIncomingDamageReduction.HasValue;

        // Each reward is a group whose members come together; a shrine needs at least one group.
        private static (float buffSeconds, float movement, float skill, float action, float heal, float blastDamage, float blastRadius,
            float shieldSeconds, float shieldReduction) ReadRewards(ZoneEffectData data)
        {
            var buff = data.RewardBuffSeconds.HasValue || data.RewardMovementBonus.HasValue ||
                       data.RewardSkillDamageBonus.HasValue || data.RewardActionSpeedBonus.HasValue;
            var blast = data.RewardBlastDamage.HasValue || data.RewardBlastRadius.HasValue;
            var shield = data.RewardShieldSeconds.HasValue || data.RewardIncomingDamageReduction.HasValue;
            float buffSeconds = 0f, movement = 0f, skill = 0f, action = 0f, heal = 0f, blastDamage = 0f, blastRadius = 0f, shieldSeconds = 0f, reduction = 0f;
            if (buff)
            {
                buffSeconds = Required(data.RewardBuffSeconds, data.Id, "rewardBuffSeconds");
                movement = data.RewardMovementBonus ?? 0f;
                skill = data.RewardSkillDamageBonus ?? 0f;
                action = data.RewardActionSpeedBonus ?? 0f;
                NumericValidation.ValidatePositive(buffSeconds, nameof(buffSeconds));
                NumericValidation.ValidateRange(movement, 0f, 2f, nameof(movement));
                NumericValidation.ValidateNonNegativeFinite(skill, nameof(skill));
                NumericValidation.ValidateNonNegativeFinite(action, nameof(action));
                if (movement <= 0f && skill <= 0f && action <= 0f)
                    throw new ArgumentException($"Shrine '{data.Id}' buff reward needs a movement, skill-damage or action-speed bonus.");
            }
            if (data.RewardHealFraction.HasValue)
            {
                heal = data.RewardHealFraction.Value;
                NumericValidation.ValidateRange(heal, 0f, 1f, nameof(heal));
                if (heal <= 0f) throw new ArgumentException($"Shrine '{data.Id}' rewardHealFraction must be positive.");
            }
            if (blast)
            {
                blastDamage = Required(data.RewardBlastDamage, data.Id, "rewardBlastDamage");
                blastRadius = Required(data.RewardBlastRadius, data.Id, "rewardBlastRadius");
                NumericValidation.ValidatePositive(blastDamage, nameof(blastDamage));
                NumericValidation.ValidatePositive(blastRadius, nameof(blastRadius));
            }
            if (shield)
            {
                shieldSeconds = Required(data.RewardShieldSeconds, data.Id, "rewardShieldSeconds");
                reduction = Required(data.RewardIncomingDamageReduction, data.Id, "rewardIncomingDamageReduction");
                NumericValidation.ValidatePositive(shieldSeconds, nameof(shieldSeconds));
                NumericValidation.ValidateRange(reduction, 0f, 0.95f, nameof(reduction));
                if (reduction <= 0f) throw new ArgumentException($"Shrine '{data.Id}' shield reward needs a positive reduction.");
            }
            if (!buff && !data.RewardHealFraction.HasValue && !blast && !shield)
                throw new ArgumentException($"Shrine '{data.Id}' needs at least one reward.");
            return (buffSeconds, movement, skill, action, heal, blastDamage, blastRadius, shieldSeconds, reduction);
        }

        /// <summary>
        /// Strike altar timing at a run time: false while the volley is over (hidden); otherwise the warning fill 0..1 and,
        /// after the strike, the flash progress 0..1.
        /// </summary>
        public bool StrikeState(float phaseSeconds, float runSeconds, out float telegraph, out float flash)
        {
            var t = (runSeconds + phaseSeconds) % StrikePeriodSeconds;
            if (t < 0f) t += StrikePeriodSeconds;
            telegraph = 0f; flash = 0f;
            if (t < StrikeTelegraphSeconds) { telegraph = t / StrikeTelegraphSeconds; return true; }
            telegraph = 1f;
            if (t >= StrikeTelegraphSeconds + StrikeFlashSeconds) return false;
            flash = (t - StrikeTelegraphSeconds) / StrikeFlashSeconds;
            return true;
        }

        /// <summary>True when a strike goes off (end of the warning) in (<paramref name="from"/>, <paramref name="to"/>]; <paramref name="volley"/> numbers that volley.</summary>
        public bool StrikeFiresBetween(float phaseSeconds, float from, float to, out int volley)
        {
            volley = 0;
            if (Kind != ZoneEffectKind.Strike || to <= from) return false;
            var cycle = Mathf.Floor((to + phaseSeconds - StrikeTelegraphSeconds) / StrikePeriodSeconds);
            var fireTime = cycle * StrikePeriodSeconds + StrikeTelegraphSeconds - phaseSeconds;
            volley = (int)cycle;
            return fireTime > from && fireTime <= to;
        }

        /// <summary>Where strike circle <paramref name="index"/> of a volley lands: deterministic per run seed, zone and volley, fully inside the altar.</summary>
        public Vector2 StrikeCenter(Vector2 altarCenter, int seed, int zoneIndex, int volley, int index)
        {
            var reach = Radius - StrikeRadius;
            var angle = Unit(seed, zoneIndex, volley, index * 2) * Mathf.PI * 2f;
            var distance = Mathf.Sqrt(Unit(seed, zoneIndex, volley, index * 2 + 1)) * reach;
            return altarCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        }

        // Allocation-free hash to 0..1, so strike circles can be recomputed every frame without a Random instance.
        private static float Unit(int a, int b, int c, int d)
        {
            unchecked
            {
                var h = (uint)a * 2654435761u ^ (uint)b * 40503u ^ (uint)c * 2246822519u ^ (uint)d * 3266489917u;
                h ^= h >> 15; h *= 2246822519u; h ^= h >> 13; h *= 3266489917u; h ^= h >> 16;
                return (h & 0xFFFFFF) / 16777216f;
            }
        }

        /// <summary>Progress 0..1 of a cycling altar's rest until it switches on again; -1 while it is on (or the zone never rests).</summary>
        public float RestProgress(float phaseSeconds, float runSeconds)
        {
            if (Lifetime != ZoneLifetimeMode.Cycling || PulsePeriodSeconds <= PulseVisibleSeconds) return -1f;
            var t = CycleTime(phaseSeconds, runSeconds);
            return t < PulseVisibleSeconds ? -1f : (t - PulseVisibleSeconds) / (PulsePeriodSeconds - PulseVisibleSeconds);
        }

        /// <summary>True when the world point lies inside the projected ground ellipse centered at <paramref name="center"/>.</summary>
        public bool Contains(Vector2 center, Vector2 point)
        {
            var offset = point - center;
            offset.y /= VerticalScale;
            return offset.sqrMagnitude <= Radius * Radius;
        }

        private static float Required(float? value, string id, string name) =>
            value ?? throw new ArgumentException($"Zone effect '{id}' requires {name}.");

        [Flags]
        private enum ValueGroup
        {
            SlowOnly = 1, Movement = 2, Regeneration = 4, Arcane = 8, Rift = 16, Portal = 32, Protection = 64, Buff = 128, Charge = 256
        }

        // A kind's data must not carry another kind's numbers.
        private static void Forbid(ZoneEffectData data, ValueGroup groups)
        {
            var carried = ((groups & ValueGroup.SlowOnly) != 0 && (data.EnemySlowFraction.HasValue || data.EnemySlowSeconds.HasValue)) ||
                          ((groups & ValueGroup.Movement) != 0 && data.PlayerMovementBonus.HasValue) ||
                          ((groups & ValueGroup.Regeneration) != 0 && data.PlayerRegenerationPerSecond.HasValue) ||
                          ((groups & ValueGroup.Arcane) != 0 && (data.PlayerSkillDamageBonus.HasValue || data.PlayerActionSpeedBonus.HasValue)) ||
                          ((groups & ValueGroup.Rift) != 0 && (data.PlayerDamagePerSecond.HasValue || data.EnemyDamagePerSecond.HasValue)) ||
                          ((groups & ValueGroup.Portal) != 0 && (data.PortalCooldownSeconds.HasValue || data.PortalExitDistance.HasValue ||
                                                                 data.PortalMinPairDistance.HasValue)) ||
                          ((groups & ValueGroup.Protection) != 0 && data.PlayerIncomingDamageReduction.HasValue) ||
                          ((groups & ValueGroup.Buff) != 0 && data.PlayerBuffSeconds.HasValue) ||
                          ((groups & ValueGroup.Charge) != 0 && (data.ChargeSecondsToMax.HasValue || data.ChargeDecaySeconds.HasValue ||
                                                                 data.ChargeSkillDamageBonus.HasValue || data.ChargeActionSpeedBonus.HasValue));
            if (carried) throw new ArgumentException($"Zone effect '{data.Id}' carries values that its kind does not use.");
        }
    }
}
