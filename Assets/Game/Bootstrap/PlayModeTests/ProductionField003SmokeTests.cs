using System.Collections;
using System.Linq;
using Game.Content;
using Game.Enemy;
using Game.Meta;
using Game.Presentation;
using Game.Run;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Game.Bootstrap.PlayModeTests
{
    /// <summary>FIELD-003 (DECISION-0067): the ruins field starts in the real scene with its own layout and pool.</summary>
    public sealed class ProductionField003SmokeTests
    {
        private static readonly string[] Pool = { "ENEMY-001", "ENEMY-002", "ENEMY-003", "ENEMY-004", "ENEMY-005", "ENEMY-006",
            "ENEMY-007", "ENEMY-008", "ENEMY-009", "ENEMY-010" };

        [UnityTest]
        public IEnumerator Field003_StartsWithItsRuins_VerticalWallsExcludeTransparentPadding_AndSpawnsOnlyItsPool()
        {
            var warnings = new System.Collections.Generic.List<string>();
            Application.LogCallback collect = (message, _, type) =>
            {
                if (type == LogType.Warning && !message.StartsWith("[Perf]")) warnings.Add(message);
            };
            Application.logMessageReceived += collect;
            GameplayCompositionRoot root = null;
            try
            {
                // A profile that has cleared FIELD-001 and FIELD-002, so FIELD-003 is open through the normal field screen.
                var codec = new ProfileCodec(MetaCatalog.Load());
                var profile = codec.Create();
                profile.ClearedFields.Add("FIELD-001");
                profile.ClearedFields.Add("FIELD-002");
                profile.Unlocked.Add("FIELD-002");
                profile.Unlocked.Add("FIELD-003");
                var store = new MemoryProfileStore();
                store.WriteAsync(codec.Encode(profile)).GetAwaiter().GetResult();
                ProductionSmokeScene.Load(store);
                yield return null;
                yield return null;
                root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                var character = root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart);
                using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = character; character.SendEvent(submit); }
                root.UseReferenceSeeds = true;
                Assert.IsTrue(root.TryStartField(new ContentId("FIELD-003")), "FIELD-003 opens after FIELD-002 is cleared.");
                yield return null;

                var run = Object.FindAnyObjectByType<RunController>();
                Assert.AreEqual("FIELD-003", run.Model.Selection.FieldId.ToString());
                var art = GameObject.Find("FieldEnvironmentArt");
                // DECISION-0068: the colliders are exactly this run's generated layout.
                var layout = FixtureFieldEnvironmentPresentationCatalog.Load(RuntimeContentCatalog.ProductionFieldPresentationPath)
                    .Values.Single(p => p.Id.ToString() == "FIELD-003-PRESENTATION").ObstacleLayout;
                var expected = FieldObstacleLayoutGenerator.Generate(layout, 200f, Vector2.zero, root.LayoutSeed, "FIELD-003-ENVIRONMENT");
                Assert.AreEqual(expected.Count, art.GetComponentsInChildren<Collider2D>().Length);
                // The smoke checks layout and spawn pool; an idle player may not survive 12 s of FIELD-003 after the
                // DECISION-0073 skill nerf, so health is locked as with the development toggle.
                Object.FindAnyObjectByType<Game.Character.PlayerCharacterRuntime>().Health.IsLocked = true;
                for (var i = 0; i < 600; i++) yield return new WaitForFixedUpdate();
                Assert.AreEqual(RunState.Running, run.Model.State);
                var enemies = Object.FindObjectsByType<EnemyRuntime>(FindObjectsSortMode.None);
                Assert.Greater(enemies.Length, 0, "The FIELD-003 timeline spawns from the start.");
                foreach (var enemy in enemies)
                    CollectionAssert.Contains(Pool, enemy.ContentId.ToString());
            }
            finally
            {
                if (root != null && root.IsInitialized) root.Shutdown();
                Application.logMessageReceived -= collect;
            }
            CollectionAssert.IsEmpty(warnings, "Only PerfGuard timing warnings are tolerated.");
        }
    }
}
