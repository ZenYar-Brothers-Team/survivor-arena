using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Presentation;
using Game.ScreenEvents;
using NUnit.Framework;
using UnityEngine;

namespace Game.Bootstrap.Tests
{
    /// <summary>DECISION-0157: FIELD-010 is an open arena whose danger comes from the approved screen events; no other field has them.</summary>
    public sealed class ProductionField010ScreenEventsTests
    {
        private static readonly string[] ApprovedNames =
        {
            "Копьё", "Клинок", "Ряд колонн", "Решётка", "Круги суда", "Кольцо света", "Половина экрана", "Углы", "Падающие перья",
            "Облачная волна", "Веер копий", "Небесный суд", "Дождь копий"
        };

        // Slowest character base speed in ProductionCharacters.json is 2.4; the content's fairness speed must stay under it.
        private const float SlowestCharacterSpeed = 2.4f;
        private static readonly Rect View = new Rect(-8.9f, -5f, 17.8f, 10f);

        private static ScreenEventsDefinition Definition()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            return catalog.FieldEnvironmentPresentations[new ContentId("FIELD-010-ENVIRONMENT")].ScreenEvents;
        }

        [Test]
        public void Field010_ResolvesAllApprovedArtworkAndSweepBindings()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            var environment = catalog.FieldEnvironmentPresentations[new ContentId("FIELD-010-ENVIRONMENT")];
            var art = environment.ScreenEventPresentation;
            Assert.IsNotNull(art);
            Assert.AreEqual(8, art.Artwork.Count);
            foreach (var entry in art.Artwork.Values)
                entry.Visual.Resolve(catalog.Registry).RequireRole(entry.Role);
            CollectionAssert.AreEquivalent(new[] { "SCREEN-EVENT-001", "SCREEN-EVENT-002", "SCREEN-EVENT-010", "SCREEN-EVENT-011", "SCREEN-EVENT-013" },
                art.SweepArtwork.Keys.Select(id => id.ToString()).ToArray());
            using (var resources = new ScreenEventArtResources(art, catalog.Registry))
            {
                Assert.AreEqual(4, resources.Quad.vertexCount);
                foreach (var key in art.Artwork.Keys)
                    Assert.IsNotNull(resources.MaterialFor(key).mainTexture, key);
            }
        }

        [Test]
        public void Field010_IsPlayableAndOpen()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            var field = catalog.Fields.Roster.AllFields.Single(f => f.Id.ToString() == "FIELD-010");
            var configuration = field.Resolve(catalog.Registry);
            var presentation = catalog.FieldEnvironmentPresentations[configuration.Environment.Id];
            Assert.AreEqual(0, presentation.InteriorObstacleCount, "No obstacles: the danger is the events.");
            Assert.IsNull(presentation.ZoneLayout);
            Assert.AreEqual("FIELD-010-VISUAL-GROUND", presentation.Ground.Id.ToString());
        }

        [Test]
        public void OnlyField010_HasScreenEvents()
        {
            var catalog = RuntimeContentCatalog.CreateProduction();
            foreach (var pair in catalog.FieldEnvironmentPresentations)
                Assert.AreEqual(pair.Key.ToString() == "FIELD-010-ENVIRONMENT", pair.Value.ScreenEvents != null, pair.Key.ToString());
        }

        [Test]
        public void Field010_HasTheThirteenApprovedEvents()
        {
            var definition = Definition();
            CollectionAssert.AreEqual(ApprovedNames, definition.Events.Values.Select(e => e.Name).ToArray());
            CollectionAssert.AreEqual(Enumerable.Range(1, 13).Select(i => $"SCREEN-EVENT-{i:000}").ToArray(),
                definition.Events.Keys.ToArray());
            CollectionAssert.AreEquivalent(new[] { "SCREEN-EVENT-012", "SCREEN-EVENT-013" },
                definition.Events.Values.Where(e => e.Rare).Select(e => e.Id.ToString()).ToArray(), "Only the judgment and the rain of spears are rare.");
        }

        [Test]
        public void Field010_IntensityIsAWaveThatGrowsAcrossTheRun()
        {
            var definition = Definition();
            var peaks = new List<float>();
            for (var cycle = 0; cycle < 6; cycle++)
            {
                var start = cycle * definition.WavePeriodSeconds;
                Assert.Less(definition.IntensityAt(start), definition.IntensityAt(start + definition.WavePeriodSeconds * .5f), "Each wave builds up from calm.");
                Assert.Greater(definition.IntensityAt(start + definition.WavePeriodSeconds * .5f), definition.IntensityAt(start + definition.WavePeriodSeconds), "…and eases off.");
                peaks.Add(definition.IntensityAt(start + definition.WavePeriodSeconds * .5f));
            }
            for (var i = 1; i < peaks.Count; i++) Assert.GreaterOrEqual(peaks[i], peaks[i - 1] - 1e-4f, "Later peaks are never lower.");
            Assert.GreaterOrEqual(definition.Stages.Last().PeakIntensity, definition.RareMinIntensity, "Rare events can appear in the last stage.");
        }

        [Test]
        public void Field010_RareEventsOnlyAppearInLateStages_AndFairnessSpeedIsBelowTheSlowestCharacter()
        {
            var definition = Definition();
            Assert.Less(definition.FairnessSpeed, SlowestCharacterSpeed);
            Assert.IsFalse(definition.Stages[0].Pool.Any(entry => entry.Event.Rare));
            Assert.IsTrue(definition.Stages.Last().Pool.Any(entry => entry.Event.Rare));
            Assert.IsTrue(definition.SuspendWhileBossAlive, "Events step aside for the bosses.");
        }

        [Test]
        public void Field010_EveryEventIsSurvivableForAPlayerAnywhereOnTheScreen()
        {
            var definition = Definition();
            var player = new FakeEventPlayer();
            var unfair = new List<string>();
            foreach (var id in definition.Events.Keys)
            {
                for (var seed = 0; seed < 3; seed++)
                foreach (var position in Grid(5, 3))
                {
                    player.Position = position;
                    var runtime = new ScreenEventRuntime(definition, player, seed);
                    Assert.IsTrue(runtime.TryStart(id, View));
                    if (runtime.UnfairStartCount > 0) unfair.Add($"{id} seed {seed} at {position}");
                }
            }
            Assert.IsEmpty(unfair, string.Join("; ", unfair));
        }

        private static IEnumerable<Vector2> Grid(int columns, int rows)
        {
            for (var y = 0; y < rows; y++)
            for (var x = 0; x < columns; x++)
                yield return new Vector2(View.xMin + (x + .5f) / columns * View.width, View.yMin + (y + .5f) / rows * View.height);
        }

        private sealed class FakeEventPlayer : IScreenEventPlayerTarget
        {
            public Vector2 Position { get; set; }
            public bool IsAlive => true;
            public void HitFraction(float fractionOfMaxHealth, ContentId source) { }
        }
    }
}
