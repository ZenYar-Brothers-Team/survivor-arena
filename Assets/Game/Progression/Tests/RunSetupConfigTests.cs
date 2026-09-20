using System;
using Game.Content;
using NUnit.Framework;

namespace Game.Progression.Tests
{
    public class RunSetupConfigTests
    {
        [Test]
        public void FixtureCatalog_ReadsFormerSceneFieldsFromConfig()
        {
            var setup = FixtureRunSetupCatalog.Create();

            Assert.AreEqual(new ContentId("FIXTURE-CHARACTER-AGILE"), setup.StartingCharacterId);
            Assert.AreEqual(3, setup.Draft.OfferCount);
            Assert.AreEqual(12345, setup.Draft.Seed);
            Assert.AreEqual(2, setup.Draft.InitialRerolls);
            Assert.AreEqual(2, setup.Draft.InitialBanishes);
            CollectionAssert.AreEqual(new[] { 5f, 10f, 15f }, setup.Experience.LevelThresholds);
            Assert.AreEqual(60f, setup.Experience.BaseDropLifetimeSeconds);
        }

        [Test]
        public void ExperienceSettings_RejectsMissingOrNonPositiveValues()
        {
            Assert.Throws<ArgumentException>(() => new ExperienceSettings(60f));
            Assert.Throws<ArgumentException>(() => new ExperienceSettings(60f, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExperienceSettings(0f, 5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExperienceSettings(float.NaN, 5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExperienceSettings(60f, 5f, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExperienceSettings(60f, -1f));
        }

        [Test]
        public void ExperienceSettings_CopiesThresholdsSoCallersCannotMutateThem()
        {
            var source = new[] { 5f, 10f };
            var settings = new ExperienceSettings(60f, source);

            source[0] = 999f;
            var copy = settings.CopyThresholds();
            copy[1] = 999f;

            CollectionAssert.AreEqual(new[] { 5f, 10f }, settings.LevelThresholds);
        }

        [Test]
        public void DraftSettings_RejectsInvalidCounts()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DraftSettings(0, 1, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DraftSettings(-1, 1, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DraftSettings(3, 1, -1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DraftSettings(3, 1, 0, -1));
        }

        [Test]
        public void DraftSettings_AcceptsZeroControlsAndAnySeed()
        {
            var settings = new DraftSettings(1, -7, 0, 0);

            Assert.AreEqual(-7, settings.Seed);
            Assert.AreEqual(0, settings.InitialRerolls);
            Assert.AreEqual(0, settings.InitialBanishes);
        }

        [Test]
        public void RunSetupConfig_RejectsDefaultIdAndMissingParts()
        {
            var draft = new DraftSettings(3, 1, 0, 0);
            var experience = new ExperienceSettings(60f, 5f);

            Assert.Throws<ArgumentException>(() => new RunSetupConfig(default, draft, experience));
            Assert.Throws<ArgumentNullException>(() => new RunSetupConfig("FIXTURE-CHARACTER-X", null, experience));
            Assert.Throws<ArgumentNullException>(() => new RunSetupConfig("FIXTURE-CHARACTER-X", draft, null));
        }
    }
}
