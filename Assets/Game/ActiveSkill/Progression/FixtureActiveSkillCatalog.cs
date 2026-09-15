using System;
using System.Collections.Generic;
using Game.Content;
using Game.Presentation;

namespace Game.ActiveSkill
{
    public static class FixtureActiveSkillCatalog
    {
        public static IReadOnlyList<ActiveSkillProgressionDefinition> Create()
        {
            return new[]
            {
                CreateBolt(),
                CreateRing(),
                CreateBeam(),
                CreateOrbit(),
                CreateBoomerang(),
                CreateChain(),
                CreateMine(),
                CreateDelayedArea()
            };
        }

        private static ActiveSkillProgressionDefinition CreateBolt()
        {
            return CreateSixLevels("FIXTURE-SKILL-BOLT", "Fixture Bolt", level =>
            {
                var count = level >= 5 ? 3 : level >= 3 ? 2 : 1;
                var layout = count > 1 ? ProjectileLayout.Fan : ProjectileLayout.Single;
                var pierce = level >= 4 ? 1 : 0;
                return Level(
                    3f + level,
                    level >= 5 ? 0.65f : 0.9f,
                    ActiveSkillTargetingMode.NearestEnemy,
                    Wave(new ProjectileBurstEffect(count, layout, 24f, pierce, 10f, 2f, 0.15f)));
            });
        }

        private static ActiveSkillProgressionDefinition CreateRing()
        {
            return CreateSixLevels("FIXTURE-SKILL-RING", "Fixture Ring/Cross", level =>
            {
                var count = level >= 4 ? 8 : level >= 2 ? 6 : 4;
                var layout = level >= 4 ? ProjectileLayout.Cross : ProjectileLayout.Ring;
                // Each tier's look is its own content reference rather than a branch
                // buried in the projectile math, so a validator can catch a tier that
                // never got real art instead of silently reusing the previous sprite.
                var visual = new ContentRef<SpriteDefinition>(
                    level >= 4 ? "FIXTURE-SKILL-RING-VISUAL-CROSS" : "FIXTURE-SKILL-RING-VISUAL-RING");
                var burst = new ProjectileBurstEffect(count, layout, 0f, level >= 3 ? 1 : 0, 7f, 2f, 0.12f);
                if (level == 6)
                {
                    return Level(3f, 1.8f, ActiveSkillTargetingMode.Self, visual,
                        Wave(burst),
                        new ActiveSkillActivationWave(0.25f, 22.5f, 0.75f, burst));
                }
                return Level(2f + level * 0.25f, 2f, ActiveSkillTargetingMode.Self, visual, Wave(burst));
            });
        }

        private static ActiveSkillProgressionDefinition CreateBeam()
        {
            return CreateSixLevels("FIXTURE-SKILL-BEAM", "Fixture Beam", level =>
                Level(
                    1.5f + level * 0.25f,
                    level >= 5 ? 2f : 2.6f,
                    ActiveSkillTargetingMode.NearestEnemy,
                    Wave(new BeamEffect(
                        level >= 6 ? 1.2f : level >= 2 ? 0.8f : 0.6f,
                        0.2f,
                        level >= 4 ? 0.8f : 0.5f,
                        8f,
                        tracksTarget: level >= 6))));
        }

        private static ActiveSkillProgressionDefinition CreateOrbit()
        {
            return CreateSixLevels("FIXTURE-SKILL-ORBIT", "Fixture Orbit", level =>
                Level(
                    2f + level * 0.3f,
                    3f,
                    ActiveSkillTargetingMode.Self,
                    Wave(new OrbitEffect(
                        level >= 6 ? 4 : level >= 3 ? 3 : 2,
                        level >= 4 ? 2.2f : 1.7f,
                        level >= 5 ? 180f : 120f,
                        2.5f,
                        0.35f))));
        }

