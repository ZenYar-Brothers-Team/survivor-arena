using System.Collections;
using System.Linq;
using Game.Character;
using Game.Combat;
using Game.Content;
using Game.Enemy;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Bootstrap.PlayModeTests
{
    /// <summary>DECISION-0066 in the real gameplay loop: a zone hits and is drawn, summons join the run, run end cleans both.</summary>
    public sealed class BossHazardSmokeTests
    {
        [UnityTest]
        public IEnumerator Gameplay_ZoneHitsAndDraws_SummonsSpawn_TerminalCleansHazardsAndSummons()
        {
            ProfileSmokeScene.Load();
            yield return null;
            yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            CharacterSelectionSmokeDriver.StartDefault(root);
            yield return null;
            try
            {
                var run = Object.FindAnyObjectByType<RunController>();
                var spawner = Object.FindAnyObjectByType<ContinuousFixtureEnemySpawner>();
                var player = Object.FindAnyObjectByType<PlayerCharacterRuntime>();
                var timeline = spawner.Director.Timeline;
                run.Model.Tick(timeline.Hooks.Single(h => h.Kind == WaveHookKind.FinalBoss).TimeSeconds - run.Model.Elapsed);
                yield return null;
                yield return new WaitForFixedUpdate();
                var boss = root.BossEncounters.FinalBoss;
                Assert.IsNotNull(boss);
                var hazards = root.BossEncounters.HazardsOf(boss);
                var color = new Color(1f, .4f, .2f, .8f);
                var zone = new BossZoneProfile(BossZonePlacement.AtPlayer, 1, 2f, 0f, 0f, 0f, 0.3f, 40f,
                    new CombatControlProfile(0.3f, 0.12f), 0f, 0f, 0f, 0.45f, color, color);
                var before = player.Health.CurrentHealth;
                hazards.Start(BossSpecialRequest.ForZone(zone, new ContentId("FIXTURE-SMOKE-ZONE")), boss.Position, player.transform.position);
                yield return null;
                var shapes = root.BossEncounters.GetComponentsInChildren<BossHazardPresentation>();
                Assert.IsTrue(shapes.Any(view => view.VisibleShapes > 0), "The filling zone is drawn.");
                yield return new WaitForSeconds(0.5f);
                Assert.LessOrEqual(player.Health.CurrentHealth, before - 40f + 1e-3f, "The completed zone hit the player standing in it.");

                var minion = FixtureEnemyCatalog.Create()[0];
                var summon = new BossSummonProfile(minion, 2, 6f, 4, 0.2f, color);
                hazards.Start(BossSpecialRequest.ForSummon(summon, new ContentId("FIXTURE-SMOKE-SUMMON")), boss.Position,
                    player.transform.position);
                hazards.Start(BossSpecialRequest.ForZone(zone, new ContentId("FIXTURE-SMOKE-ZONE")), boss.Position, player.transform.position);
                yield return new WaitForSeconds(0.25f);
                Assert.AreEqual(2, root.BossEncounters.SummonedAlive);
                run.Model.Tick(run.Model.Duration);
                yield return null;
                Assert.AreNotEqual(RunState.Running, run.Model.State);
                Assert.AreEqual(0, root.BossEncounters.SummonedAlive, "Run end removes the summons.");
                Assert.IsTrue(root.BossEncounters.GetComponentsInChildren<BossHazardPresentation>(true).All(view => view.VisibleShapes == 0),
                    "Run end hides every hazard.");
            }
            finally { root.Shutdown(); }
        }
    }
}
