using System;
using Game.Presentation.Json;
using NUnit.Framework;

namespace Game.Presentation.Tests
{
    public sealed class FieldRoadArtDefinitionTests
    {
        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void Art_NonPositiveOrNonFiniteRepeat_IsRejected(float repeat)
        {
            var data = ValidData(); data.SurfaceRepeat = repeat;
            Assert.Throws<ArgumentOutOfRangeException>(() => new FieldRoadArtDefinition(data));
        }

        [Test]
        public void Art_MissingRepeatOrInvertedUv_IsRejected()
        {
            var data = ValidData(); data.SurfaceRepeat = null;
            Assert.Throws<ArgumentException>(() => new FieldRoadArtDefinition(data));
            data = ValidData(); data.CurbUvBounds = new[] { .9f,.1f,.2f,.8f };
            Assert.Throws<ArgumentException>(() => new FieldRoadArtDefinition(data));
        }

        [TestCase(-.1f)]
        [TestCase(1.1f)]
        [TestCase(float.NaN)]
        public void Edge_InvalidOpacity_IsRejected(float opacity)
        {
            var data = ValidData(); data.EdgeOpacity = opacity;
            Assert.Throws<ArgumentOutOfRangeException>(() => new FieldRoadArtDefinition(data));
        }

        private static FieldRoadArtData ValidData() => new FieldRoadArtData
        {
            MainVisualId = "FIELD-003-ROAD-MAIN-VISUAL-TILE", BranchVisualId = "FIELD-003-ROAD-BROKEN-VISUAL-TILE",
            CurbVisualId = "FIELD-003-ROAD-CURB-VISUAL-PROP", SurfaceRepeat = 8, CurbWidth = .38f,
            CurbRepeat = 4, CurbUvBounds = new[] { .01f,.46f,.99f,.60f },
            CurbFaceHeight = .12f, CurbFaceTint = "#aaa296", EdgeWidth = .7f, EdgeOpacity = .65f,
            EdgeNoiseScale = .85f, EdgeNoiseStrength = .25f, EdgeSoilColor = "#8b795b"
        };
    }
}
