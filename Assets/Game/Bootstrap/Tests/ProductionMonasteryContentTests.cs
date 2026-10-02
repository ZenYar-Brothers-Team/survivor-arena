using System.Linq;
using Game.Content;
using Game.Presentation;
using Game.Zones;
using NUnit.Framework;
using UnityEngine;

namespace Game.Bootstrap.Tests
{
    public sealed class ProductionMonasteryContentTests
    {
        [Test]
        public void Field007_ApprovedPreview_HasAssignedGroundAndBalancedThirtySixAltars()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            var field = catalog.Fields.Roster.AllFields.Single(f => f.Id.ToString() == "FIELD-007");
            var config = field.Resolve(catalog.Registry);
            Assert.AreEqual("FIELD-001-TIMELINE", config.Timeline.Id.ToString());
            var presentation = catalog.FieldEnvironmentPresentations[config.Environment.Id];
            Assert.AreEqual("FIELD-007-VISUAL-GROUND", presentation.Ground.Id.ToString());
            Assert.AreEqual(160f, presentation.ArenaSideLength);
            Assert.AreEqual(0, presentation.InteriorObstacleCount);
            var layout = presentation.ZoneLayout;
            Assert.AreEqual(36, layout.Zones.Count);
            Assert.AreEqual(3, layout.MaxPerScreen);
            Assert.AreEqual(12, layout.Zones.Count(e => e.Kind == ZoneEffectKind.Shrine));
            Assert.AreEqual(12, layout.Zones.Count(e => e.Kind != ZoneEffectKind.Shrine && e.Polarity == ZoneAltarPolarity.Positive));
            Assert.AreEqual(12, layout.Zones.Count(e => e.Polarity == ZoneAltarPolarity.Negative));
            Assert.IsTrue(layout.Zones.All(e => e.Kind != ZoneEffectKind.Charge && e.Polarity != ZoneAltarPolarity.Neutral));
            Assert.IsTrue(layout.Zones.Where(e => e.Kind != ZoneEffectKind.Shrine).All(e => e.Lifetime == ZoneLifetimeMode.Cycling));
            Assert.IsTrue(layout.Zones.Where(e => e.Polarity == ZoneAltarPolarity.Negative).All(e => !e.HarmsEnemies));
            foreach (var group in layout.Zones.GroupBy(e => e.Kind == ZoneEffectKind.Shrine ? 0 : e.Polarity == ZoneAltarPolarity.Positive ? 1 : 2))
            {
                var counts = group.GroupBy(e => e.Id).Select(e => e.Count()).ToArray();
                Assert.LessOrEqual(counts.Max() - counts.Min(), 1);
            }
            foreach (var reference in presentation.AltarPresentation.GetReferencedContent())
                catalog.Registry.Get<SpriteDefinition>(reference.Id).RequireRole(SpriteRole.Prop);
        }

        [Test]
        public void AltarView_NegativeAndShrine_UseOnlyPolaritySpritesAndPausedStateIsStable()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            var presentation = catalog.FieldEnvironmentPresentations[new ContentId("FIELD-007-ENVIRONMENT")];
            foreach (var effect in presentation.ZoneLayout.Effects.Values)
            {
                var obj = new GameObject("Altar test");
                try
                {
                    var zone = new ZonePlacement(0, effect, new Vector2(3, 4), 4f);
                    zone.SetNear(true);
                    var view = obj.AddComponent<AltarPresentationRuntime>();
                    view.Initialize(zone, presentation.AltarPresentation, catalog.Registry);
                    view.Apply(0f);
                    var expected = effect.Kind == ZoneEffectKind.Shrine ? presentation.AltarPresentation.Shrine :
                        effect.Polarity == ZoneAltarPolarity.Positive ? presentation.AltarPresentation.Positive : presentation.AltarPresentation.Negative;
                    Assert.AreSame(expected.Resolve(catalog.Registry).Sprite, view.Body.sprite);
                    Assert.AreEqual(0, obj.GetComponentsInChildren<Collider2D>().Length, "Presentation never blocks movement.");
                    zone.SetCharge(.5f); view.Apply(0f);
                    if (effect.Kind == ZoneEffectKind.Shrine) Assert.AreEqual(.5f, view.Progress);
                    var color = view.Body.color; var position = view.transform.position; var progress = view.Progress;
                    view.Apply(0f);
                    Assert.AreEqual(color, view.Body.color); Assert.AreEqual(position, view.transform.position); Assert.AreEqual(progress, view.Progress);
                    view.Shutdown();
                    Assert.IsNull(view.Body); Assert.AreEqual(0, obj.transform.childCount);
                    view.Initialize(zone, presentation.AltarPresentation, catalog.Registry);
                    zone.SetNear(false); view.Apply(0f); Assert.IsFalse(view.Body.enabled);
                }
                finally { Object.DestroyImmediate(obj); }
            }
        }
    }
}
