using NUnit.Framework;

namespace Game.Character.Tests
{
    public class FixtureCharacterCatalogTests
    {
        [Test]
        public void CreateDefault_LoadsConfiguredBaseStatsFromJson()
        {
            var baseStats = FixtureCharacterCatalog.CreateDefault();

            Assert.Greater(baseStats.MaxHealth, 0f);
            Assert.Greater(baseStats.MovementSpeed, 0f);
            Assert.AreEqual(1f, baseStats.ActiveSkillDamageMultiplier);
            Assert.AreEqual(1f, baseStats.ActiveSkillCooldownMultiplier);
            Assert.AreEqual(0f, baseStats.DisappearingXpRecovery);
        }

        [Test]
        public void Create_UnknownId_Throws()
        {
            Assert.Throws<System.InvalidOperationException>(
                () => FixtureCharacterCatalog.Create("NOT-A-REAL-CHARACTER"));
        }
    }
}
