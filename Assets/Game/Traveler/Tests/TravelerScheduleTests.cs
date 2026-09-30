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
        private static TravelerRole ThreeRoles(ContentId id) =>
            id.ToString().Contains("BRUISER") || id.ToString().Contains("DASH") || id.ToString().Contains("CROSS") ? TravelerRole.Offensive
            : id.ToString().Contains("WANDER") || id.ToString().Contains("REST") ? TravelerRole.Wanderer : TravelerRole.Protector;
        private TravelerScheduleDefinition Schedule(int minCount, params float[] probabilities)
        {
            var data = Data["schedules"][0].ToObject<TravelerScheduleData>();
            data.MinCount = minCount; data.CountProbabilities = probabilities;
            return new TravelerScheduleDefinition(data);
        }
        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(8)] [TestCase(10)]
        public void TypeMix_EveryGroupOfThreeHasDistinctRoles_ForAnyCount(int count)
        {
            // DECISION-0122: 5 = 3 + 2, 8 = 3 + 3 + 2, the last shorter group still has distinct roles.
            var probabilities = new float[count + 1]; probabilities[count] = 1;
            var definition = Schedule(0, probabilities);
            for (var seed = 0; seed < 60; seed++)
            {
                var entries = definition.Draw(900, new Random(seed), ThreeRoles).OrderBy(e => e.Sequence).ToList();
                Assert.AreEqual(count, entries.Count, $"seed {seed}");
                for (var start = 0; start < count; start += 3)
                {
                    var roles = entries.Skip(start).Take(3).Select(e => ThreeRoles(e.Id)).ToList();
                    Assert.AreEqual(roles.Count, roles.Distinct().Count(), $"seed {seed}, group at {start}");
                }
            }
        }
        [Test]
        public void TypeMix_DoesNotRepeatATravelerBeforeItsTypeIsExhausted()
        {
            var definition = Schedule(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1); // eight fixture Travelers, three roles
            for (var seed = 0; seed < 40; seed++)
            {
                var entries = definition.Draw(900, new Random(seed), ThreeRoles).OrderBy(e => e.Sequence).ToList();
                foreach (var role in new[] { TravelerRole.Offensive, TravelerRole.Wanderer, TravelerRole.Protector })
                {
                    var ids = entries.Where(e => ThreeRoles(e.Id) == role).Select(e => e.Id).ToList();
                    var size = definition.TravelerIds.Count(id => ThreeRoles(id) == role);
                    foreach (var chunk in ids.Select((id, index) => (id, index)).GroupBy(x => x.index / size))
                        Assert.AreEqual(chunk.Count(), chunk.Select(x => x.id).Distinct().Count(), $"seed {seed}, {role}");
                }
            }
        }
        [Test]
        public void CountRange_UsesMinCountAndProbabilityIndex()
        {
            var definition = Schedule(1, .2f, .2f, .2f, .2f, .2f);
            Assert.AreEqual(1, definition.MinCount); Assert.AreEqual(5, definition.MaxCount);
            var seen = Enumerable.Range(0, 300).Select(seed => definition.Draw(900, new Random(seed), ThreeRoles).Count).Distinct().OrderBy(x => x).ToList();
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, seen);
        }
        [Test]
        public void ProductionSchedules_DrawOneToFiveTravelersOnEveryField()
        {
            var catalog = FixtureTravelerCatalog.CreateProduction();
            foreach (var schedule in catalog.Schedules)
            {
                Assert.AreEqual(1, schedule.MinCount, schedule.Id.ToString()); Assert.AreEqual(5, schedule.MaxCount, schedule.Id.ToString());
                for (var seed = 0; seed < 100; seed++)
                {
                    var entries = schedule.Draw(900, new Random(seed), id => catalog.Definitions[id].Role);
                    Assert.That(entries.Count, Is.InRange(1, 5));
                    var first = entries.OrderBy(e => e.Sequence).Take(3).Select(e => catalog.Definitions[e.Id].Role).ToList();
                    Assert.AreEqual(first.Count, first.Distinct().Count());
                }
            }
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void CountDraw_IsDeterministic_WithoutReplacement_WithinCutoff(int count)
        {
            var data = Data["schedules"][0].ToObject<TravelerScheduleData>(); data.MinCount = 0;
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
