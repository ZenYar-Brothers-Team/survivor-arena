using System;
using Game.Character.Json;
using NUnit.Framework;

namespace Game.Character.Tests
{
    public sealed class CharacterBaseStatsMappingTests
    {
        [TestCase("MaxHealth")]
        [TestCase("MovementSpeed")]
        [TestCase("ActiveSkillDamageMultiplier")]
        [TestCase("ActiveSkillCooldownMultiplier")]
        [TestCase("IncomingDamageMultiplier")]
        [TestCase("HealthRestorationMultiplier")]
        [TestCase("HealthRegenerationPerSecond")]
        [TestCase("DisappearingXpRecovery")]
        [TestCase("PickedUpXpMultiplier")]
        [TestCase("XpDropLifetimeBonusSeconds")]
        [TestCase("PickupRadius")]
        [TestCase("KnockbackResistance")]
        [TestCase("OutgoingKnockbackBonus")]
        [TestCase("EffectSizeMultiplier")]
        [TestCase("EffectRangeMultiplier")]
        [TestCase("PotionDropMultiplier")]
        [TestCase("LowHealthDamageMaxBonus")]
        public void MissingRequiredField_ReportsItsName(string field)
        {
            var data = new CharacterBaseStatsData();
            foreach (var property in typeof(CharacterBaseStatsData).GetProperties()) property.SetValue(data, 1f);
            typeof(CharacterBaseStatsData).GetProperty(field).SetValue(data, null);
            var error = Assert.Throws<InvalidOperationException>(() => CharacterBaseStatsMapper.Map(data));
            StringAssert.Contains(field, error.Message);
        }

        [Test]
        public void Mapping_PreservesDistinctChannelValues()
        {
            var data = new CharacterBaseStatsData();
            foreach (var property in typeof(CharacterBaseStatsData).GetProperties()) property.SetValue(data, 1f);
            data.MaxHealth = 100f;
            data.KnockbackResistance = 0.4f;
            data.OutgoingKnockbackBonus = 0.6f;
            data.EffectSizeMultiplier = 1.2f;
            data.EffectRangeMultiplier = 1.3f;
            data.PotionDropMultiplier = 1.6f;
            data.LowHealthDamageMaxBonus = 0.7f;
            var value = CharacterBaseStatsMapper.Map(data);
            Assert.AreEqual(0.4f, value.KnockbackResistance);
            Assert.AreEqual(0.6f, value.OutgoingKnockbackBonus);
            Assert.AreEqual(1.2f, value.EffectSizeMultiplier);
            Assert.AreEqual(1.3f, value.EffectRangeMultiplier);
            Assert.AreEqual(1.6f, value.PotionDropMultiplier);
            Assert.AreEqual(0.7f, value.LowHealthDamageMaxBonus);
        }
    }
}
