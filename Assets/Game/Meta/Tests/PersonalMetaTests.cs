using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
namespace Game.Meta.Tests
{
    public sealed class PersonalMetaTests
    {
        private static async Task<ProfileService> Create(FailingProfileStore store, long currency)
        {
            var catalog = MetaCatalog.Load(); var codec = new ProfileCodec(catalog); var data = codec.Create();
            data.Currency = currency; data.Unlocked.Add("CHAR-002"); store.Main = codec.Encode(data);
            var profile = new ProfileService(catalog, store); await profile.LoadAsync(); return profile;
        }
        [Test] public async Task AllTypes_BuyOne_ApplyExactChannelsOnlyToOwner()
        {
            var profile = await Create(new FailingProfileStore(), 100000);
            Assert.IsTrue(profile.Catalog.Upgrades.Values.All(u => u.Personal));
            Assert.IsFalse(profile.Catalog.Upgrades.ContainsKey("META-001"));
            Assert.IsFalse(profile.Catalog.Upgrades.ContainsKey("META-002"));
            foreach (var upgrade in profile.Catalog.Upgrades.Values)
            {
                Assert.IsTrue(await profile.PurchaseAsync(upgrade.Id, 0, "CHAR-001"));
                Assert.AreEqual(upgrade.PriceCoefficient * 2, upgrade.Price(1));
                Assert.AreEqual(upgrade.Stat == "rerolls" || upgrade.Stat == "banishes" ? 6 : 10, upgrade.Cap);
            }
            var m = profile.Modifier("CHAR-001");
            Assert.AreEqual(.05f, m.MaxHealthMultiplierBonus); Assert.AreEqual(.03f, m.ActiveSkillDamageMultiplierBonus);
            Assert.AreEqual(.04f, m.EffectSizeMultiplierBonus); Assert.AreEqual(.03f, m.ActionSpeedBonus);
            Assert.AreEqual(.05f, m.HealthRegenerationPerSecondBonus); Assert.AreEqual(.03f, m.MovementSpeedMultiplierBonus);
            Assert.AreEqual(.1f, m.PickupRadiusMultiplierBonus); Assert.AreEqual(.03f, m.PickedUpXpMultiplierBonus);
            Assert.AreEqual(.02f, m.IncomingDamageReductionBonus); Assert.AreEqual(.05f, m.HealthRestorationMultiplierBonus);
            Assert.AreEqual(1, profile.ExtraRerolls("CHAR-001")); Assert.AreEqual(1, profile.ExtraBanishes("CHAR-001"));
            Assert.AreEqual(default(Game.Character.CharacterStatModifier), profile.Modifier("CHAR-002"));
            Assert.AreEqual(0, profile.ExtraRerolls("CHAR-002")); Assert.AreEqual(0, profile.ExtraBanishes("CHAR-002"));
            var spent = profile.Invested("CHAR-001"); await profile.SetUpgradesDisabledAsync(true);
            Assert.AreEqual(default(Game.Character.CharacterStatModifier), profile.Modifier("CHAR-001"));
            Assert.AreEqual(0, profile.ExtraRerolls("CHAR-001")); Assert.AreEqual(0, profile.ExtraBanishes("CHAR-001"));
            Assert.AreEqual(spent, profile.Invested("CHAR-001"));
            Assert.IsTrue(await profile.PurchaseAsync("META-003", 1, "CHAR-001"));
        }
        [TestCase(999, false, 899)] [TestCase(1000, true, 0)] [TestCase(1001, true, 1)]
        public async Task Refund_Boundary_UsesWalletAndActualSpending(long initial, bool allowed, long expected)
        {
            var store = new FailingProfileStore(); var profile = await Create(store, initial);
            Assert.IsTrue(await profile.PurchaseAsync("META-003", 0, "CHAR-001"));
            Assert.AreEqual(100, profile.Invested("CHAR-001"));
            Assert.AreEqual(allowed, profile.RefundLockReason("CHAR-001") == null);
            Assert.AreEqual(allowed, await profile.RefundAsync("CHAR-001", 100));
            Assert.AreEqual(expected, profile.Currency); Assert.AreEqual(allowed ? 0 : 1, profile.Level("META-003", "CHAR-001"));
            Assert.IsFalse(await profile.RefundAsync("CHAR-001", 100));
            var loaded = new ProfileService(profile.Catalog, store); await loaded.LoadAsync();
            Assert.AreEqual(expected, loaded.Currency); Assert.AreEqual(allowed ? 0 : 100, loaded.Invested("CHAR-001"));
        }
        [Test] public async Task Refund_FailureStaleAndRun_LeaveAllStateUnchanged()
        {
            var store = new FailingProfileStore(); var profile = await Create(store, 10000);
            await profile.PurchaseAsync("META-003", 0, "CHAR-001"); await profile.PurchaseAsync("META-004", 0, "CHAR-002");
            await profile.SetUpgradesDisabledAsync(true);
            Assert.IsFalse(await profile.RefundAsync("CHAR-001", 99));
            profile.SetRunActive(true); Assert.IsFalse(await profile.RefundAsync("CHAR-001", 100)); profile.SetRunActive(false);
            store.Fail = true; Assert.IsFalse(await profile.RefundAsync("CHAR-001", 100));
            Assert.AreEqual(9800, profile.Currency); Assert.AreEqual(1, profile.Level("META-003", "CHAR-001"));
            store.Fail = false; store.Gate = new TaskCompletionSource<bool>();
            var pending = profile.RefundAsync("CHAR-001", 100);
            Assert.AreEqual(ProfileState.Saving, profile.State); Assert.IsFalse(await profile.RefundAsync("CHAR-001", 100));
            Assert.IsFalse(await profile.PurchaseAsync("META-003", 1, "CHAR-001"));
            store.Gate.SetResult(true); Assert.IsTrue(await pending);
            Assert.AreEqual(8900, profile.Currency); Assert.AreEqual(1, profile.Level("META-004", "CHAR-002"));
            Assert.AreEqual(100, profile.Invested("CHAR-002")); Assert.IsTrue(profile.IsUnlocked("CHAR-002")); Assert.IsTrue(profile.UpgradesDisabled);
        }
        [Test] public async Task PriceProgression_IndependentPerTypeAndHero_CountersCapAtSix()
        {
            var profile = await Create(new FailingProfileStore(), 100000);
            for (var i = 0; i < 6; i++) Assert.IsTrue(await profile.PurchaseAsync("META-009", i, "CHAR-001"));
            Assert.AreEqual(6, profile.ExtraRerolls("CHAR-001")); Assert.AreEqual(8400, profile.Invested("CHAR-001"));
            Assert.IsFalse(await profile.PurchaseAsync("META-009", 6, "CHAR-001"));
            var before = profile.Currency;
            Assert.IsTrue(await profile.PurchaseAsync("META-009", 0, "CHAR-002"));
            Assert.IsTrue(await profile.PurchaseAsync("META-010", 0, "CHAR-001"));
            Assert.AreEqual(before - 400 - 300, profile.Currency);
        }
    }
}
