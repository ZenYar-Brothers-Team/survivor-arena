using System.Collections.Generic;

namespace Game.ActiveSkill.Tests
{
    public sealed class CombatRecordingLauncher : IActiveSkillProjectileLauncher
    {
        public List<ActiveSkillProjectile> Projectiles { get; } = new List<ActiveSkillProjectile>();
        public void Launch(ActiveSkillProjectile projectile) => Projectiles.Add(projectile);
    }
}
