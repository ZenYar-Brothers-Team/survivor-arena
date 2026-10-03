using System.Collections;
using Game.Content;
using Game.Meta;
using Game.Movement;
using Game.Traps;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Game.Bootstrap.PlayModeTests
{
    /// <summary>
    /// Review capture (graphics runs only): the reference 3D cross (trap-002) next to each newer trap model on the first map's
    /// ground, drawn with the same tilt, depth and scale as in the game, one row of four per image in TestResults/trap-compare-N.png.
    /// </summary>
    public sealed class TrapModelComparisonCaptureTests
    {
        private static readonly string[][] Rows =
        {
            new[] { "trap-002", "trap-001", "trap-003", "trap-004" },
            new[] { "trap-002", "trap-005", "trap-006", "trap-007" },
            new[] { "trap-002", "trap-008", "trap-009", "trap-010" },
            new[] { "trap-002", "trap-011", "trap-012", "trap-013" }
        };

        [UnityTest]
        public IEnumerator ReferenceCross_NextToEveryNewerModel()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) Assert.Ignore("Needs a graphics device.");
            GameplayCompositionRoot root = null;
            GameObject holder = null;
            var camera = default(Camera);
            var follow = default(CameraFollowTarget);
            var previousSize = 0f;
            try
            {
                var codec = new ProfileCodec(MetaCatalog.Load());
                var store = new MemoryProfileStore();
                store.WriteAsync(codec.Encode(codec.Create())).GetAwaiter().GetResult();
                ProductionSmokeScene.Load(store);
                yield return null;
                yield return null;
                root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                var start = root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart);
                using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = start; start.SendEvent(submit); }
                root.UseReferenceSeeds = true;
                Assert.IsTrue(root.TryStartField(new ContentId("FIELD-001")));
                yield return null;

                var library = TrapPrefabLibrary.Load();
                Assert.IsNotNull(library);
                camera = Camera.main;
                follow = camera.GetComponent<CameraFollowTarget>();
                if (follow != null) follow.enabled = false;
                previousSize = camera.orthographicSize;
                camera.orthographicSize = 1.15f;
                var origin = new Vector3(40f, 40f, 0f);
                camera.transform.position = new Vector3(origin.x, origin.y + 0.1f, -10f);
                var row = 0;
                foreach (var keys in Rows)
                {
                    holder = new GameObject("Row " + row);
                    for (var i = 0; i < keys.Length; i++)
                    {
                        var prefab = library.Find(keys[i]);
                        Assert.IsNotNull(prefab, keys[i]);
                        var slot = new GameObject("Slot " + keys[i]).transform;
                        slot.SetParent(holder.transform, false);
                        slot.position = origin + new Vector3(-2.7f + i * 1.8f, 0f, 0f);
                        var model = Object.Instantiate(prefab, slot, false);
                        model.transform.localPosition = new Vector3(0f, 0f, -3f);
                        model.transform.localRotation = Quaternion.Euler(-54f, 0f, 0f);
                        model.transform.localScale = Vector3.one * 0.6f;
                    }
                    for (var f = 0; f < 3; f++) yield return null;
                    var target = new RenderTexture(2400, 700, 24);
                    var previous = camera.targetTexture;
                    try
                    {
                        camera.targetTexture = target;
                        camera.Render();
                        UiFoundationSmokeTests.Capture(target, "trap-compare-" + row);
                    }
                    finally { camera.targetTexture = previous; target.Release(); Object.DestroyImmediate(target); }
                    Object.DestroyImmediate(holder);
                    holder = null;
                    row++;
                }
            }
            finally
            {
                if (holder != null) Object.DestroyImmediate(holder);
                if (camera != null) camera.orthographicSize = previousSize;
                if (follow != null) follow.enabled = true;
                if (root != null && root.IsInitialized) root.Shutdown();
            }
        }
    }
}
