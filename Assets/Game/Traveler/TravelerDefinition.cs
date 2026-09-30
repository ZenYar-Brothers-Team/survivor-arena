using System;
using System.Collections.Generic;
using Game.Content;
using Game.Enemy;
using Game.Traveler.Json;
namespace Game.Traveler
{
    public sealed class TravelerDefinition : IContentDefinition, IReferencesContent
    {
        public ContentId Id { get; }
        public string Name { get; }
        public string Marker { get; }
        public UnityEngine.Color Color { get; }
        public TravelerRole Role { get; }
        public EnemyDefinition Body { get; }
        public float PresenceSeconds { get; }
        public float WanderSeconds { get; }
        public float RestSeconds { get; }
        public float AvoidRadius { get; }
        public float AvoidSeconds { get; }
        public float GuardOffset { get; }
        public TravelerSupportKind Support { get; }
        public float SupportRadius { get; }
        public float Reduction { get; }
        public float Resistance { get; }
        public float ShieldHp { get; }
        public float ShieldSeconds { get; }
        public float SupportCooldown { get; }
        public int SupportTargets { get; }
        public TravelerMovementStyle MovementStyle { get; }
        public float ZigzagAngleDegrees { get; }
        public float ZigzagSeconds { get; }
        public float EscapeDashDistance { get; }
        public float EscapeDashSeconds { get; }
        public float EscapeDashCooldownSeconds { get; }
        public float OrbitRadiusX { get; }
        public float OrbitRadiusY { get; }
        public float OrbitLeadDegrees { get; }
        public float TeleportMinSeconds { get; }
        public float TeleportMaxSeconds { get; }
        /// <summary>SpeedBurst: radius around the picked target; Heal: unused (the wave uses SupportRadius).</summary>
        public float EffectRadius { get; }
        public float SpeedBonus { get; }
        public float EffectSeconds { get; }
        public float HealAmount { get; }
        /// <summary>Vertical/horizontal ratio of every support zone and its drawing: the 3/4 camera sees a flattened ground circle (DECISION-0058).</summary>
        public float SupportVerticalScale { get; }
        public UnityEngine.Color EffectColor { get; }
        public TravelerEffectShape EffectShape { get; }
        public TravelerDefinition(TravelerData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.Name) || string.IsNullOrWhiteSpace(data.Marker)) throw new ArgumentException("Traveler name/marker required.");
            Id = new ContentId(data.Id); Name = data.Name; Marker = data.Marker;
            if (data.Color == null || data.Color.Length != 4) throw new ArgumentException("Traveler color requires RGBA.");
            foreach (var channel in data.Color) NumericValidation.ValidateRange(channel, 0, 1, "color");
            Color = new UnityEngine.Color(data.Color[0], data.Color[1], data.Color[2], data.Color[3]);
            Role = data.Role ?? throw new ArgumentException("role required.");
            Support = data.Support ?? throw new ArgumentException("support required.");
            if (!Enum.IsDefined(typeof(TravelerRole), Role) || !Enum.IsDefined(typeof(TravelerSupportKind), Support)) throw new ArgumentException("Invalid role/support.");
            Body = FixtureEnemyCatalog.ToDefinition(data.Body);
            if (Body.Id != Id || (Body.Visual.Id.IsValid && Id.ToString().StartsWith("FIXTURE-", StringComparison.Ordinal)))
                throw new ArgumentException("Traveler body must share identity; fixture Travelers use placeholder visuals.");
            PresenceSeconds = Positive(data.PresenceSeconds, "presenceSeconds");
            WanderSeconds = Positive(data.WanderSeconds, "wanderSeconds");
            RestSeconds = Nonnegative(data.RestSeconds, "restSeconds");
            AvoidRadius = Nonnegative(data.AvoidRadius, "avoidRadius");
            AvoidSeconds = Positive(data.AvoidSeconds, "avoidSeconds");
            GuardOffset = Nonnegative(data.GuardOffset, "guardOffset");
            SupportRadius = Nonnegative(data.SupportRadius, "supportRadius");
            Reduction = Nonnegative(data.Reduction, "reduction"); Resistance = Nonnegative(data.Resistance, "resistance");
            if (Reduction >= 1 || Resistance > 1) throw new ArgumentException("Support fractions out of range.");
            ShieldHp = Nonnegative(data.ShieldHp, "shieldHp"); ShieldSeconds = Nonnegative(data.ShieldSeconds, "shieldSeconds");
            SupportCooldown = Positive(data.SupportCooldown, "supportCooldown");
            SupportTargets = data.SupportTargets ?? throw new ArgumentException("supportTargets required.");
            NumericValidation.ValidateCount(SupportTargets, nameof(SupportTargets));
            if (Role != TravelerRole.Offensive && (Body.ContactDamage != 0 || Body.Attack != null)) throw new ArgumentException("Peaceful travelers cannot attack.");
            if ((Role == TravelerRole.Protector) != (Support != TravelerSupportKind.None)) throw new ArgumentException("Support belongs to protector role.");
            if (Support != TravelerSupportKind.None && SupportRadius <= 0) throw new ArgumentException("Support radius required.");
            if (Support == TravelerSupportKind.Shield && (ShieldHp <= 0 || ShieldSeconds <= 0)) throw new ArgumentException("Shield requires HP and duration.");
            MovementStyle = data.MovementStyle ?? throw new ArgumentException("movementStyle required.");
            if (!Enum.IsDefined(typeof(TravelerMovementStyle), MovementStyle)) throw new ArgumentException("Invalid movementStyle.");
            if (MovementStyle != TravelerMovementStyle.Wander && Role != TravelerRole.Wanderer)
                throw new ArgumentException("Only peaceful wanderers use an escape/orbit movement style.");
            if (MovementStyle == TravelerMovementStyle.ZigzagEscape)
            { ZigzagAngleDegrees = Positive(data.ZigzagAngleDegrees, "zigzagAngleDegrees"); ZigzagSeconds = Positive(data.ZigzagSeconds, "zigzagSeconds"); NumericValidation.ValidateRange(ZigzagAngleDegrees, 0, 89, "zigzagAngleDegrees"); }
            if (MovementStyle == TravelerMovementStyle.DashEscape)
            { EscapeDashDistance = Positive(data.EscapeDashDistance, "escapeDashDistance"); EscapeDashSeconds = Positive(data.EscapeDashSeconds, "escapeDashSeconds"); EscapeDashCooldownSeconds = Positive(data.EscapeDashCooldownSeconds, "escapeDashCooldownSeconds"); }
            if (MovementStyle == TravelerMovementStyle.Orbit)
            {
                OrbitRadiusX = Positive(data.OrbitRadiusX, "orbitRadiusX"); OrbitRadiusY = Positive(data.OrbitRadiusY, "orbitRadiusY");
                OrbitLeadDegrees = Positive(data.OrbitLeadDegrees, "orbitLeadDegrees"); NumericValidation.ValidateRange(OrbitLeadDegrees, 0, 90, "orbitLeadDegrees");
                TeleportMinSeconds = Positive(data.TeleportMinSeconds, "teleportMinSeconds"); TeleportMaxSeconds = Positive(data.TeleportMaxSeconds, "teleportMaxSeconds");
                if (TeleportMaxSeconds < TeleportMinSeconds) throw new ArgumentException("teleportMaxSeconds below teleportMinSeconds.");
            }
            if (Support == TravelerSupportKind.SpeedBurst)
            {
                EffectRadius = Positive(data.EffectRadius, "effectRadius"); SpeedBonus = Positive(data.SpeedBonus, "speedBonus"); EffectSeconds = Positive(data.EffectSeconds, "effectSeconds");
                if (EffectSeconds <= 0) throw new ArgumentException("SpeedBurst requires a duration.");
            }
            if (Support == TravelerSupportKind.Heal) HealAmount = Positive(data.HealAmount, "healAmount");
            if (Support != TravelerSupportKind.None)
            { SupportVerticalScale = Positive(data.SupportVerticalScale, "supportVerticalScale"); NumericValidation.ValidateRange(SupportVerticalScale, 0.1f, 1f, "supportVerticalScale"); }
            if (Support == TravelerSupportKind.Aura || Support == TravelerSupportKind.SpeedBurst || Support == TravelerSupportKind.Heal || MovementStyle == TravelerMovementStyle.Orbit)
            {
                if (data.EffectColor == null || data.EffectColor.Length != 4) throw new ArgumentException("effectColor requires RGBA for support/teleport effects.");
                foreach (var channel in data.EffectColor) NumericValidation.ValidateRange(channel, 0, 1, "effectColor");
                EffectColor = new UnityEngine.Color(data.EffectColor[0], data.EffectColor[1], data.EffectColor[2], data.EffectColor[3]);
                EffectShape = data.EffectShape ?? throw new ArgumentException("effectShape required with effectColor.");
                if (!Enum.IsDefined(typeof(TravelerEffectShape), EffectShape)) throw new ArgumentException("Invalid effectShape.");
            }
        }
        private static float Positive(float? value, string name) { var number = value ?? throw new ArgumentException(name + " required."); NumericValidation.ValidatePositive(number, name); return number; }
        private static float Nonnegative(float? value, string name) { var number = value ?? throw new ArgumentException(name + " required."); NumericValidation.ValidateNonNegative(number, name); return number; }
        public EnemyDefinition Scale(float multiplier)
        {
            NumericValidation.ValidatePositive(multiplier, nameof(multiplier));
            var a = Body.Attack;
            var attack = a == null ? null : new EnemyAttackProfile(a.Pattern, a.Damage * multiplier, a.CooldownSeconds,
                a.ProjectileSpeed, a.ProjectileLifetimeSeconds, a.ProjectileCount, a.SpreadDegrees, a.BurstIntervalSeconds,
                a.ProjectileRadius, a.ExplosionRadius, a.RotationStepDegrees, a.Controls, a.TelegraphSeconds,
                a.ProjectileVisual, a.Cadence, a.FixedOrientation, a.FollowUps, a.WindupMovementMultiplier);
            // Art references stay: the runtime builds the animated body from them (a scaled body without
            // them threw mid-spawn and left an unregistered, invulnerable Traveler, DECISION-0059).
            return new EnemyDefinition(Id, Body.MaxHealth * multiplier, Body.CollisionSize, Body.MovementSpeed,
                Body.ContactDamage * multiplier, Body.ContactDamageInterval, Body.ExperienceReward, visual: Body.Visual, movement: Body.Movement,
                attack: attack, knockbackResistance: Body.KnockbackResistance, contactControls: Body.ContactControls, dashContactControls: Body.DashContactControls,
                motionProfile: Body.MotionProfile, dashVolley: Body.DashVolley);
        }
        // Body art/motion/projectile visuals must reach the registry like a boss body does (DECISION-0057).
        public IEnumerable<ContentReference> GetReferencedContent() => Body.GetReferencedContent();
    }
}
