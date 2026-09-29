using System;
using System.IO;
using System.Threading.Tasks;
using Game.Meta;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Game.Automation.Tests
{
    public sealed class ExperimentConfigTests
    {
        private string _root;
        private MetaCatalog _catalog;
        private ExperimentConfigLoader _loader;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "balance-config-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            _catalog = MetaCatalog.Load();
            _loader = new ExperimentConfigLoader(_catalog, new[] { "FIELD-001" }, _root);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        private JObject Fresh() => JObject.Parse(@"{
            'schemaVersion':1,'experimentId':'sample','template':'fresh','chains':2,'maxRunsPerChain':2,
            'runSpeed':1,'characterId':'CHAR-001','fieldRoute':['FIELD-001'],
            'movementPolicy':{'id':'safePickup','version':1,'decisionIntervalSeconds':0.2,
                'observationRadius':12,'predictionSeconds':0.5,'obstaclePadding':0.15,'stuckSeconds':3},
            'draftPolicy':{'id':'randomLegal','version':1},
            'purchasePolicy':{'id':'cheapestPersonalUpgrade','version':1,
                'allowedUpgradeIds':['META-003'],'maxPurchasesPerIntermission':2},
            'stopAfterRouteClear':false,'maxExperimentWallSeconds':600,
            'runWallTimeoutSeconds':1200,'transitionTimeoutSeconds':30,'outputDirectory':'sample'
        }");

        [Test]
        public void Load_ActiveFirst15DraftPolicy_IsAccepted()
        {
            var config = Fresh();
            config["draftPolicy"]["id"] = "activeFirst15";
            Assert.AreEqual("activeFirst15", _loader.Parse(config.ToString(), _root).Data.DraftPolicy.Id);
        }

        [Test]
        public async Task Load_FreshValid_IsImmutableAndMatchesCanonicalProfile()
        {
            var config = _loader.Parse(Fresh().ToString(), _root);
            config.Data.FieldRoute.Clear();
            Assert.AreEqual(1, config.Data.FieldRoute.Count);
            Assert.AreEqual(Path.Combine(_root, "sample"), config.OutputDirectory);
            var expected = new ProfileCodec(_catalog).Create();
            Assert.IsTrue(expected.Unlocked.Contains(config.Data.CharacterId));
            Assert.IsTrue(expected.Unlocked.Contains(config.Data.FieldRoute[0]));
            var store = await new IsolatedProfileFactory(_catalog, config).CreateChainStoreAsync();
            Assert.AreEqual(new ProfileCodec(_catalog).Encode(expected), await store.ReadAsync(false));
        }

        [Test]
        public void Load_UnknownFieldVersionAndOutputCollision_FailBeforeRun()
        {
            var value = Fresh(); value["surprise"] = true;
            Assert.Throws<Newtonsoft.Json.JsonSerializationException>(() => _loader.Parse(value.ToString(), _root));
            value = Fresh(); value["schemaVersion"] = 2;
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
            value = Fresh(); value["fieldRoute"][0] = "FIELD-999";
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
            Directory.CreateDirectory(Path.Combine(_root, "sample"));
            Assert.Throws<IOException>(() => _loader.Parse(Fresh().ToString(), _root));
        }

        [Test]
        public void Load_LockedCharacterFieldAndInvalidPolicy_FailBeforeRun()
        {
            var value = Fresh(); value["characterId"] = "CHAR-002";
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
            value = Fresh(); value["fieldRoute"] = new JArray("FIELD-002");
            var loaderWithSecondField = new ExperimentConfigLoader(_catalog, new[] { "FIELD-001", "FIELD-002" }, _root);
            Assert.Throws<ArgumentException>(() => loaderWithSecondField.Parse(value.ToString(), _root));
            value = Fresh(); value["movementPolicy"]["predictionSeconds"] = 100;
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
            value = Fresh(); value["purchasePolicy"]["allowedUpgradeIds"] = new JArray("META-999");
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
        }

        [Test]
        public void Load_ExperienceFocusedPolicy_IsSelectableButUnknownPolicyIsRejected()
        {
            var value = Fresh();
            value["movementPolicy"]["id"] = "experienceFocused";
            Assert.AreEqual("experienceFocused", _loader.Parse(value.ToString(), _root).Data.MovementPolicy.Id);
            value["movementPolicy"]["id"] = "unknown";
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
        }

        [Test]
        public void Load_OrbitExperience_RequiresBoundedArcOnlyForThatProfile()
        {
            var value = Fresh();
            value["movementPolicy"]["id"] = "orbitExperience";
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
            value["movementPolicy"]["arcOffsetWorldUnits"] = 6;
            Assert.AreEqual("orbitExperience", _loader.Parse(value.ToString(), _root).Data.MovementPolicy.Id);
            value["movementPolicy"]["arcOffsetWorldUnits"] = 11;
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
            value["movementPolicy"]["arcOffsetWorldUnits"] = 6;
            value["movementPolicy"]["version"] = 2;
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
            value["movementPolicy"]["version"] = 1;
            value["movementPolicy"]["id"] = "safePickup";
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
        }

        [Test]
        public void Load_HerdLoop_RequiresTypedSettingsAndKeepsOtherProfilesSeparate()
        {
            var value = Fresh();
            value["movementPolicy"]["id"] = "herdLoop";
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
            value["movementPolicy"]["arcOffsetWorldUnits"] = 6;
            value["movementPolicy"]["crowdMinEnemies"] = 8;
            value["movementPolicy"]["crowdRadius"] = 8;
            value["movementPolicy"]["lureSeconds"] = 7;
            value["movementPolicy"]["sweepSeconds"] = 5;
            value["movementPolicy"]["collectSeconds"] = 10;
            Assert.AreEqual("herdLoop", _loader.Parse(value.ToString(), _root).Data.MovementPolicy.Id);
            value["movementPolicy"]["id"] = "herdLoopAdaptive";
            Assert.AreEqual("herdLoopAdaptive", _loader.Parse(value.ToString(), _root).Data.MovementPolicy.Id);
            value["movementPolicy"]["id"] = "herdLoop";
            value["movementPolicy"]["crowdMinEnemies"] = 3;
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
            value["movementPolicy"]["crowdMinEnemies"] = 8;
            value["movementPolicy"]["id"] = "safePickup";
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
        }

        [Test]
        public void Load_TrajectorySearch_RequiresBoundedSettingsForItsOwnProfile()
        {
            var value = Fresh();
            value["movementPolicy"]["id"] = "trajectorySearch";
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
            value["movementPolicy"]["trajectory"] = JObject.Parse(@"{'horizonSeconds':8,'stepSeconds':0.2,
                'candidateCount':96,'contactPenalty':60,'clearance':0.25}");
            Assert.AreEqual("trajectorySearch", _loader.Parse(value.ToString(), _root).Data.MovementPolicy.Id);
            value["movementPolicy"]["trajectory"]["candidateCount"] = 10000;
            Assert.Throws<ArgumentOutOfRangeException>(() => _loader.Parse(value.ToString(), _root));
            value["movementPolicy"]["trajectory"]["candidateCount"] = 96;
            value["movementPolicy"]["id"] = "safePickup";
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
        }

        [Test]
        public void Load_Human_RequiresRecordingOneChainAndNormalSpeed()
        {
            var value = Fresh();
            value["movementPolicy"]["id"] = "human";
            value["chains"] = 1;
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
            value["demonstration"] = JObject.Parse(@"{'schemaVersion':1,'sampleIntervalSeconds':0.1,
                'maxSamples':100,'maxFileMegabytes':1,'queueCapacity':16,'maxEntitiesPerCollection':32}");
            Assert.AreEqual("human", _loader.Parse(value.ToString(), _root).Data.MovementPolicy.Id);
            value["runSpeed"] = 5;
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
            value["runSpeed"] = 1;
            value["chains"] = 2;
            Assert.Throws<ArgumentException>(() => _loader.Parse(value.ToString(), _root));
            value["movementPolicy"]["id"] = "safePickup";
            Assert.IsNotNull(_loader.Parse(value.ToString(), _root).Data.Demonstration);
            value["demonstration"]["queueCapacity"] = 0;
            Assert.Throws<ArgumentOutOfRangeException>(() => _loader.Parse(value.ToString(), _root));
            value["demonstration"]["queueCapacity"] = 16;
            value["demonstration"]["unknown"] = true;
            Assert.Throws<Newtonsoft.Json.JsonSerializationException>(() => _loader.Parse(value.ToString(), _root));
        }

        [Test]
        public void Preset_InvalidPersonalLevelsAndOwner_AreRejected()
        {
            var builder = new PresetProfileBuilder(_catalog);
            var declaration = JObject.Parse(@"{'currency':1000,'firstRun':true,'upgradesDisabled':false,
                'upgrades':{'META-003:CHAR-001':11},'unlocked':[],'clearedFields':[]}");
            Assert.Throws<ArgumentException>(() => builder.Build(declaration.ToString()));
            declaration["upgrades"]["META-003:CHAR-001"] = 1;
            declaration["upgrades"]["META-003:CHAR-002"] = 1;
            Assert.Throws<ArgumentException>(() => builder.Build(declaration.ToString()));
        }

        [Test]
        public async Task Preset_TwoChainsHaveIndependentStoresAndCapturedSource()
        {
            var builder = new PresetProfileBuilder(_catalog);
            var declaration = @"{'currency':1000,'firstRun':true,'upgradesDisabled':false,
                'upgrades':{'META-003:CHAR-001':2},'unlocked':[],'clearedFields':[]}";
            var serialized = builder.Build(declaration);
            var path = Path.Combine(_root, "initial.json");
            File.WriteAllText(path, serialized);
            var configData = Fresh(); configData["template"] = "preset"; configData["initialProfilePath"] = "initial.json";
            var config = _loader.Parse(configData.ToString(), _root);
            Assert.AreEqual(serialized, File.ReadAllText(path));
            File.WriteAllText(path, new ProfileCodec(_catalog).Encode(new ProfileCodec(_catalog).Create()));
            var factory = new IsolatedProfileFactory(_catalog, config);
            var first = await factory.CreateChainProfileAsync();
            var second = await factory.CreateChainProfileAsync();
            Assert.AreEqual(1000, first.Currency);
            Assert.AreEqual(2, first.Level("META-003", "CHAR-001"));
            Assert.IsTrue(await first.PurchaseAsync("META-003", 2, "CHAR-001"));
            Assert.AreEqual(1000, second.Currency);
            Assert.AreEqual(2, second.Level("META-003", "CHAR-001"));
            Assert.AreNotEqual(serialized, File.ReadAllText(path));
            var decoded = new ProfileCodec(_catalog).Decode(serialized);
            Assert.AreEqual(300, decoded.UpgradeSpending["META-003:CHAR-001"]);
            Assert.IsEmpty(decoded.Runs);
        }

        [Test]
        public async Task Isolation_PlayerSaveAndSettingsSentinels_AreNeverOpened()
        {
            var save = Path.Combine(_root, "profile-meta-r1.json");
            var settings = Path.Combine(_root, "settings-v1.json");
            File.WriteAllText(save, "player profile sentinel");
            File.WriteAllText(settings, "player settings sentinel");
            var factory = new IsolatedProfileFactory(_catalog, _loader.Parse(Fresh().ToString(), _root));
            var profile = await factory.CreateChainProfileAsync();
            var isolatedSettings = factory.CreateSettingsStore();
            await isolatedSettings.WriteAsync("automation settings");
            Assert.IsTrue(profile.CanStart);
            Assert.AreEqual("player profile sentinel", File.ReadAllText(save));
            Assert.AreEqual("player settings sentinel", File.ReadAllText(settings));
        }
    }
}
