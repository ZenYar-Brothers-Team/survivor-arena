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
            Assert.AreEqual(30, presentation.InteriorObstacleCount, "Five of each of the six Academy props.");
            Assert.IsNotNull(presentation.BlobLayout);
            Assert.AreEqual(30, presentation.BlobLayout.Library.Count, "Numbered copies share the art of their kind.");
            Assert.AreEqual(6, presentation.BlobLayout.Library.Values.Select(item => item.Visual.Id.ToString()).Distinct().Count());
            Assert.IsTrue(presentation.BlobLayout.Library.Values.All(item => item.Visual.Id.ToString().StartsWith("FIELD-006-VISUAL-")),
                "Only the Academy's own obstacle art, nothing from the first map.");
            Assert.AreEqual(0, presentation.DecorationChance);
            Assert.IsTrue(layout.SuppressAreaUnitFeedback);
            Assert.IsTrue(layout.Effects.Values.All(e => e.RelocatesBetweenCycles), "No permanent placements: every effect appears on the schedule.");
            Assert.AreEqual(4 * 9, layout.Zones.Count, "Four spare placements for each kind (one per chain), including single burst portals.");
            Assert.IsFalse(layout.Effects.ContainsKey(new ContentId("FIELD-006-ZONE-HEAL")));
            Assert.IsFalse(layout.Effects.ContainsKey(new ContentId("FIELD-006-ZONE-HASTE")), "The wind current is removed.");
            var rift = layout.Effects[new ContentId("FIELD-006-ZONE-RIFT")];
            Assert.AreEqual(10f, rift.PlayerDamagePerSecond); Assert.AreEqual(10f, rift.EnemyDamagePerSecond);
            var portal = layout.Effects[new ContentId("FIELD-006-ZONE-PORTAL")];
            Assert.IsTrue(portal.IsBurstPortal); Assert.IsFalse(portal.IsScheduledPortalPair);
            Assert.AreEqual(5f, portal.PortalJumpDistance); Assert.IsFalse(portal.AffectsBothSides);
            Assert.AreEqual("#B780FF", "#" + UnityEngine.ColorUtility.ToHtmlStringRGB(portal.Color));
            Assert.AreEqual(5f, layout.Effects[new ContentId("FIELD-006-ZONE-EXPERIENCE")].PlayerExperienceMultiplier);
            var knockback = layout.Effects[new ContentId("FIELD-006-ZONE-KNOCKBACK")];
            Assert.AreEqual(5f, knockback.Radius, "Half of the 10-unit screen height."); Assert.IsFalse(knockback.AffectsBothSides);
            var action = layout.Effects[new ContentId("FIELD-006-ZONE-ARCANE")];
            Assert.AreEqual(0f, action.PlayerSkillDamageBonus);
            Assert.AreEqual(.5f, action.PlayerActionSpeedBonus);
            Assert.AreEqual(4, layout.RandomSchedule.Chains);
            Assert.AreEqual(.7f, layout.RandomSchedule.SpawnRadiusScreenWidths);
            Assert.IsFalse(layout.RandomSchedule.HasPortalChain, "DECISION-0153: portal competes on the general chains.");
            Assert.AreEqual(8f, layout.RandomSchedule.IntervalMinSeconds);
            Assert.AreEqual(16f, layout.RandomSchedule.IntervalMaxSeconds);
            foreach (var effect in layout.Effects.Values)
            {
                StringAssert.StartsWith("FIELD-006-ZONE-", effect.Id.ToString());
                var sharesSides = effect.Kind != Game.Zones.ZoneEffectKind.Portal && effect.Kind != Game.Zones.ZoneEffectKind.Experience &&
                                  effect.Kind != Game.Zones.ZoneEffectKind.Knockback;
                Assert.AreEqual(sharesSides, effect.AffectsBothSides, effect.Id.ToString());
                Assert.AreEqual(.8f, effect.VerticalScale);
                Assert.AreEqual(.7f, effect.ActiveRadiusFraction, "Works inside the middle of the wide rim band, not at its outer edge.");
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
