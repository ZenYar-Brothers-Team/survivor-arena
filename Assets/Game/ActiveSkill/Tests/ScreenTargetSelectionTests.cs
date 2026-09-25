using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    /// <summary>DECISION-0058: player attacks choose enemies and points only on the visible screen.</summary>
    public sealed class ScreenTargetSelectionTests
    {
        // Reference screen 10 units high at 16:9 around the player at the origin.
        private static readonly TargetViewportRect Screen = new TargetViewportRect(Vector2.zero, 8.9f, 5f);

        [Test]
        public void Rect_ContainsOnlyVisiblePoints_AndPicksPointsOnScreenWithinRadius()
        {
            Assert.IsTrue(Screen.Contains(new Vector2(8.8f, 4.9f)));
            Assert.IsFalse(Screen.Contains(new Vector2(0f, 5.5f)));
            var random = new System.Random(7);
            for (var i = 0; i < 200; i++)
            {
                Assert.IsTrue(Screen.TryPickPoint(Vector2.zero, 6f, random, out var point));
                Assert.IsTrue(Screen.Contains(point));
                Assert.LessOrEqual(point.magnitude, 6f + 1e-4f);
            }
        }

        [Test]
        public void Nearest_PrefersAVisibleEnemy_OverACloserOffscreenOne()
        {
            var offscreen = new SkillTestTarget(new Vector2(0f, 5.5f));
            var visible = new SkillTestTarget(new Vector2(7f, 0f));
            var provider = new SceneEnemyTargetProvider(new SkillTestCombatQuery(offscreen, visible), new FixedTargetViewport(Screen));

            Assert.IsTrue(provider.TryGetTarget(Vector2.zero, out var target));
            Assert.AreSame(visible, target);
        }

        [Test]
        public void NearestSkill_WithNoEnemyOnScreen_FiresAlongItsPreviousDirection()
        {
            using var context = new SkillFrameworkTestContext();
            var definition = FixtureActiveSkillCatalog.Create().Single(s => s.Id.ToString() == "FIXTURE-SKILL-BOLT");
            var skill = new ActiveSkillInstance(definition);
            var recorder = new SkillActivationRecorder();
            var enemy = new SkillTestTarget(new Vector2(0f, 3f));
            var viewport = new FixedTargetViewport(Screen);
            var provider = new SceneEnemyTargetProvider(new SkillTestCombatQuery(enemy), viewport);

            Assert.IsTrue(skill.Tick(0f, true, context.Player, provider, recorder));
            Assert.AreSame(enemy, recorder.Activations[0].InitialTarget);
            enemy.Position = new Vector2(0f, 6f); // left the screen
            Assert.IsTrue(skill.Tick(10f, true, context.Player, provider, recorder), "The attack does not wait for a target.");

            Assert.IsNull(recorder.Activations[1].InitialTarget);
            Assert.Less(Vector2.Distance(Vector2.up, recorder.Activations[1].AimDirection), 1e-4f);
        }

        [Test]
        public void RandomSkill_WithNoEnemyOnScreen_StrikesARandomVisiblePointInRange()
        {
            using var context = new SkillFrameworkTestContext();
            var definition = FixtureActiveSkillCatalog.Create().Single(s => s.Id.ToString() == "FIXTURE-SKILL-WORLD-TARGET");
            var skill = new ActiveSkillInstance(definition);
            var recorder = new SkillActivationRecorder();
            var offscreen = new SkillTestTarget(new Vector2(20f, 0f));
            var provider = new SceneEnemyTargetProvider(new SkillTestCombatQuery(offscreen), new FixedTargetViewport(Screen));

            Assert.IsTrue(skill.Tick(0f, true, context.Player, provider, recorder));
            var activation = recorder.Activations.Single();
            Assert.IsNull(activation.InitialTarget, "An off-screen enemy is never chosen.");
            Assert.IsTrue(Screen.Contains(activation.AimPoint));
            Assert.AreEqual(activation.AimPoint, skill.LastAimPoint);
        }

        [Test]
        public void WithoutScreenLimit_ANearestSkillStillWaitsForATarget()
        {
            using var context = new SkillFrameworkTestContext();
            var definition = FixtureActiveSkillCatalog.Create().Single(s => s.Id.ToString() == "FIXTURE-SKILL-BOLT");
            var skill = new ActiveSkillInstance(definition);

            Assert.IsFalse(skill.Tick(0f, true, context.Player, new SceneEnemyTargetProvider(new SkillTestCombatQuery()), new SkillActivationRecorder()));
        }
    }
}
