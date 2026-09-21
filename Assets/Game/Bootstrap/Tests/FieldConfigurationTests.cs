using System;
using System.Linq;
using Game.Content;
using Game.Enemy;
using Game.Field;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Game.Bootstrap.Tests
{
    public sealed class FieldConfigurationTests
    {
        private FixtureRuntimeContentCatalog Catalog => FixtureRuntimeContentCatalog.Create();
        private FieldDefinition Copy(string environment = "FIXTURE-ENVIRONMENT-ARENA", string timeline = "FIXTURE-WAVE-TIMELINE",
            string final = "FIXTURE-BOSS-FINAL", string mid = "FIXTURE-BOSS-MID", string[] enemies = null, string traveler = null)
        {
            var source = Catalog.Fields.Roster.AllFields[0];
            return new FieldDefinition("FIXTURE-FIELD-TEST", "Test", "Description", "Preview pending", 2, "Fixture unlock",
                environment, timeline, final, (enemies ?? source.Enemies.Select(e => e.Id.ToString()).ToArray()).Select(id => new ContentId(id)),
                mid == null ? (ContentId?)null : new ContentId(mid), traveler == null ? (ContentId?)null : new ContentId(traveler));
        }
        [Test]
        public void Catalog_TwoFields_ResolveDifferentWaveEnemyAndBossConfiguration()
        {
            var fields = Catalog.Fields.Roster.AllFields;
            Assert.AreEqual(2, fields.Count);
            var first = fields[0].Resolve(Catalog.Registry);
            var second = fields[1].Resolve(Catalog.Registry);
            Assert.AreNotEqual(first.Timeline.Id, second.Timeline.Id);
            Assert.AreNotEqual(first.Timeline.Seed, second.Timeline.Seed);
            Assert.AreEqual(7, first.Enemies.Count);
            Assert.AreEqual(1, second.Enemies.Count);
            Assert.AreEqual(2, first.Bosses.Count);
            Assert.AreEqual(1, second.Bosses.Count);
            Assert.AreEqual(WaveHookKind.FinalBoss, second.Bosses[0].Hook);
            Assert.IsNull(second.Travelers);
            Assert.IsTrue(Catalog.Fields.Roster.TrySelect(Catalog.Fields.DefaultFieldId, out _));
            Assert.IsFalse(Catalog.Fields.Roster.TrySelect(fields[1].Id, out _));
            Assert.IsTrue(Catalog.SourceSnapshot.ContainsKey("Content/Fields/FixtureFields"));
            Assert.IsTrue(Catalog.SourceSnapshot.ContainsKey("Content/Waves/FixtureFieldWaveTimeline"));
        }
        [TestCase("MISSING")]
        [TestCase("FIXTURE-SKILL-BOLT")]
        public void Resolve_MissingOrWrongEnvironment_Rejects(string id) =>
            Assert.Throws<ContentNotFoundException>(() => Copy(environment: id).Resolve(Catalog.Registry));
        [TestCase("MISSING")]
        [TestCase("FIXTURE-ENEMY-SEEKER")]
        public void Resolve_MissingOrWrongTimeline_Rejects(string id) =>
            Assert.Throws<ContentNotFoundException>(() => Copy(timeline: id).Resolve(Catalog.Registry));
        [TestCase("MISSING")]
        [TestCase("FIXTURE-ENVIRONMENT-ARENA")]
        public void Resolve_MissingOrWrongBoss_Rejects(string id) =>
            Assert.Throws<ContentNotFoundException>(() => Copy(final: id).Resolve(Catalog.Registry));
        [Test]
        public void Resolve_EnemyOutsidePool_Rejects() => Assert.Throws<InvalidOperationException>(() =>
            Copy(enemies: new[] { "FIXTURE-ENEMY-SEEKER" }).Resolve(Catalog.Registry));
        [Test]
        public void Resolve_HookWithoutDefinition_Rejects() => Assert.Throws<InvalidOperationException>(() =>
            Copy(mid: null).Resolve(Catalog.Registry));
        [Test]
        public void Resolve_DefinitionWithoutHook_Rejects() => Assert.Throws<InvalidOperationException>(() =>
            Copy(timeline: "FIXTURE-WAVE-FOCUSED").Resolve(Catalog.Registry));
        [Test]
        public void Resolve_SwappedBossRoles_Rejects() => Assert.Throws<InvalidOperationException>(() =>
            Copy(final: "FIXTURE-BOSS-MID", mid: "FIXTURE-BOSS-FINAL").Resolve(Catalog.Registry));
        [Test]
        public void Resolve_TravelerRefToWrongType_Rejects() => Assert.Throws<ContentNotFoundException>(() =>
            Copy(traveler: "FIXTURE-WAVE-TIMELINE").Resolve(Catalog.Registry));
        [Test]
        public void Resolve_OptionalTravelerToken_IsTypedWithoutProductionBinding()
        {
            var token = new FieldTravelerScheduleDefinition("FIXTURE-TRAVELER-SCHEDULE");
            var field = Copy(traveler: token.Id.ToString());
            var definitions = new System.Collections.Generic.Dictionary<ContentId, IContentDefinition>();
            void Collect(IContentDefinition definition)
            {
                if (!definitions.TryAdd(definition.Id, definition)) return;
                if (definition is IReferencesContent references)
                    foreach (var reference in references.GetReferencedContent())
                        Collect(reference.Id == token.Id ? token : Catalog.Registry.Get<IContentDefinition>(reference.Id));
            }
            Collect(field);
            var registry = ContentRegistry.BuildFrom(definitions.Values);
            Assert.AreSame(token, field.Resolve(registry).Travelers);
        }
        [Test]
        public void Catalog_MissingDifficultyOrUnlock_RejectsExplicitly()
        {
            var json = Catalog.SourceSnapshot["Content/Fields/FixtureFields"];
            StringAssert.Contains("difficulty", Assert.Throws<InvalidOperationException>(() =>
                FixtureFieldCatalog.FromJson(json.Replace("\"difficulty\": 2,", ""))).Message);
            Assert.Throws<ArgumentException>(() => FixtureFieldCatalog.FromJson(
                json.Replace("Available in the fixture profile.", "")));
        }
        [Test]
        public void Environment_WrongCollisionMask_RejectsBeforeRun()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Single);
            var environment = Catalog.Fields.Environments[0];
            Assert.AreEqual("SpawnPoint", FieldEnvironmentBinding.Validate(environment, scene).name);
            var collider = GameObject.Find("Obstacle_Fixture").GetComponent<Collider2D>();
            var original = collider.excludeLayers;
            try
            {
                collider.excludeLayers = 0;
                Assert.Throws<InvalidOperationException>(() => FieldEnvironmentBinding.Validate(environment, scene));
                Assert.IsFalse(Object.FindAnyObjectByType<GameplayCompositionRoot>().IsInitialized);
            }
            finally { collider.excludeLayers = original; }
        }
    }
}
