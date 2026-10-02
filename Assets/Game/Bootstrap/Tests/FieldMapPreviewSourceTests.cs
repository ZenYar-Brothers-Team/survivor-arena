using System.Linq;
using Game.Presentation;
using NUnit.Framework;

namespace Game.Bootstrap.Tests
{
    public sealed class FieldMapPreviewSourceTests
    {
        [Test]
        public void RoadPieces_DrawCorridorsUnderMainRoadsThenRoundEnds_WithProfileWidthsAndColors()
        {
            var d = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")["FIELD-003-ENVIRONMENT"];
            var layout = d.RoadFallbackLayouts[0];
            var profile = layout.Profile;
            var pieces = FieldMapPreviewSource.RoadPieces(layout);
            var branches = layout.DeadEnds.Count; var roads = layout.Roads.Count;

            Assert.AreEqual(roads + 2 * branches, pieces.Count);
            for (var i = 0; i < branches; i++)
            {
                var branch = layout.DeadEnds[i];
                var corridor = pieces[i];
                var end = pieces[branches + roads + i];
                CollectionAssert.AreEqual(new[] { branch.Entrance, branch.EndCenter }, corridor.Points);
                Assert.AreEqual(profile.DeadEndWidth, corridor.Width);
                CollectionAssert.AreEqual(new[] { branch.EndCenter }, end.Points);
                Assert.AreEqual(profile.DeadEndEndRadius * 2f, end.Width, "A single-point piece is a disc of the round end's diameter.");
                Assert.IsTrue(new[] { corridor, end }.All(p => p.Color == profile.DeadEndColor));
            }
            for (var i = 0; i < roads; i++)
            {
                var road = pieces[branches + i];
                CollectionAssert.AreEqual(layout.Roads[i], road.Points, "Main roads cover the corridor mouths.");
                Assert.AreEqual(profile.MainRoadWidth, road.Width);
                Assert.AreEqual(profile.MainColor, road.Color);
            }
        }

        [Test]
        public void RoadPieces_WithoutRoadLayout_IsEmpty()
        {
            Assert.IsEmpty(FieldMapPreviewSource.RoadPieces(null));
        }
    }
}
