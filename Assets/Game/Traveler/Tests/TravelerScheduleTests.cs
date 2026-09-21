using System;
using System.Linq;
using Game.Content;
using Game.Content.Json;
using Game.Traveler.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
namespace Game.Traveler.Tests
{
    public sealed class TravelerScheduleTests
    {
        private JObject Data => JObject.Parse(JsonContentFile.ReadText("Content/Travelers/FixtureTravelers"));
        [TestCase(0,0)] [TestCase(1,780)]
        public void TimeEndpoints_AllowSimultaneousSchedules_KeepCutoff(double sample,float expected)
        {
            var data=Data["schedules"][0].ToObject<TravelerScheduleData>(); data.CountProbabilities=new[]{0f,0f,0f,1f};
            var result=new TravelerScheduleDefinition(data).Draw(900,new TravelerEndpointRandom(sample));
            Assert.AreEqual(3,result.Count); Assert.AreEqual(3,result.Select(item=>item.Id).Distinct().Count());
            foreach(var item in result) Assert.AreEqual(expected,item.Time);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void CountDraw_IsDeterministic_WithoutReplacement_WithinCutoff(int count)
        {
            var data = Data["schedules"][0].ToObject<TravelerScheduleData>();
            data.CountProbabilities = Enumerable.Range(0,4).Select(i => i == count ? 1f : 0f).ToArray();
            var definition = new TravelerScheduleDefinition(data);
            var first = definition.Draw(900, new Random(37)); var second = definition.Draw(900, new Random(37));
            Assert.AreEqual(count, first.Count); Assert.AreEqual(count, first.Select(item => item.Id).Distinct().Count());
            CollectionAssert.AreEqual(first.Select(item => item.Time), second.Select(item => item.Time));
            CollectionAssert.AreEqual(first.Select(item => item.Id), second.Select(item => item.Id));
            foreach (var item in first) Assert.That(item.Time, Is.InRange(0,780));
        }
        [Test]
        public void Scaling_ChangesOnlyHealthAndDamage_ZeroStaysZero()
        {
            var data = Data["schedules"][0].ToObject<TravelerScheduleData>(); data.FieldRank = 5;
            var schedule = new TravelerScheduleDefinition(data);
            Assert.AreEqual(1.75f, schedule.Scale(390,900), .0001f);
            foreach (var definition in FixtureTravelerCatalog.Create().Definitions.Values)
            {
                var scaled = definition.Scale(1.75f);
                Assert.AreEqual(definition.Body.MaxHealth * 1.75f, scaled.MaxHealth);
                Assert.AreEqual(definition.Body.ContactDamage * 1.75f, scaled.ContactDamage);
                Assert.AreEqual(definition.Body.MovementSpeed, scaled.MovementSpeed);
                Assert.AreEqual(definition.Body.KnockbackResistance, scaled.KnockbackResistance);
                if (scaled.Attack != null) Assert.AreEqual(definition.Body.Attack.Damage * 1.75f, scaled.Attack.Damage);
            }
            Assert.Throws<ArgumentException>(() => schedule.Draw(120,new Random(1)));
        }
        [TestCase("countProbabilities")] [TestCase("fieldRank")] [TestCase("spawnScreenHeights")] [TestCase("placementAttempts")]
        public void RequiredScheduleFields_CannotBeMissing(string field)
        { var data=Data; ((JObject)data["schedules"][0]).Remove(field); Assert.Throws<ArgumentException>(() => FixtureTravelerCatalog.FromJson(data.ToString())); }
        [Test]
        public void InvalidProbabilitiesDuplicatesAndReferences_AreRejected()
        {
            var data=Data; data["schedules"][0]["countProbabilities"]=new JArray(0,0,0,0);
            Assert.Throws<ArgumentException>(() => FixtureTravelerCatalog.FromJson(data.ToString()));
            data=Data; data["schedules"][0]["travelerIds"][1]=data["schedules"][0]["travelerIds"][0].DeepClone();
            Assert.Throws<ArgumentException>(() => FixtureTravelerCatalog.FromJson(data.ToString()));
            data=Data; data["schedules"][0]["travelerIds"][0]="MISSING";
            Assert.Throws<ArgumentException>(() => FixtureTravelerCatalog.FromJson(data.ToString()));
            var catalog=FixtureTravelerCatalog.Create();
            Assert.DoesNotThrow(() => ContentRegistry.BuildFrom(catalog.Definitions.Values.Cast<IContentDefinition>().Concat(catalog.Schedules)));
        }
    }
}
