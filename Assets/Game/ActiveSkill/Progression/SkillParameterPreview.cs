using System.Collections.Generic;

namespace Game.ActiveSkill
{
    /// <summary>Resolved definition values for upgrade comparisons, without reading or mutating runtime stats.</summary>
    public static class SkillParameterPreview
    {
        public static Dictionary<string, float> Capture(ActiveSkillLevelDefinition level)
        {
            var result = new Dictionary<string, float>();
            result["Action speed bonus"] = level.Targeting.ActionSpeedBonus;
            result["Targeting radius"] = level.Targeting.Radius;
            for (var i = 0; i < level.Waves.Count; i++)
            {
                var wave = level.Waves[i];
                var prefix = $"Wave {i + 1}: ";
                result[prefix + "delay"] = wave.DelaySeconds;
                result[prefix + "knockback"] = wave.Controls.KnockbackDistance;
                result[prefix + "slow"] = wave.Controls.SlowFraction;
                for (var j = 0; j < wave.Effects.Count; j++)
                {
                    var key = prefix + (wave.Effects.Count == 1 ? "" : $"Effect {j + 1}: ");
                    switch (wave.Effects[j])
                    {
                        case ProjectileBurstEffect p:
                            result[key + "projectiles"] = p.ProjectileCount;
                            result[key + "speed"] = p.Speed;
                            result[key + "lifetime"] = p.LifetimeSeconds;
                            result[key + "projectile radius"] = p.CollisionRadius;
                            result[key + "pierce"] = p.PierceCount;
                            result[key + "explosion radius"] = p.ImpactAreaRadius;
                            result[key + "time to stop"] = p.Behavior.StopAfterSeconds;
                            result[key + "ricochets"] = p.Behavior.RicochetCount;
                            break;
                        case BoomerangEffect b:
                            result[key + "projectiles"] = b.ProjectileCount;
                            result[key + "range"] = b.Range;
                            result[key + "projectile radius"] = b.CollisionRadius;
                            result[key + "return damage"] = b.ReturnDamageMultiplier;
                            break;
                        case BeamEffect b:
                            result[key + "width"] = b.Width;
                            result[key + "range"] = b.Range;
                            result[key + "duration"] = b.DurationSeconds;
                            break;
                        case OrbitEffect o:
                            result[key + "blades"] = o.BladeCount;
                            result[key + "orbit radius"] = o.Radius;
                            result[key + "blade radius"] = o.BladeHitboxRadius;
                            result[key + "rotation speed"] = o.AngularSpeedDegrees;
                            break;
                        case ChainEffect c:
                            result[key + "targets"] = c.TargetCount;
                            result[key + "jump range"] = c.JumpRange;
                            result[key + "damage retention"] = c.DamageRetentionPerJump;
                            break;
                        case AreaEffect a: result[key + "radius"] = a.Radius; break;
                        case MineEffect m:
                            result[key + "blast radius"] = m.BlastRadius;
                            result[key + "lifetime"] = m.LifetimeSeconds;
                            result[key + "mine limit"] = m.MaxConcurrent;
                            break;
                    }
                }
            }
            return result;
        }
    }
}
