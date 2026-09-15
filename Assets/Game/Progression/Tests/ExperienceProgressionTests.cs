using System;
using NUnit.Framework;

namespace Game.Progression.Tests
{
    public class ExperienceProgressionTests
    {
        [Test]
        public void Experience_FillsBarAndCarriesRemainderAcrossMultipleLevels()
        {
            var progression = new ExperienceProgression(5f, 10f, 20f);
            var levels = 0;
            progression.LevelUp += _ => levels++;

            var gained = progression.AddExperience(17f);

            Assert.AreEqual(2, gained);
            Assert.AreEqual(2, levels);
            Assert.AreEqual(3, progression.Level);
            Assert.AreEqual(2f, progression.CurrentExperience);
            Assert.AreEqual(20f, progression.RequiredExperience);
            Assert.AreEqual(0.1f, progression.Progress01, 0.0001f);
        }

        [Test]
        public void Thresholds_AreConfigurableAndValidated()
        {
            Assert.Throws<ArgumentException>(() => new ExperienceProgression());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ExperienceProgression(0f));

            var progression = new ExperienceProgression(2f);
            progression.AddExperience(6f);

            Assert.AreEqual(4, progression.Level);
        }
    }
}
