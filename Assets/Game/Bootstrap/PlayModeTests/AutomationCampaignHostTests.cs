using System;
using System.Collections;
using System.IO;
using Game.Automation;
using Game.Bootstrap.Automation;
using Game.Meta;
using Game.Run;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class AutomationCampaignHostTests
    {
        [UnityTest]
        public IEnumerator Victory_UsesSavedUnlockToAdvanceToSecondField()
        {
            var catalog = MetaCatalog.Load();
            var store = new MemoryProfileStore();
            ProductionSmokeScene.Load(store, openSelection: false);
            yield return null;
            yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            Assert.IsTrue(root.AtMainMenu);
            var example = Path.GetFullPath(Path.Combine(Application.dataPath, "../scripts/balance/examples/fresh.json"));
            var data = JObject.Parse(File.ReadAllText(example));
            data["chains"] = 1;
            data["maxRunsPerChain"] = 2;
            data["fieldRoute"] = new JArray("FIELD-001", "FIELD-002");
            data["purchasePolicy"]["allowedUpgradeIds"] = new JArray();
            var outputRoot = Path.Combine(Application.temporaryCachePath, "automation-route-" + Guid.NewGuid().ToString("N"));
            var config = new ExperimentConfigLoader(catalog, new[] { "FIELD-001", "FIELD-002" }, outputRoot)
                .Parse(data.ToString(), Path.GetDirectoryName(example));
            var campaign = root.gameObject.AddComponent<AutomationCampaignHost>();
            campaign.Initialize(root, config, store, "chain-0001");
            for (var i = 0; i < 30 && !root.IsInitialized; i++) yield return null;
            Assert.IsTrue(root.IsInitialized, campaign.StopReason);
            var controller = Object.FindAnyObjectByType<RunController>();
            var firstId = controller.Model.RunId;
            // Fixture-only model time jump; pilot runs use natural time.
            controller.Model.Tick(controller.Model.Duration);
            for (var i = 0; i < 120 && controller.Model.RunId == firstId; i++) yield return null;
            Assert.AreNotEqual(firstId, controller.Model.RunId, campaign.StopReason);
            Assert.IsTrue(root.Profile.IsUnlocked("FIELD-002"));
            Assert.AreEqual("FIELD-002", root.FieldConfiguration.Field.Id.ToString());
            controller.Model.Kill();
            for (var i = 0; i < 100 && !campaign.IsFinished; i++) yield return null;
            Assert.AreEqual("maxRunsPerChain", campaign.StopReason);
            root.Shutdown();
            Object.Destroy(campaign);
        }

        [UnityTest]
        public IEnumerator TwoLosses_UseOneProfileAndPurchaseStrengthensSecondRun()
        {
            var catalog = MetaCatalog.Load();
            var codec = new ProfileCodec(catalog);
            var initial = codec.Create();
            initial.Currency = 100;
            var store = new MemoryProfileStore();
            store.WriteAsync(codec.Encode(initial)).GetAwaiter().GetResult();
            ProductionSmokeScene.Load(store, openSelection: false);
            yield return null;
            yield return null;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            Assert.IsTrue(root.AtMainMenu);
            var example = Path.GetFullPath(Path.Combine(Application.dataPath, "../scripts/balance/examples/fresh.json"));
            var data = JObject.Parse(File.ReadAllText(example));
            data["chains"] = 1;
            data["maxRunsPerChain"] = 2;
            data["purchasePolicy"]["maxPurchasesPerIntermission"] = 1;
            var outputRoot = Path.Combine(Application.temporaryCachePath, "automation-campaign-" + Guid.NewGuid().ToString("N"));
            var config = new ExperimentConfigLoader(catalog, new[] { "FIELD-001" }, outputRoot)
                .Parse(data.ToString(), Path.GetDirectoryName(example));
            var campaign = root.gameObject.AddComponent<AutomationCampaignHost>();
            campaign.Initialize(root, config, store, "chain-0001");
            for (var i = 0; i < 30 && !root.IsInitialized; i++) yield return null;
            Assert.IsTrue(root.IsInitialized, campaign.StopReason);
            var controller = Object.FindAnyObjectByType<RunController>();
            var firstId = controller.Model.RunId;
            controller.Model.Kill();
            for (var i = 0; i < 100 && campaign.StartedRuns < 2; i++) yield return null;
            Assert.AreEqual(2, campaign.StartedRuns, campaign.StopReason);
            for (var i = 0; i < 30 && controller.Model.RunId == firstId; i++) yield return null;
            Assert.AreEqual(1, root.Profile.Level("META-003", "CHAR-001"));
            Assert.AreEqual(5, root.Profile.Currency);
            Assert.AreNotEqual(firstId, controller.Model.RunId);
            var firstFolder = Path.Combine(config.OutputDirectory, "chains", "chain-0001", "runs", firstId.ToString("N"));
            Assert.IsTrue(File.Exists(Path.Combine(firstFolder, "profile-after-purchases.json")));
            var firstReport = JObject.Parse(File.ReadAllText(Path.Combine(firstFolder, "automation.json")));
            Assert.AreEqual(1, ((JArray)firstReport["purchases"]).Count);
            Assert.AreEqual(100, (long)firstReport["purchases"][0]["Price"]);
            controller.Model.Kill();
            for (var i = 0; i < 100 && !campaign.IsFinished; i++) yield return null;
            Assert.IsTrue(campaign.IsFinished);
            Assert.AreEqual("maxRunsPerChain", campaign.StopReason);
            Assert.AreEqual(1, root.Profile.Level("META-003", "CHAR-001"));
            root.Shutdown();
            Object.Destroy(campaign);
        }
    }
}
