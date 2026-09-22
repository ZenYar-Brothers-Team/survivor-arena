using System;
using System.Collections.Generic;
using Game.ActiveSkill.Json;
using Game.Content;
using Game.Combat;
using Game.Content.Json;
using Game.Presentation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Game.ActiveSkill
{
    // Non-production active-skill content, config-driven instead of hardcoded:
    // see Resources/Content/ActiveSkills/FixtureActiveSkills.json and AGENTS.md's
    // content-config rule. Effects are polymorphic in JSON via a "kind"
    // discriminator, resolved by ActiveSkillEffectJsonConverter.
    public static class FixtureActiveSkillCatalog
    {
        private const string ResourcePath = "Content/ActiveSkills/FixtureActiveSkills";

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter(), new ActiveSkillEffectJsonConverter() },
            MissingMemberHandling = MissingMemberHandling.Error,
        };

        public static IReadOnlyList<ActiveSkillProgressionDefinition> Create()
        {
            var data = JsonContentFile.Load<ActiveSkillProgressionData[]>(ResourcePath, Settings);
            var definitions = new ActiveSkillProgressionDefinition[data.Length];
            for (var i = 0; i < data.Length; i++)
                definitions[i] = ToDefinition(data[i]);

            return definitions;
        }

        public static ActiveSkillProgressionDefinition ToDefinition(ActiveSkillProgressionData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Levels != null && (data.BaseLevel != null || data.LevelChanges != null))
                throw new ArgumentException("Use either resolved Levels or BaseLevel + LevelChanges.");
            var sourceLevels = data.Levels ?? ActiveSkillLevelResolver.Resolve(data.BaseLevel, data.LevelChanges, Settings);
            var levels = new ActiveSkillLevelDefinition[sourceLevels.Length];
            for (var i = 0; i < levels.Length; i++)
                levels[i] = ToLevel(sourceLevels[i]);

            var icon = string.IsNullOrEmpty(data.IconVisualId)
                ? default
                : new ContentRef<SpriteDefinition>(data.IconVisualId);
            return new ActiveSkillProgressionDefinition(data.Id, data.DisplayName, icon, levels);
        }

        private static ActiveSkillLevelDefinition ToLevel(ActiveSkillLevelData data)
        {
            var waves = new ActiveSkillActivationWave[data.Waves.Length];
            for (var i = 0; i < waves.Length; i++)
                waves[i] = ToWave(data.Waves[i]);

            var visual = string.IsNullOrEmpty(data.VisualId)
                ? default
                : new ContentRef<SpriteDefinition>(data.VisualId);

            if (data.TargetingMode == ActiveSkillTargetingMode.RandomEnemy && !data.TargetingRadius.HasValue)
                throw new ArgumentException("Random targeting requires TargetingRadius.");
            if (data.TargetingMode == ActiveSkillTargetingMode.MovementDirection && !data.InitialDirectionDegrees.HasValue)
                throw new ArgumentException("Movement targeting requires InitialDirectionDegrees before the first movement.");
            foreach (var wave in waves)
                foreach (var effect in wave.Effects)
                    if (effect is ProjectileBurstEffect projectile && projectile.Layout == ProjectileLayout.IndependentRandom && !data.RandomSeed.HasValue)
                        throw new ArgumentException("IndependentRandom requires RandomSeed.");
            var targeting = new ActiveSkillTargetingProfile(data.TargetingMode, data.TargetingRadius ?? 0f,
                data.RandomSeed, data.InitialDirectionDegrees ?? 0f, data.ActionSpeedBonus, data.RotationPerActivationDegrees);
            return new ActiveSkillLevelDefinition(data.BaseDamage, data.CooldownSeconds, targeting, visual, waves);
        }

        private static ActiveSkillActivationWave ToWave(ActiveSkillActivationWaveData data)
        {
            var effects = new IActiveSkillEffect[data.Effects.Length];
            for (var i = 0; i < effects.Length; i++)
                effects[i] = ToEffect(data.Effects[i]);

            return new ActiveSkillActivationWave(data.DelaySeconds, data.RotationDegrees, data.DamageMultiplier, data.Controls?.ToProfile() ?? CombatControlProfile.None, effects);
        }

        private static IActiveSkillEffect ToEffect(IActiveSkillEffectData data)
        {
            return data switch
            {
                ProjectileBurstEffectData p => new ProjectileBurstEffect(
                    p.ProjectileCount, p.Layout, p.SpreadDegrees, p.PierceCount, p.Speed,
                    p.LifetimeSeconds, p.CollisionRadius, p.ImpactAreaRadius, p.DamageMultiplier, p.Behavior?.ToBehavior()),
                BeamEffectData b => new BeamEffect(
                    b.DurationSeconds, b.TickIntervalSeconds, b.Width, b.Range, b.TracksTarget, b.DamageMultiplier),
                OrbitEffectData o => new OrbitEffect(
                    o.BladeCount, o.Radius, o.AngularSpeedDegrees, o.DurationSeconds, o.HitCooldownSeconds, o.DamageMultiplier, o.BladeHitboxRadius ?? throw new ArgumentException("Orbit requires BladeHitboxRadius.")),
                BoomerangEffectData bo => new BoomerangEffect(
                    bo.ProjectileCount, bo.SpreadDegrees, bo.Speed, bo.Range, bo.CollisionRadius,
                    bo.ReturnDamageMultiplier, bo.DamageMultiplier,
                    bo.HitCooldownSeconds ?? throw new ArgumentException("Boomerang requires HitCooldownSeconds."),
                    bo.ReturnKnockbackMultiplier ?? throw new ArgumentException("Boomerang requires ReturnKnockbackMultiplier."),
                    bo.LifetimeSeconds ?? throw new ArgumentException("Boomerang requires LifetimeSeconds.")),
                ChainEffectData c => new ChainEffect(
                    c.TargetCount, c.JumpRange, c.DamageRetentionPerJump, c.DamageMultiplier),
                AreaEffectData a => new AreaEffect(a.Radius, a.DamageMultiplier),
                MineEffectData m => new MineEffect(
                    m.TriggerRadius, m.BlastRadius, m.LifetimeSeconds, m.MaxConcurrent,
                    m.SecondaryDelaySeconds, m.SecondaryDamageMultiplier, m.DamageMultiplier,
                    m.SecondaryRadiusMultiplier ?? (m.SecondaryDamageMultiplier > 0f ? throw new ArgumentException("Secondary mine requires SecondaryRadiusMultiplier.") : 1f),
                    m.SecondaryKnockbackMultiplier ?? (m.SecondaryDamageMultiplier > 0f ? throw new ArgumentException("Secondary mine requires SecondaryKnockbackMultiplier.") : 1f)),
                _ => throw new ArgumentOutOfRangeException(nameof(data), $"Unsupported effect data type '{data.GetType().Name}'.")
            };
        }
    }
}
