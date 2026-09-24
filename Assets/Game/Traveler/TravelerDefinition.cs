using System;
using System.Collections.Generic;
using Game.Content;
using Game.Enemy;
using Game.Traveler.Json;
namespace Game.Traveler
{
    public sealed class TravelerDefinition : IContentDefinition
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
                a.ProjectileVisual);
            return new EnemyDefinition(Id, Body.MaxHealth * multiplier, Body.CollisionSize, Body.MovementSpeed,
                Body.ContactDamage * multiplier, Body.ContactDamageInterval, Body.ExperienceReward, movement: Body.Movement,
                attack: attack, knockbackResistance: Body.KnockbackResistance, contactControls: Body.ContactControls, dashContactControls: Body.DashContactControls);
        }
    }
}