        private static ActiveSkillProgressionDefinition CreateBoomerang()
        {
            return CreateSixLevels("FIXTURE-SKILL-BOOMERANG", "Fixture Boomerang", level =>
                Level(
                    4f + level * 0.5f,
                    level >= 5 ? 1.7f : 2.2f,
                    ActiveSkillTargetingMode.NearestEnemy,
                    Wave(new BoomerangEffect(
                        level >= 4 ? 2 : 1,
                        level >= 4 ? 18f : 0f,
                        7f,
                        level >= 3 ? 5f : 4f,
                        0.18f,
                        level >= 6 ? 1.75f : 1f))));
        }

        private static ActiveSkillProgressionDefinition CreateChain()
        {
            return CreateSixLevels("FIXTURE-SKILL-CHAIN", "Fixture Chain", level =>
                Level(
                    3f + level * 0.5f,
                    level >= 5 ? 1.4f : 1.9f,
                    ActiveSkillTargetingMode.NearestEnemy,
                    Wave(new ChainEffect(
                        level >= 6 ? 8 : level >= 4 ? 6 : level >= 2 ? 4 : 3,
                        4f,
                        level >= 6 ? 0.95f : 0.85f))));
        }

        private static ActiveSkillProgressionDefinition CreateMine()
        {
            return CreateSixLevels("FIXTURE-SKILL-MINE", "Fixture Mine", level =>
                Level(
                    6f + level,
                    level >= 5 ? 2f : 2.7f,
                    ActiveSkillTargetingMode.Self,
                    Wave(new MineEffect(
                        0.8f,
                        level >= 2 ? 1.8f : 1.4f,
                        4f,
                        level >= 4 ? 6 : 4,
                        secondaryDelaySeconds: level >= 6 ? 0.35f : 0f,
                        secondaryDamageMultiplier: level >= 6 ? 0.6f : 0f))));
        }

        private static ActiveSkillProgressionDefinition CreateDelayedArea()
        {
            return CreateSixLevels("FIXTURE-SKILL-DELAYED", "Fixture Delayed Area", level =>
            {
                var area = new AreaEffect(level >= 2 ? 1.8f : 1.4f);
                if (level >= 6)
                {
                    return Level(8f + level, 3.2f, ActiveSkillTargetingMode.NearestEnemy,
                        new ActiveSkillActivationWave(0.45f, 0f, 1f, area),
                        new ActiveSkillActivationWave(0.7f, 0f, 0.8f, area),
                        new ActiveSkillActivationWave(0.95f, 0f, 0.65f, area));
                }
                if (level >= 4)
                {
                    return Level(8f + level, 3.5f, ActiveSkillTargetingMode.NearestEnemy,
                        new ActiveSkillActivationWave(0.45f, 0f, 1f, area),
                        new ActiveSkillActivationWave(0.75f, 0f, 0.65f, area));
                }
                return Level(8f + level, 3.5f, ActiveSkillTargetingMode.NearestEnemy,
                    new ActiveSkillActivationWave(0.45f, 0f, 1f, area));
            });
        }

        private static ActiveSkillProgressionDefinition CreateSixLevels(
            string id,
            string name,
            Func<int, ActiveSkillLevelDefinition> factory)
        {
            var levels = new ActiveSkillLevelDefinition[ActiveSkillProgressionDefinition.MaxLevel];
            for (var level = 1; level <= levels.Length; level++)
                levels[level - 1] = factory(level);
            return new ActiveSkillProgressionDefinition(id, name, levels);
        }

        private static ActiveSkillLevelDefinition Level(
            float damage,
            float cooldown,
            ActiveSkillTargetingMode targeting,
            params ActiveSkillActivationWave[] waves)
        {
            return new ActiveSkillLevelDefinition(damage, cooldown, targeting, waves);
        }

        private static ActiveSkillLevelDefinition Level(
            float damage,
            float cooldown,
            ActiveSkillTargetingMode targeting,
            ContentRef<SpriteDefinition> visual,
            params ActiveSkillActivationWave[] waves)
        {
            return new ActiveSkillLevelDefinition(damage, cooldown, targeting, visual, waves);
        }

        private static ActiveSkillActivationWave Wave(params IActiveSkillEffect[] effects)
        {
            return new ActiveSkillActivationWave(0f, 0f, 1f, effects);
        }
    }
}
