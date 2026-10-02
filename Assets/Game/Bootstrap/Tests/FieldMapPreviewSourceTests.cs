using System.Linq;
using Game.Presentation;
using NUnit.Framework;

namespace Game.Bootstrap.Tests
{
    public sealed class FieldMapPreviewSourceTests
    {
        [Test]
        public void RoadPieces_FollowTheSurfacePaintingOrderWithProfileWidthsAndColors()
        {
            var d = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")["FIELD-003-ENVIRONMENT"];
            var layout = d.RoadFallbackLayouts[0];
            var profile = layout.Profile;
            var pieces = FieldMapPreviewSource.RoadPieces(layout);

            Assert.AreEqual(layout.Roads.Count + 2 * layout.DeadEnds.Count, pieces.Count);
            for (var i = 0; i < layout.Roads.Count; i++)
            {
                CollectionAssert.AreEqual(layout.Roads[i], pieces[i].Points);
                Assert.AreEqual(profile.MainRoadWidth, pieces[i].Width);
                Assert.AreEqual(profile.MainColor, pieces[i].Color);
            }
            for (var i = 0; i < layout.DeadEnds.Count; i++)
            {
                var branch = layout.DeadEnds[i];
                var corridor = pieces[layout.Roads.Count + 2 * i];
                var end = pieces[layout.Roads.Count + 2 * i + 1];
                CollectionAssert.AreEqual(new[] { branch.Entrance, branch.EndCenter }, corridor.Points);
                Assert.AreEqual(profile.DeadEndWidth, corridor.Width);
                CollectionAssert.AreEqual(new[] { branch.EndCenter }, end.Points);
                Assert.AreEqual(profile.DeadEndEndRadius * 2f, end.Width, "A single-point piece is a disc of the round end's diameter.");
                Assert.IsTrue(new[] { corridor, end }.All(p => p.Color == profile.DeadEndColor));
            }
        }

        [Test]
        public void RoadPieces_WithoutRoadLayout_IsEmpty()
        {
            Assert.IsEmpty(FieldMapPreviewSource.RoadPieces(null));
        }
    }
}
