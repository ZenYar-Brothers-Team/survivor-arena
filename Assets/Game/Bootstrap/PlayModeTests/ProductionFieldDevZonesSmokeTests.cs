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
    /// <summary>FIELD-DEV-ZONES (field-dev-zones-v1): the effect-zone test field starts on a fresh profile, ticks its zones and cleans up.</summary>
    public sealed class ProductionFieldDevZonesSmokeTests
    {
        [UnityTest]
        public IEnumerator ZonesField_StartsWithAllSixEffects_TicksWhileRunning_AndRemovesItselfOnShutdown()
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
                var codec = new ProfileCodec(MetaCatalog.Load());
                var store = new MemoryProfileStore();
                store.WriteAsync(codec.Encode(codec.Create())).GetAwaiter().GetResult();
                ProductionSmokeScene.Load(store);
                yield return null;
                yield return null;
                root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
                var character = root.SelectionDocument.rootVisualElement.Q<Button>(GameplayUiElementIds.CharacterSelectStart);
                using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = character; character.SendEvent(submit); }
                root.UseReferenceSeeds = true;
                Assert.IsTrue(root.TryStartField(new ContentId("FIELD-DEV-ZONES")), "The zones test field is always available.");
                yield return null;

                var run = Object.FindAnyObjectByType<RunController>();
                Assert.AreEqual("FIELD-DEV-ZONES", run.Model.Selection.FieldId.ToString());
                Assert.AreEqual(6, root.ZoneSeed, "Reference seeds pin the zone layout.");
                var driver = Object.FindAnyObjectByType<ZoneRuntimeDriver>();
                Assert.IsNotNull(driver, "The zone driver exists on a zones field.");
                var zones = driver.Runtime.Zones;
                Assert.AreEqual(11, zones.Count, "Two slows, two hastes, one spring, two arcane, two rifts and one portal pair.");
                CollectionAssert.AreEquivalent(new[] { ZoneEffectKind.Slow, ZoneEffectKind.Haste, ZoneEffectKind.Regeneration,
                    ZoneEffectKind.ArcanePower, ZoneEffectKind.Rift, ZoneEffectKind.Portal }, zones.Select(z => z.Effect.Kind).Distinct());
                Assert.IsTrue(zones.Any(z => z.Effect.Lifetime == ZoneLifetimeMode.Permanent) &&
                              zones.Any(z => z.Effect.Lifetime == ZoneLifetimeMode.Pulsing), "Both permanent and pulsing zones exist.");
                Assert.AreEqual(11, driver.GetComponentsInChildren<SpriteRenderer>(true).Length, "One placeholder disc per zone.");

                Object.FindAnyObjectByType<Game.Character.PlayerCharacterRuntime>().Health.IsLocked = true;
                for (var i = 0; i < 120; i++) yield return new WaitForFixedUpdate();
                Assert.AreEqual(RunState.Running, run.Model.State);
                Assert.Greater(driver.Runtime.Time, 0.5f, "Zones advance while the run is running.");

                root.Shutdown();
                yield return null;
                Assert.IsNull(Object.FindAnyObjectByType<ZoneRuntimeDriver>(), "Shutdown removes the zones.");
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
