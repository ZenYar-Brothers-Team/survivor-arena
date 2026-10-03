using System;
using NUnit.Framework;

namespace Game.ScreenEvents.Tests
{
    public sealed class ScreenEventsDefinitionTests
    {
        [Test]
        public void Definition_AcceptsEveryLayout()
        {
            var definition = ScreenEventTestData.Definition();
            Assert.AreEqual(9, definition.Events.Count);
            Assert.AreEqual(2, definition.Stages.Count);
        }

        [Test]
        public void Event_MissingNumberOfItsLayout_ThrowsByName()
        {
            var data = ScreenEventTestData.Ring();
            data.GapWidthH = null;
            var error = Assert.Throws<ArgumentException>(() => new ScreenEventDefinition(data));
            StringAssert.Contains("gapWidthH", error.Message);
        }

        [Test]
        public void Event_NumberOfAnotherLayout_IsNotRequired()
        {
            var data = ScreenEventTestData.Half();
            Assert.IsNull(data.WidthH);
            Assert.DoesNotThrow(() => new ScreenEventDefinition(data));
        }

        [Test]
        public void Event_OutOfRangeValues_Throw()
        {
            var damage = ScreenEventTestData.Spear(); damage.DamagePercent = 120;
            var intensity = ScreenEventTestData.Spear(); intensity.Intensity = 1.5f;
            var counts = ScreenEventTestData.Spear(); counts.CountMin = 4; counts.CountMax = 2;
            Assert.Throws<ArgumentOutOfRangeException>(() => new ScreenEventDefinition(damage));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ScreenEventDefinition(intensity));
            Assert.Throws<ArgumentException>(() => new ScreenEventDefinition(counts));
        }

        [Test]
        public void Profile_PoolNamingUnknownEvent_Throws()
        {
            var events = new[] { ScreenEventTestData.Spear() };
            var stages = new[] { ScreenEventTestData.Stage(0, .1f, .5f, 5, 9, ("T-SPEAR", 1f), ("T-NOPE", 1f)) };
            Assert.Throws<ArgumentException>(() => ScreenEventTestData.Definition(events, stages));
        }

        [Test]
        public void Profile_EventUsedByNoStage_Throws()
        {
            var events = new[] { ScreenEventTestData.Spear(), ScreenEventTestData.Cross() };
            var stages = new[] { ScreenEventTestData.Stage(0, .1f, .5f, 5, 9, ("T-SPEAR", 1f)) };
            var error = Assert.Throws<ArgumentException>(() => ScreenEventTestData.Definition(events, stages));
            StringAssert.Contains("T-CROSS", error.Message);
        }

        [Test]
        public void Profile_StageWithOnlyRareEvents_Throws()
        {
            var events = new[] { ScreenEventTestData.Judgment() };
            var stages = new[] { ScreenEventTestData.Stage(0, .1f, .5f, 5, 9, ("T-JUDGMENT", 1f)) };
            Assert.Throws<ArgumentException>(() => ScreenEventTestData.Definition(events, stages));
        }

        [Test]
        public void Profile_StagesMustStartAtZeroAndAscend()
        {
            var events = new[] { ScreenEventTestData.Spear() };
            Assert.Throws<ArgumentException>(() => ScreenEventTestData.Definition(events,
                new[] { ScreenEventTestData.Stage(10, .1f, .5f, 5, 9, ("T-SPEAR", 1f)) }));
            Assert.Throws<ArgumentException>(() => ScreenEventTestData.Definition(events, new[]
            {
                ScreenEventTestData.Stage(0, .1f, .5f, 5, 9, ("T-SPEAR", 1f)),
                ScreenEventTestData.Stage(0, .1f, .5f, 5, 9, ("T-SPEAR", 1f))
            }));
        }

        [Test]
        public void IntensityAt_IsACosineWaveThatStartsCalmAndPeaksMidPeriod_AndGrowsWithStages()
        {
            var definition = ScreenEventTestData.Definition();
            Assert.AreEqual(.1f, definition.IntensityAt(0f), 1e-4f, "The run starts at the valley.");
            var peak = definition.IntensityAt(50f);
            Assert.Greater(peak, .29f, "Half a period in is the top of the first wave.");
            Assert.Less(definition.IntensityAt(25f), peak);
            Assert.Less(definition.IntensityAt(75f), peak);
            Assert.Less(definition.IntensityAt(100f), peak, "It eases off again after the peak.");
            Assert.Greater(definition.IntensityAt(350f), peak, "Later waves peak higher.");
            for (var t = 0f; t < 600f; t += 7f)
                Assert.That(definition.IntensityAt(t), Is.InRange(0f, 1f));
        }

        [Test]
        public void IntensityAt_BlendsStageValuesWithoutAJump()
        {
            var definition = ScreenEventTestData.Definition();
            var before = definition.IntensityAt(299.9f);
            var after = definition.IntensityAt(300.1f);
            Assert.Less(Math.Abs(after - before), .02f);
        }

        [Test]
        public void Profile_MissingRequiredField_ThrowsByName()
        {
            var data = ScreenEventTestData.Profile();
            data.FairnessSpeed = null;
            var error = Assert.Throws<ArgumentException>(() => new ScreenEventsDefinition(data));
            StringAssert.Contains("fairnessSpeed", error.Message);
        }
    }
}
