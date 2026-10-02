using System.Collections;
using System.Linq;
using Game.Content;
using Game.Meta;
using Game.Run;
using Game.UI;
using Game.Zones;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Game.Bootstrap.PlayModeTests
{
    /// <summary>FIELD-009: starts on the start platform, hurts the player in the void and cleans up.</summary>
    public sealed class ProductionField009SmokeTests
    {
        [UnityTest]
        public IEnumerator PlatformsField_StartsOnAPlatform_HurtsInTheVoid_AndRemovesItselfOnShutdown()
        {
            GameplayCompositionRoot root = null;
            try
            {
                ProductionSmokeScene.Load(new MemoryProfileStore(), false);
                yield return null; yield return null;
                root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                Assert.IsFalse(root.Profile.IsUnlocked("FIELD-009"), "A fresh profile cannot select FIELD-009 before the Dev unlock.");
                root.UnlockAllForDevelopment();
                for (var i = 0; i < 30 && !root.Profile.IsUnlocked("FIELD-009"); i++) yield return null;
                Assert.IsTrue(root.Profile.IsUnlocked("FIELD-009"), "The Dev button unlocks FIELD-009.");
                root.Play(); yield return null;
                UiFoundationSmokeTests.Submit(root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart));
                yield return null;
                var card = root.FieldSelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.FieldSelectCard("FIELD-009"));
                Assert.IsNotNull(card, "FIELD-009 has a card on the field select screen."); Assert.IsTrue(card.enabledSelf);
                UiFoundationSmokeTests.Submit(card);
                root.UseReferenceSeeds = true;
                UiFoundationSmokeTests.Submit(root.FieldSelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.FieldSelectStart));
                yield return null; yield return null;

                var run = Object.FindAnyObjectByType<RunController>();
                var driver = Object.FindAnyObjectByType<FieldVoidDamageDriver>();
                Assert.IsNotNull(driver, "The void damage driver exists on a platform field.");
                var layout = driver.Layout;
                var player = Object.FindAnyObjectByType<Game.Character.PlayerCharacterRuntime>();
                Assert.That(Vector2.Distance(player.transform.position, layout.SpawnPosition), Is.LessThan(.5f), "The run starts on the start platform.");
                Assert.IsTrue(layout.IsWalkable(player.transform.position));
                Assert.AreEqual(RunState.Running, run.Model.State);

                Assert.IsNotNull(GameObject.Find("PlatformNetwork/PlatformRim"));
                Assert.IsNotNull(layout.Profile.Art, "FIELD-009 uses the approved material profile.");
                var ground = GameObject.Find("PlatformNetwork/Void").GetComponent<MeshRenderer>().sharedMaterial;
                Assert.AreEqual("SurvivorArena/FieldPlatformSurface", ground.shader.name);
                Assert.IsNotNull(ground.mainTexture);
                var camera = Camera.main;
                var bridge = layout.Bridges.First(b => b.From == layout.StartIndex || b.To == layout.StartIndex);
                var other = bridge.From == layout.StartIndex ? bridge.To : bridge.From;
                var direction = (layout.Platforms[other].Center - layout.SpawnPosition).normalized;
                var view = layout.SpawnPosition + direction * layout.Platforms[layout.StartIndex].Radius;
                var reviewPosition = view - direction;
                player.transform.position = reviewPosition;
                player.GetComponent<Rigidbody2D>().position = reviewPosition;
                if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
                {
                    var target = new RenderTexture(1920, 1080, 24); target.Create();
                    var captureObject = new GameObject("Platform art capture");
                    try
                    {
                        var capture = captureObject.AddComponent<Camera>(); capture.CopyFrom(camera);
                        capture.targetTexture = target;
                        capture.transform.position = new Vector3(view.x, view.y, camera.transform.position.z);
                        yield return null; yield return null;
                        UiFoundationSmokeTests.Capture(target, "field009-art-junction");
                        capture.orthographicSize = layout.Profile.ArenaSideLength * .55f;
                        capture.transform.position = new Vector3(0, 0, camera.transform.position.z);
                        yield return null; yield return null;
                        UiFoundationSmokeTests.Capture(target, "field009-art-overview");
                    }
                    finally
                    {
                        Object.DestroyImmediate(captureObject);
                        target.Release(); Object.DestroyImmediate(target);
                    }
                }

                var half = layout.Profile.ArenaSideLength * .5f - 3f;
                var voidPoint = Vector2.zero;
                var found = false;
                for (var x = -half; x <= half && !found; x += 2f)
                for (var y = -half; y <= half && !found; y += 2f)
                    if (!layout.IsWalkable(new Vector2(x, y))) { voidPoint = new Vector2(x, y); found = true; }
                Assert.IsTrue(found, "The arena has void.");
                var before = player.Health.CurrentHealth;
                player.transform.position = voidPoint;
                player.GetComponent<Rigidbody2D>().position = voidPoint;
                for (var i = 0; i < 50; i++) yield return new WaitForFixedUpdate();
                Assert.Less(player.Health.CurrentHealth, before - 1f, "Standing in the void costs health.");

                root.Shutdown();
                yield return null;
                Assert.IsNull(Object.FindAnyObjectByType<FieldVoidDamageDriver>(), "Shutdown removes the void damage driver.");
                Assert.IsNull(GameObject.Find("PlatformNetwork"), "Shutdown removes all platform art.");
            }
            finally
            {
                if (root != null && root.IsInitialized) root.Shutdown();
            }
        }
    }
}
