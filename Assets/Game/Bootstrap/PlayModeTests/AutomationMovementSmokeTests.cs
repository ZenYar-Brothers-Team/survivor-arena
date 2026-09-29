using System.Collections;
using Game.Bootstrap.Automation;
using Game.Automation;
using Game.Movement;
using Game.Progression;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class AutomationMovementSmokeTests
    {
        [UnityTest]
        public IEnumerator BotSteering_PreparedProductionScene_CollectsReachableXp()
        {
            GameplayCompositionRoot root = null;
            PlayerMover mover = null;
            try
            {
                ProductionSmokeScene.Load();
                yield return null;
                yield return null;
                root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                CharacterSelectionSmokeDriver.StartDefault(root);
                yield return null;
                mover = Object.FindAnyObjectByType<PlayerMover>();
                var experience = Object.FindAnyObjectByType<PlayerExperienceRuntime>();
                var run = Object.FindAnyObjectByType<RunController>();
                var drop = ExperienceDropFactory.Spawn(4f, (Vector2)mover.transform.position + Vector2.right * 2f,
                    60f, experience, run, pool: experience.DropPool);
                var settings = new MovementPolicyData { Id = "safePickup", Version = 1,
                    DecisionIntervalSeconds = 0.2f, ObservationRadius = 12f,
                    PredictionSeconds = 0.5f, ObstaclePadding = 0.15f, StuckSeconds = 3f };
                var policy = new BotMovementPolicy(settings);
                var observation = root.CreateAutomationObservationAdapter(12f);
                var source = new BotDirectionSource();
                mover.ConfigureInputSource(source);
                Assert.Greater(observation.Capture().Pickups.Count, 0);
                for (var i = 0; i < 200 && !drop.IsConsumed; i++)
                {
                    if (i % 10 == 0) source.SetDirection(policy.Decide(observation.Capture(), 0.2f).Direction);
                    yield return new WaitForFixedUpdate();
                }
                Assert.IsTrue(drop.IsConsumed, $"The bot must collect visible XP without a grant or teleport. " +
                    $"Position={mover.transform.position}, drop={drop.transform.position}, intent={source.Direction}, " +
                    $"observedPickups={observation.Capture().Pickups.Count}.");
            }
            finally
            {
                if (mover != null) mover.ConfigureInputSource(null);
                if (root != null && root.IsInitialized) root.Shutdown();
            }
        }

        [UnityTest]
        public IEnumerator BotInput_ProductionRun_UsesOrdinaryPhysicsAndStopsOnPauseAndEnd()
        {
            GameplayCompositionRoot root = null;
            PlayerMover mover = null;
            try
            {
                ProductionSmokeScene.Load();
                yield return null;
                yield return null;
                root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                CharacterSelectionSmokeDriver.StartDefault(root);
                yield return null;
                mover = Object.FindAnyObjectByType<PlayerMover>();
                var run = Object.FindAnyObjectByType<RunController>();
                var adapter = root.CreateAutomationObservationAdapter(12f);
                var before = adapter.Capture();
                Assert.IsTrue(before.CoverageComplete);
                Assert.Greater(before.MovementSpeed, 0f);
                var source = new BotDirectionSource();
                mover.ConfigureInputSource(source);
                source.SetDirection(Vector2.right);
                var initial = mover.transform.position;
                for (var i = 0; i < 10; i++) yield return new WaitForFixedUpdate();
                Assert.Greater(mover.transform.position.x, initial.x, "The ordinary Rigidbody path must move the player.");
                Assert.Greater(adapter.Capture().Position.x, initial.x);

                run.Model.RequestPause(RunPauseReasons.Manual);
                yield return new WaitForFixedUpdate();
                Assert.AreEqual(Vector2.zero, mover.MovementDirection);
                run.Model.ReleasePause(RunPauseReasons.Manual);
                run.Model.Stop();
                yield return new WaitForFixedUpdate();
                Assert.AreEqual(Vector2.zero, mover.MovementDirection);
            }
            finally
            {
                if (mover != null) mover.ConfigureInputSource(null);
                if (root != null && root.IsInitialized) root.Shutdown();
            }
        }
    }
}
