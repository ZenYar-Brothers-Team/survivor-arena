using System;
using System.Linq;
using System.Threading.Tasks;
using Game.Character;
using Game.Run;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
namespace Game.Meta.Tests
{
    public sealed class MetaProfileTests
    {
        private MetaCatalog _catalog;
        [SetUp] public void Setup() { _catalog = MetaCatalog.Load(); }
        [Test] public void ProductionCatalog_ApprovedValuesAndReferences_AreComplete()
        {
            Assert.AreEqual(70,_catalog.Unlocks.Count); Assert.AreEqual(4,_catalog.Upgrades.Count);
            Assert.AreEqual(5,_catalog.RewardPerLevel); Assert.AreEqual(50,_catalog.EmptyBookReward);
            Assert.AreEqual(900,_catalog.FieldClearSeconds);
            Assert.AreEqual(100,_catalog.Unlocks["CHAR-002"].Price);
            Assert.AreEqual(300,_catalog.Unlocks["CHAR-004"].Price);
            Assert.AreEqual(500,_catalog.Unlocks["CHAR-006"].Price);
            Assert.AreEqual(700,_catalog.Unlocks["CHAR-008"].Price);
        }
        [Test] public async Task Apply_ResultDuplicateAndReload_PayExactlyOnce()
        {
            var store=new MemoryProfileStore(); var profile=new ProfileService(_catalog,store); await profile.LoadAsync();
            var run=MetaTestData.Run();run.Start();run.Stop();
            Assert.IsTrue(await profile.ApplyAsync(run.Outcome,true)); Assert.AreEqual(200,profile.Currency);
            Assert.IsTrue(await profile.ApplyAsync(run.Outcome,true)); Assert.AreEqual(200,profile.Currency);
            var loaded=new ProfileService(_catalog,store);await loaded.LoadAsync();
            Assert.IsTrue(await loaded.ApplyAsync(run.Outcome,true)); Assert.AreEqual(200,loaded.Currency);
            Assert.AreEqual(100,loaded.LastReceipt.BookReward);
            loaded.LastReceipt.NewUnlocks.Add("mutated"); Assert.IsFalse(loaded.LastReceipt.NewUnlocks.Contains("mutated"));
        }
        [TestCase(RunCompletionReason.Aborted)] [TestCase(RunCompletionReason.Retry)] [TestCase(RunCompletionReason.Error)]
        public async Task Exit_StartedAtLevelOne_PaysFive(RunCompletionReason reason)
        {
            var profile=new ProfileService(_catalog,new MemoryProfileStore());await profile.LoadAsync();
            var run=MetaTestData.Run(1,0);using var binding=new ProfileRunBinding(run,profile);
            run.Start();run.Pause();run.Stop(reason);await binding.SaveTask;
            Assert.AreEqual(5,profile.Currency); Assert.IsTrue(profile.CanStart);
        }
        [Test] public async Task Exit_NotStarted_DoesNotPayOrUnlockFirstRun()
        {
            var profile=new ProfileService(_catalog,new MemoryProfileStore());await profile.LoadAsync();
            var run=MetaTestData.Run();using var binding=new ProfileRunBinding(run,profile);run.Stop();await binding.SaveTask;
            Assert.AreEqual(0,profile.Currency); Assert.AreEqual("Survive 15:00 on FIELD-001",profile.PurchaseLockReason("CHAR-002"));
        }
        [Test] public async Task Victory_RealFifteenMinutes_UnlocksFieldSkillSetTogether()
        {
            var profile=new ProfileService(_catalog,new MemoryProfileStore());await profile.LoadAsync();
            var run=MetaTestData.Run();run.Start();run.Pause();run.Tick(900);Assert.AreEqual(0,run.Elapsed);run.Resume();run.Tick(900);
            await profile.ApplyAsync(run.Outcome,true);
            // DECISION-0050 FIELD-001 pack; SKILL-016 now waits for FIELD-002.
            foreach(var id in new[]{"FIELD-002","SKILL-008","PASSIVE-013","SET-002","SET-003","SET-011","SET-016","SET-020"}) Assert.IsTrue(profile.IsUnlocked(id),id);
            Assert.IsFalse(profile.IsUnlocked("SKILL-016"));
            Assert.IsFalse(profile.IsUnlocked("CHAR-002"));Assert.IsNull(profile.PurchaseLockReason("CHAR-002"));
        }
        [Test] public async Task Victory_ShortFixture_DoesNotClearField()
        {
            var profile=new ProfileService(_catalog,new MemoryProfileStore());await profile.LoadAsync();
            var run=MetaTestData.Run(duration:1);run.Start();run.Tick(1);await profile.ApplyAsync(run.Outcome,true);
            Assert.IsFalse(profile.IsUnlocked("FIELD-002"));
        }
        [Test] public async Task DeathBeforeDeadline_LaterTickAndQuit_DoNotChangeOutcome()
        {
            var profile=new ProfileService(_catalog,new MemoryProfileStore());await profile.LoadAsync();
            var run=MetaTestData.Run();run.Start();run.Tick(899);run.Kill();run.Tick(1);run.Stop();await profile.ApplyAsync(run.Outcome,true);
            Assert.AreEqual(RunCompletionReason.Defeat,run.Outcome.Reason);Assert.IsFalse(profile.IsUnlocked("FIELD-002"));Assert.AreEqual(200,profile.Currency);
        }
        [Test] public async Task Purchase_InsufficientLockedDuplicateAndCap_DoNotOverspend()
        {
            var store=new MemoryProfileStore();var profile=new ProfileService(_catalog,store);await profile.LoadAsync();
            Assert.IsFalse(await profile.PurchaseAsync("META-001",0));Assert.IsFalse(await profile.PurchaseAsync("META-003",0,"CHAR-002"));
            var lost=MetaTestData.Run(1,0);lost.Start();lost.Stop();await profile.ApplyAsync(lost.Outcome,true);
            Assert.IsFalse(await profile.PurchaseAsync("CHAR-002",0),"A finished run no longer opens CHAR-002 (DECISION-0050).");
            var run=MetaTestData.Run(999,0);run.Start();run.Tick(900);await profile.ApplyAsync(run.Outcome,true);
            Assert.IsTrue(await profile.PurchaseAsync("CHAR-002",0));Assert.IsFalse(await profile.PurchaseAsync("CHAR-002",0));
            for(var n=0;n<5;n++)Assert.IsTrue(await profile.PurchaseAsync("META-001",n));
            Assert.IsFalse(await profile.PurchaseAsync("META-001",4));Assert.IsFalse(await profile.PurchaseAsync("META-001",5));
            Assert.AreEqual(3400,profile.Currency);
            var loaded=new ProfileService(_catalog,store);await loaded.LoadAsync();Assert.AreEqual(5,loaded.Level("META-001"));Assert.IsTrue(loaded.IsUnlocked("CHAR-002"));
        }
        [Test] public async Task NewProductionProfile_StartsWithExactlyTheStartupSet()
        {
            var profile=new ProfileService(_catalog,new MemoryProfileStore());await profile.LoadAsync();
            string[] Open(string kind)=>_catalog.Unlocks.Values.Where(r=>r.Kind==kind&&profile.IsUnlocked(r.Id)).Select(r=>r.Id).OrderBy(i=>i).ToArray();
            CollectionAssert.AreEqual(new[]{"SKILL-001","SKILL-002","SKILL-003","SKILL-004","SKILL-005","SKILL-006","SKILL-007","SKILL-010","SKILL-013","SKILL-014"},Open("skill"));
            CollectionAssert.AreEqual(new[]{"PASSIVE-001","PASSIVE-002","PASSIVE-003","PASSIVE-004","PASSIVE-005","PASSIVE-007","PASSIVE-008","PASSIVE-009","PASSIVE-011","PASSIVE-012"},Open("passive"));
            CollectionAssert.AreEqual(new[]{"SET-001","SET-004","SET-006","SET-010","SET-017"},Open("set"));
            CollectionAssert.AreEqual(new[]{"CHAR-001"},Open("character"));CollectionAssert.AreEqual(new[]{"FIELD-001"},Open("field"));
            Assert.AreEqual(0,profile.Currency);
        }
        [Test] public async Task PoolGrowth_FollowsDecision0050Stages()
        {
            var profile=new ProfileService(_catalog,new MemoryProfileStore());await profile.LoadAsync();
            int Count(string kind)=>_catalog.Unlocks.Values.Count(r=>r.Kind==kind&&profile.IsUnlocked(r.Id));
            var expected=new[]{(11,11,10),(13,13,13),(14,14,18),(16,14,20)};
            for(var i=0;i<4;i++)
            {
                var run=MetaTestData.Run(1,0,"FIELD-"+(i+1).ToString("000"));run.Start();run.Tick(900);await profile.ApplyAsync(run.Outcome,true);
                Assert.AreEqual(expected[i],(Count("skill"),Count("passive"),Count("set")),"after FIELD-"+(i+1).ToString("000"));
            }
        }
        [Test] public async Task Load_SavedClearWithoutNewUnlock_GrantsItOnceWithoutCurrency()
        {
            var codec=new ProfileCodec(_catalog);var data=codec.Create();
            data.ClearedFields.Add("FIELD-001");data.Currency=42;data.Unlocked.Add("SKILL-012");// old DECISION-0037 initial unlock stays
            var store=new FailingProfileStore{Main=codec.Encode(data)};
            var profile=new ProfileService(_catalog,store);await profile.LoadAsync();
            Assert.AreEqual(ProfileState.Ready,profile.State);
            foreach(var id in new[]{"FIELD-002","SKILL-008","SET-020"}) Assert.IsTrue(profile.IsUnlocked(id),id);
            Assert.IsTrue(profile.IsUnlocked("SKILL-012"),"Existing unlocks are never revoked.");
            Assert.AreEqual(42,profile.Currency);Assert.AreEqual(1,store.Writes,"Migrated unlocks are persisted once.");
            var again=new ProfileService(_catalog,store);await again.LoadAsync();Assert.AreEqual(1,store.Writes);
        }
        [Test] public async Task Modifiers_GlobalAndPersonal_AddWithoutMutatingPreviousRun()
        {
            var profile=new ProfileService(_catalog,new MemoryProfileStore());await profile.LoadAsync();
            var run=MetaTestData.Run(1000,0);run.Start();run.Stop();await profile.ApplyAsync(run.Outcome,true);
            var prior=profile.Modifier("CHAR-001");
            await profile.PurchaseAsync("META-001",0);await profile.PurchaseAsync("META-003",0,"CHAR-001");
            Assert.AreEqual(0,prior.MaxHealthMultiplierBonus);Assert.AreEqual(.10f,profile.Modifier("CHAR-001").MaxHealthMultiplierBonus,.0001);
            Assert.AreEqual(.05f,profile.Modifier("CHAR-002").MaxHealthMultiplierBonus,.0001);
            profile.SetRunActive(true);Assert.IsFalse(await profile.PurchaseAsync("META-001",1));profile.SetRunActive(false);
        }
        [Test] public async Task DamageUpgrades_BothCaps_TotalThirtyPercentAndCorrectPrice()
        {
            var profile=new ProfileService(_catalog,new MemoryProfileStore());await profile.LoadAsync();
            var run=MetaTestData.Run(1000,0);run.Start();run.Stop();await profile.ApplyAsync(run.Outcome,true);
            for(var n=0;n<5;n++) { Assert.IsTrue(await profile.PurchaseAsync("META-002",n)); Assert.IsTrue(await profile.PurchaseAsync("META-004",n,"CHAR-001")); }
            Assert.AreEqual(.30f,profile.Modifier("CHAR-001").ActiveSkillDamageMultiplierBonus,.0001);
            Assert.AreEqual(.15f,profile.Modifier("CHAR-002").ActiveSkillDamageMultiplierBonus,.0001);
            Assert.AreEqual(2750,profile.Currency);
            Assert.IsFalse(await profile.PurchaseAsync("META-004",5,"CHAR-001"));
        }
        [Test] public async Task FieldChain_AllCharacterConditionsAndPurchasesResolve()
        {
            var profile=new ProfileService(_catalog,new MemoryProfileStore());await profile.LoadAsync();
            for(var i=1;i<=9;i++)
            {
                Assert.IsTrue(profile.IsUnlocked("FIELD-"+i.ToString("000")));
                var run=MetaTestData.Run(1000,0,"FIELD-"+i.ToString("000"));run.Start();run.Tick(900);await profile.ApplyAsync(run.Outcome,true);
            }
            Assert.IsTrue(profile.IsUnlocked("FIELD-010"));
            foreach(var id in new[]{"CHAR-003","CHAR-005","CHAR-007","CHAR-009","CHAR-010"})Assert.IsTrue(profile.IsUnlocked(id),id);
            foreach(var id in new[]{"CHAR-002","CHAR-004","CHAR-006","CHAR-008"}) { Assert.IsFalse(profile.IsUnlocked(id)); Assert.IsTrue(await profile.PurchaseAsync(id,0)); }
        }
        [Test] public async Task SaveFailure_PendingResultBlocksProgressAndRetriesWithoutDuplicate()
        {
            var store=new FailingProfileStore();var profile=new ProfileService(_catalog,store);await profile.LoadAsync();store.Fail=true;
            var run=MetaTestData.Run();run.Start();run.Stop();Assert.IsFalse(await profile.ApplyAsync(run.Outcome,true));
            Assert.AreEqual(ProfileState.PendingResult,profile.State);Assert.IsFalse(profile.CanStart);Assert.AreEqual(0,profile.Currency);
            Assert.IsFalse(await profile.PurchaseAsync("META-001",0));store.Fail=false;Assert.IsTrue(await profile.RetrySaveAsync());
            Assert.AreEqual(200,profile.Currency);await profile.ApplyAsync(run.Outcome,true);Assert.AreEqual(2,store.Writes);
        }
        [Test] public async Task PurchaseFailure_RollsBackBalanceAndLevel()
        {
            var store=new FailingProfileStore();var profile=new ProfileService(_catalog,store);await profile.LoadAsync();
            var run=MetaTestData.Run();run.Start();run.Stop();await profile.ApplyAsync(run.Outcome,true);store.Fail=true;
            Assert.IsFalse(await profile.PurchaseAsync("META-001",0));Assert.AreEqual(200,profile.Currency);Assert.AreEqual(0,profile.Level("META-001"));
        }
        [Test] public async Task InFlightSave_RejectsSecondIntent()
        {
            var store=new FailingProfileStore();var profile=new ProfileService(_catalog,store);await profile.LoadAsync();
            var run=MetaTestData.Run();run.Start();run.Stop();await profile.ApplyAsync(run.Outcome,true);
            store.Gate=new TaskCompletionSource<bool>();var first=profile.PurchaseAsync("META-001",0);
            Assert.AreEqual(ProfileState.Saving,profile.State);Assert.IsFalse(await profile.PurchaseAsync("META-001",0));
            store.Gate.SetResult(true);Assert.IsTrue(await first);Assert.AreEqual(100,profile.Currency);
        }
        [Test] public async Task IncompleteOutcome_IsNotAZeroPayout()
        {
            var profile=new ProfileService(_catalog,new MemoryProfileStore());await profile.LoadAsync();var run=new RunModel();run.Start();run.Stop();
            Assert.IsFalse(await profile.ApplyAsync(run.Outcome,true));Assert.AreEqual(ProfileState.PendingResult,profile.State);Assert.IsFalse(profile.CanStart);
        }
        [Test] public async Task Load_CorruptMain_RecoversValidBackupAndPreservesFiles()
        {
            var codec=new ProfileCodec(_catalog);var store=new FailingProfileStore {Main="broken",Backup=codec.Encode(codec.Create())};
            var profile=new ProfileService(_catalog,store);await profile.LoadAsync();Assert.AreEqual(ProfileState.Ready,profile.State);Assert.AreEqual(1,store.Preserved);
            StringAssert.Contains("Recovered backup",profile.Message);
        }
        [Test] public async Task Load_CorruptBoth_ResetIsExplicit()
        {
            var store=new FailingProfileStore { Main="broken",Backup="broken" };var profile=new ProfileService(_catalog,store);
            await profile.LoadAsync();Assert.AreEqual(ProfileState.LoadError,profile.State);Assert.AreEqual(0,store.Writes);
            await profile.ResetAsync();Assert.AreEqual(ProfileState.Ready,profile.State);Assert.AreEqual(1,store.Preserved);
        }
        [TestCase(2)] [TestCase(0)] public async Task Load_UnknownVersion_DoesNotReplaceWithBackupOrReset(int version)
        {
            var codec=new ProfileCodec(_catalog);var json=JObject.Parse(codec.Encode(codec.Create()));json["schemaVersion"]=version;
            var store=new FailingProfileStore {Main=json.ToString(),Backup=codec.Encode(codec.Create())};var profile=new ProfileService(_catalog,store);
            await profile.LoadAsync();await profile.ResetAsync();Assert.AreEqual(ProfileState.LoadError,profile.State);Assert.AreEqual(0,store.Writes);
        }
        [Test] public async Task Load_KnownMigration_PreservesSourceAndWritesCurrentVersion()
        {
            var codec=new ProfileCodec(_catalog);var json=JObject.Parse(codec.Encode(codec.Create()));json["schemaVersion"]=0;
            var store=new FailingProfileStore {Main=json.ToString()};var profile=new ProfileService(_catalog,store,new[]{new SyntheticProfileMigration()});
            await profile.LoadAsync();Assert.AreEqual(ProfileState.Ready,profile.State);Assert.AreEqual(0,(int)JObject.Parse(store.Backup)["schemaVersion"]);
            Assert.AreEqual(1,(int)JObject.Parse(store.Main)["schemaVersion"]);
        }
    }
}
