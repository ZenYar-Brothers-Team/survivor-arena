using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Meta;
using Game.Run;
using NUnit.Framework;

namespace Game.Automation.Tests
{
    public sealed class AutomationPolicyTests
    {
        [Test]
        public async Task CheapestPersonalUpgrade_SpendsSavedCurrencyOnlyOncePerSuccess()
        {
            var catalog = MetaCatalog.Load();
            var codec = new ProfileCodec(catalog);
            var data = codec.Create();
            data.Currency = 350;
            var store = new MemoryProfileStore();
            await store.WriteAsync(codec.Encode(data));
            var profile = new ProfileService(catalog, store);
            await profile.LoadAsync();
            var policy = new AutomationPurchasePolicy(new PurchasePolicyData
            { AllowedUpgradeIds = new List<string> { "META-003" }, MaxPurchasesPerIntermission = 5 });
            var result = await policy.ExecuteAsync(profile, "CHAR-001");
            Assert.IsNull(result.Error);
            Assert.AreEqual(2, result.Purchases.Count);
            Assert.AreEqual(50, profile.Currency);
            Assert.AreEqual(2, profile.Level("META-003", "CHAR-001"));
            Assert.AreEqual(300, profile.Invested("CHAR-001"));
            Assert.AreEqual("Not enough currency", result.Refusals["META-003"]);
            var reloaded = new ProfileService(catalog, store);
            await reloaded.LoadAsync();
            Assert.AreEqual(2, reloaded.Level("META-003", "CHAR-001"));
            Assert.AreEqual(50, reloaded.Currency);
        }

        [Test]
        public async Task Route_AdvancesOnlyAfterVictoryAndReportsLockedOrUnplayable()
        {
            var catalog = MetaCatalog.Load();
            var codec = new ProfileCodec(catalog);
            var store = new MemoryProfileStore();
            await store.WriteAsync(codec.Encode(codec.Create()));
            var profile = new ProfileService(catalog, store);
            await profile.LoadAsync();
            var settings = new ExperimentConfigData { FieldRoute = new List<string> { "FIELD-001", "FIELD-002" },
                StopAfterRouteClear = true };
            var available = new[] { "FIELD-001", "FIELD-002" };
            Assert.AreEqual(0, AutomationRoutePolicy.NextIndex(0, RunCompletionReason.Defeat, settings, profile, available, out var reason));
            Assert.IsNull(reason);
            Assert.AreEqual(0, AutomationRoutePolicy.NextIndex(0, RunCompletionReason.Victory, settings, profile, available, out reason));
            Assert.AreEqual("routeBlocked:locked:FIELD-002", reason);
            var unlocked = codec.Create(); unlocked.Unlocked.Add("FIELD-002");
            var secondStore = new MemoryProfileStore(); await secondStore.WriteAsync(codec.Encode(unlocked));
            var second = new ProfileService(catalog, secondStore); await second.LoadAsync();
            Assert.AreEqual(0, AutomationRoutePolicy.NextIndex(0, RunCompletionReason.Victory, settings, second,
                new[] { "FIELD-001" }, out reason));
            Assert.AreEqual("routeBlocked:unplayable:FIELD-002", reason);
            Assert.AreEqual(1, AutomationRoutePolicy.NextIndex(0, RunCompletionReason.Victory, settings, second, available, out reason));
            Assert.IsNull(reason);
            Assert.AreEqual(1, AutomationRoutePolicy.NextIndex(1, RunCompletionReason.Victory, settings, second, available, out reason));
            Assert.AreEqual("routeCleared", reason);
        }
    }
}
