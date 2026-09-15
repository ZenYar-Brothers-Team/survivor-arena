using System;
using Game.Run;

namespace Game.ActiveSkill
{
    public sealed class SceneProjectileLauncher : IActiveSkillProjectileLauncher
    {
        private readonly RunController _runController;

        public SceneProjectileLauncher(RunController runController)
        {
            _runController = runController != null
                ? runController
                : throw new ArgumentNullException(nameof(runController));
        }

        public void Launch(ActiveSkillProjectile projectile)
        {
            FixtureProjectileFactory.Spawn(projectile, _runController);
        }
    }
}
