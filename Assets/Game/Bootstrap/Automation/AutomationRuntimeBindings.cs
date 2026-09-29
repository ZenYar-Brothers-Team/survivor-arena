using System;
using Game.Movement;
using Game.Progression;
using Game.Run;
using Game.Character;
using Game.Enemy;

namespace Game.Bootstrap.Automation
{
    /// <summary>Explicit, run-scoped bindings created by the composition root only after initialization.</summary>
    public sealed class AutomationRuntimeBindings
    {
        public RunController Run { get; }
        public LevelUpDraftRuntime Draft { get; }
        public PlayerMover Mover { get; }
        public AutomationObservationAdapter Observation { get; }
        public PlayerCharacterRuntime Player { get; }
        public PlayerExperienceRuntime Experience { get; }
        public ContinuousFixtureEnemySpawner Spawner { get; }

        public AutomationRuntimeBindings(RunController run, LevelUpDraftRuntime draft, PlayerMover mover,
            AutomationObservationAdapter observation, PlayerCharacterRuntime player,
            PlayerExperienceRuntime experience, ContinuousFixtureEnemySpawner spawner)
        {
            Run = run ?? throw new ArgumentNullException(nameof(run));
            Draft = draft ?? throw new ArgumentNullException(nameof(draft));
            Mover = mover ?? throw new ArgumentNullException(nameof(mover));
            Observation = observation ?? throw new ArgumentNullException(nameof(observation));
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Experience = experience ?? throw new ArgumentNullException(nameof(experience));
            Spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
        }
    }
}
