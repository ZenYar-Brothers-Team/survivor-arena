using System.Linq;
using Game.Content;
using Game.Field;
using Game.Meta;
using NUnit.Framework;

namespace Game.Bootstrap.Tests
{
    /// <summary>DECISION-0142: academy preview runs under its permanent field ID with FIELD-001 encounters.</summary>
    public sealed class ProductionField006ContentTests
    {
        [Test]
        public void Academy_ResolvesSharedFirstFieldWaves_OwnGroundAndSeals()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            var academy = catalog.Fields.Roster.AllFields.Single(f => f.Id.ToString() == "FIELD-006");
            var one = catalog.Fields.Roster.AllFields.Single(f => f.Id.ToString() == "FIELD-001").Resolve(catalog.Registry);
            var resolved = academy.Resolve(catalog.Registry);
            Assert.IsFalse(academy.IsTest);
            Assert.AreEqual(3, academy.Difficulty);
            Assert.AreEqual("FIELD-006-VISUAL-BACKGROUND", academy.Thumbnail.Value.Id.ToString());
            Assert.AreSame(one.Timeline, resolved.Timeline, "Exact shared waves, including opening cadence, hooks and cap.");
            Assert.AreSame(one.Travelers, resolved.Travelers);
            CollectionAssert.AreEquivalent(one.Enemies.Select(e => e.Id), resolved.Enemies.Select(e => e.Id));
            CollectionAssert.AreEquivalent(one.Bosses.Select(b => b.Id), resolved.Bosses.Select(b => b.Id));
            var presentation = catalog.FieldEnvironmentPresentations[new ContentId("FIELD-006-ENVIRONMENT")];
            Assert.AreEqual("FIELD-006-VISUAL-GROUND", presentation.Ground.Id.ToString());
            Assert.IsNotNull(presentation.Ground.Resolve(catalog.Registry).Sprite);
            var layout = presentation.ZoneLayout;
            Assert.AreEqual(0, presentation.InteriorObstacleCount);
            Assert.IsNull(presentation.BlobLayout);
            Assert.AreEqual(0, presentation.DecorationChance);
            Assert.IsTrue(layout.SuppressAreaUnitFeedback);
            Assert.IsTrue(layout.Effects.Values.Any(e => e.RelocatesBetweenCycles));
            Assert.IsTrue(layout.Effects.Values.Any(e => !e.RelocatesBetweenCycles));
            Assert.AreEqual(14, layout.Zones.Count);
            Assert.IsFalse(layout.Effects.ContainsKey(new ContentId("FIELD-006-ZONE-HEAL")));
            var action = layout.Effects[new ContentId("FIELD-006-ZONE-ARCANE")];
            Assert.AreEqual(0f, action.PlayerSkillDamageBonus);
            Assert.AreEqual(.5f, action.PlayerActionSpeedBonus);
            Assert.AreEqual(2, layout.RandomSchedule.Chains);
            Assert.AreEqual(8f, layout.RandomSchedule.IntervalMinSeconds);
            Assert.AreEqual(16f, layout.RandomSchedule.IntervalMaxSeconds);
            foreach (var effect in layout.Effects.Values)
            {
                StringAssert.StartsWith("FIELD-006-ZONE-", effect.Id.ToString());
                Assert.IsTrue(effect.AffectsBothSides);
                Assert.AreEqual(.8f, effect.VerticalScale);
                Assert.AreNotEqual(Game.Zones.ZoneLifetimeMode.Permanent, effect.Lifetime);
                if (effect.Kind == Game.Zones.ZoneEffectKind.SpeedBurst)
                { Assert.AreEqual(1.25f, effect.Radius); Assert.AreEqual(effect.Radius, effect.MinRadius); }
                else Assert.AreEqual(effect.Radius / 3f, effect.MinRadius, .00001f);
                if (effect.Lifetime == Game.Zones.ZoneLifetimeMode.Pulsing)
                    Assert.AreEqual(5f, effect.PulsePrepareSeconds);
            }
        }

        [Test]
        public void Academy_LockedOnFreshProfile_DevUnlockPersistsAndAllowsSelection()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            var meta = MetaCatalog.Load(); var store = new MemoryProfileStore();
            var profile = new ProfileService(meta, store); profile.LoadAsync().GetAwaiter().GetResult();
            var roster = new FieldRoster(catalog.Fields.Roster.AllFields, new ProfileAccessProvider(profile));
            var id = new ContentId("FIELD-006");
            Assert.IsFalse(roster.TrySelect(id, out _));
            Assert.IsTrue(profile.UnlockAllForDevelopmentAsync("character", "field").GetAwaiter().GetResult());
            var restored = new ProfileService(meta, store); restored.LoadAsync().GetAwaiter().GetResult();
            Assert.IsTrue(new FieldRoster(catalog.Fields.Roster.AllFields, new ProfileAccessProvider(restored)).TrySelect(id, out var field));
            Assert.AreEqual(id, field.Id);
        }
    }
}
