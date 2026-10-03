using System.Collections;
using System.Linq;
using Game.Character;
using Game.Content;
using Game.Meta;
using Game.Run;
using Game.Traps;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Game.Bootstrap.PlayModeTests
{
    /// <summary>DECISION-0156: the knights camp builds its traps in the real scene and a turret near the player fires.</summary>
    public sealed class ProductionField004TrapsSmokeTests
    {
        private static Transform ModelOf(TrapRuntimeDriver driver, TrapPlacement trap)
        {
            foreach (var candidate in driver.GetComponentsInChildren<Transform>(true))
                if (candidate.name == "TrapModel-" + trap.ModelKey &&
                    ((Vector2)candidate.parent.position - trap.Center).sqrMagnitude < .01f) return candidate;
            return null;
        }

        [UnityTest]
        public IEnumerator Field004_BuildsItsTraps_AndANearbyTurretWindsUpAndFires()
        {
            GameplayCompositionRoot root = null;
            try
            {
                var codec = new ProfileCodec(MetaCatalog.Load());
                var profile = codec.Create();
                profile.ClearedFields.Add("FIELD-001");
                profile.ClearedFields.Add("FIELD-002");
                profile.ClearedFields.Add("FIELD-003");
                profile.Unlocked.Add("FIELD-002");
                profile.Unlocked.Add("FIELD-004");
                var store = new MemoryProfileStore();
                store.WriteAsync(codec.Encode(profile)).GetAwaiter().GetResult();
                ProductionSmokeScene.Load(store);
                yield return null;
                yield return null;
                root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                var character = root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart);
                using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = character; character.SendEvent(submit); }
                root.UseReferenceSeeds = true;
                Assert.IsTrue(root.TryStartField(new ContentId("FIELD-004")));
                yield return null;

                var driver = GameObject.Find("FieldTraps")?.GetComponent<TrapRuntimeDriver>();
                Assert.IsNotNull(driver, "FIELD-004 builds its trap driver.");
                var runtime = driver.Runtime;
                Assert.Greater(runtime.Traps.Count, 60, "A trap or two on every screen of the 100-unit arena.");
                Assert.AreEqual(30, runtime.Barrels.Count);
                Assert.AreEqual(10, runtime.Barrels.Count(b => b.Explosive));

                // Bodies stop only the player: the contact collider excludes every layer but Player.
                var bodies = driver.GetComponentsInChildren<CircleCollider2D>(true);
                Assert.AreEqual(runtime.Traps.Count + runtime.Barrels.Count, bodies.Length, "Every turret and barrel has a body (far ones are switched off).");
                var playerMask = 1 << LayerMask.NameToLayer("Player");
                Assert.IsTrue(bodies.All(c => !c.isTrigger && c.excludeLayers == ~playerMask));

                // The start-screen cross is drawn with its 3D prefab: tilted, in front of the sprites, head turning with its pose.
                var startTrap = runtime.Traps.First(t => t.Type.Id.ToString() == "TRAP-002");
                var nearPlayer = Object.FindAnyObjectByType<PlayerCharacterRuntime>();
                var nearBody = nearPlayer.GetComponent<Rigidbody2D>();
                var nearSpot = startTrap.Center + new Vector2(0f, -2.5f);
                nearPlayer.transform.position = nearSpot;
                if (nearBody != null) { nearBody.position = nearSpot; nearBody.linearVelocity = Vector2.zero; }
                nearPlayer.Health.IsLocked = true;
                yield return null; yield return null;
                var model = ModelOf(driver, startTrap);
                Assert.IsNotNull(model, "A trap near the player carries the 3D model.");
                Assert.Greater(model.GetComponentsInChildren<MeshRenderer>().Length, 20);
                Assert.Less(model.localPosition.z, 0f, "Pushed towards the camera so ground sprites cannot cover it.");
                var head = model.Find("RotatingHead");
                Assert.AreEqual("trap-002", startTrap.ModelKey);
                yield return null;
                var before = head.localRotation;
                var spin = startTrap.HeadAngleDegrees;
                for (var i = 0; i < 30; i++) yield return null;
                Assert.AreNotEqual(spin, startTrap.HeadAngleDegrees, "The head turns while the trap rests.");
                Assert.AreNotEqual(before, head.localRotation, "The 3D head follows the simulated pose.");
                var camera = Camera.main;
                if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null && camera != null)
                {
                    var target = new RenderTexture(1600, 900, 24);
                    var previous = camera.targetTexture;
                    try
                    {
                        camera.targetTexture = target;
                        camera.Render();
                        UiFoundationSmokeTests.Capture(target, "field004-trap-start-screen");
                    }
                    finally { camera.targetTexture = previous; target.Release(); Object.DestroyImmediate(target); }
                }

                var player = Object.FindAnyObjectByType<PlayerCharacterRuntime>();
                player.Health.IsLocked = true;
                var run = Object.FindAnyObjectByType<RunController>();
                var trap = runtime.Traps.First(t => t.Type.Id.ToString() == "TRAP-005");
                var body = player.GetComponent<Rigidbody2D>();
                var spot = trap.Center + new Vector2(0f, 4.6f);
                player.transform.position = spot;
                if (body != null) { body.position = spot; body.linearVelocity = Vector2.zero; }
                var deadline = Time.realtimeSinceStartup + 25f;
                while (trap.VolleyIndex == 0 && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.AreEqual(RunState.Running, run.Model.State);
                Assert.IsTrue(trap.IsActive, "A turret with the player inside the radius is active.");
                Assert.GreaterOrEqual(trap.VolleyIndex, 1, "The turret wound up and fired.");
                // Projectiles are drawn nearer to the camera than the models, so they are never hidden behind a turret.
                var projectile = driver.transform.Find("Projectile-0");
                Assert.IsNotNull(projectile, "A projectile is in flight right after the shot.");
                var modelDepth = model.position.z;
                Assert.Less(projectile.position.z, modelDepth - 1f, "In front of every model.");
                var outline = projectile.Find("Outline")?.GetComponent<SpriteRenderer>();
                Assert.IsNotNull(outline, "Every trap projectile has a thin red outline.");
                Assert.IsNull(projectile.Find("Halo"), "No big discs any more.");
                var own = projectile.GetComponent<SpriteRenderer>();
                Assert.AreEqual(own.sprite, outline.sprite, "The outline copies the projectile's own silhouette.");
                Assert.Less(outline.sortingOrder, own.sortingOrder, "Behind the projectile itself.");
                Assert.AreEqual("SurvivorArena/SpriteDilatedOutline", outline.sharedMaterial.shader.name);
                Assert.Greater(outline.sharedMaterial.GetColor("_Color").r, .9f);
                Assert.Less(outline.sharedMaterial.GetColor("_Color").g, .2f);
                Assert.Greater(outline.sharedMaterial.GetColor("_Color").a, .8f, "Noticeable, not faint.");
                var widthPixels = outline.sharedMaterial.GetFloat("_WidthPixels");
                Assert.That(widthPixels, Is.InRange(1.5f, 4f), "Thin and noticeable: a few screen pixels at any window size.");
                Assert.AreEqual(1f, outline.transform.localScale.x, 1e-4f, "Not enlarged: the shader draws the rim.");
                // The projectile in flight with its red outline: a short moment after the shot, before it reaches the player.
                var flightEnds = Time.time + .22f;
                while (Time.time < flightEnds) yield return null;
                // Placeholder look in flight: rectangle turret and rectangle projectile of another colour.
                if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null && camera != null)
                {
                    var target = new RenderTexture(1600, 900, 24);
                    var previous = camera.targetTexture;
                    try
                    {
                        camera.targetTexture = target;
                        camera.Render();
                        UiFoundationSmokeTests.Capture(target, "field004-trap-volley");
                    }
                    finally { camera.targetTexture = previous; target.Release(); Object.DestroyImmediate(target); }
                }
                // Narrative size: shots leave from the muzzle of the model they are drawn with (captures of the biggest and the smallest guns).
                foreach (var id in new[] { "TRAP-001", "TRAP-007", "TRAP-009", "TRAP-011", "TRAP-012" })
                {
                    var gun = runtime.Traps.First(t => t.Type.Id.ToString() == id);
                    var gunSpot = gun.Center + new Vector2(0f, -2.2f);
                    player.transform.position = gunSpot;
                    if (body != null) { body.position = gunSpot; body.linearVelocity = Vector2.zero; }
                    var gunDeadline = Time.realtimeSinceStartup + 25f;
                    var volley = gun.VolleyIndex;
                    while (gun.VolleyIndex == volley && Time.realtimeSinceStartup < gunDeadline) yield return null;
                    var settle = Time.time + .12f;
                    while (Time.time < settle) yield return null;
                    if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null && camera != null)
                    {
                        var target = new RenderTexture(1600, 900, 24);
                        var previous = camera.targetTexture;
                        try
                        {
                            camera.targetTexture = target;
                            camera.Render();
                            UiFoundationSmokeTests.Capture(target, "field004-trap-muzzle-" + id);
                        }
                        finally { camera.targetTexture = previous; target.Release(); Object.DestroyImmediate(target); }
                    }
                }
                // A screen away from the start: the random scatter and the reduced obstacles.
                var away = new Vector2(-30f, -24f);
                player.transform.position = away;
                if (body != null) { body.position = away; body.linearVelocity = Vector2.zero; }
                for (var i = 0; i < 10; i++) yield return null;
                if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null && camera != null)
                {
                    var target = new RenderTexture(2400, 1350, 24);
                    var previous = camera.targetTexture;
                    try
                    {
                        camera.targetTexture = target;
                        camera.Render();
                        UiFoundationSmokeTests.Capture(target, "field004-trap-random-screen");
                    }
                    finally { camera.targetTexture = previous; target.Release(); Object.DestroyImmediate(target); }
                }
            }
            finally
            {
                if (root != null && root.IsInitialized) root.Shutdown();
            }
        }
    }
}
