using System;
using System.Collections.Generic;
using Game.ActiveSkill.Json;
using Game.Content;
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

        private static ActiveSkillProgressionDefinition ToDefinition(ActiveSkillProgressionData data)
        {
            var levels = new ActiveSkillLevelDefinition[data.Levels.Length];
            for (var i = 0; i < levels.Length; i++)
                levels[i] = ToLevel(data.Levels[i]);

            return new ActiveSkillProgressionDefinition(data.Id, data.DisplayName, levels);
        }

        private static ActiveSkillLevelDefinition ToLevel(ActiveSkillLevelData data)
        {
            var waves = new ActiveSkillActivationWave[data.Waves.Length];
            for (var i = 0; i < waves.Length; i++)
                waves[i] = ToWave(data.Waves[i]);

            var visual = string.IsNullOrEmpty(data.VisualId)
                ? default
                : new ContentRef<SpriteDefinition>(data.VisualId);

            return new ActiveSkillLevelDefinition(data.BaseDamage, data.CooldownSeconds, data.TargetingMode, visual, waves);
        }

        private static ActiveSkillActivationWave ToWave(ActiveSkillActivationWaveData data)
        {
            var effects = new IActiveSkillEffect[data.Effects.Length];
            for (var i = 0; i < effects.Length; i++)
                effects[i] = ToEffect(data.Effects[i]);

            return new ActiveSkillActivationWave(data.DelaySeconds, data.RotationDegrees, data.DamageMultiplier, effects);
        }

        private static IActiveSkillEffect ToEffect(IActiveSkillEffectData data)
        {
            return data switch
            {
                ProjectileBurstEffectData p => new ProjectileBurstEffect(
                    p.ProjectileCount, p.Layout, p.SpreadDegrees, p.PierceCount, p.Speed,
                    p.LifetimeSeconds, p.CollisionRadius, p.ImpactAreaRadius, p.DamageMultiplier),
                BeamEffectData b => new BeamEffect(
                    b.DurationSeconds, b.TickIntervalSeconds, b.Width, b.Range, b.TracksTarget, b.DamageMultiplier),
                OrbitEffectData o => new OrbitEffect(
                    o.BladeCount, o.Radius, o.AngularSpeedDegrees, o.DurationSeconds, o.HitCooldownSeconds, o.DamageMultiplier),
                BoomerangEffectData bo => new BoomerangEffect(
                    bo.ProjectileCount, bo.SpreadDegrees, bo.Speed, bo.Range, bo.CollisionRadius,
                    bo.ReturnDamageMultiplier, bo.DamageMultiplier),
                ChainEffectData c => new ChainEffect(
                    c.TargetCount, c.JumpRange, c.DamageRetentionPerJump, c.DamageMultiplier),
                AreaEffectData a => new AreaEffect(a.Radius, a.DamageMultiplier),
                MineEffectData m => new MineEffect(
                    m.TriggerRadius, m.BlastRadius, m.LifetimeSeconds, m.MaxConcurrent,
                    m.SecondaryDelaySeconds, m.SecondaryDamageMultiplier, m.DamageMultiplier),
                _ => throw new ArgumentOutOfRangeException(nameof(data), $"Unsupported effect data type '{data.GetType().Name}'.")
            };
        }
    }
}
