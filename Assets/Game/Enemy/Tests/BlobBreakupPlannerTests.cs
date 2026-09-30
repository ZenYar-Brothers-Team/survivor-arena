using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    public sealed class BlobBreakupPlannerTests
    {
        private static BlobBreakupDefinition Trial() => new BlobBreakupDefinition(
            10f, 11, .7f, 60, 65f, 3f, 1.5f, 6f);

        [Test]
        public void Plan_LargeDenseGroup_CanSelectSixtyButNoMore()
        {
            var positions = new List<Vector2>();
            for (var i = 0; i < 200; i++)
                positions.Add(new Vector2(-5f + i % 20 * .1f, -.5f + i / 20 * .1f));
            var selected = new List<int>();
            var waypoints = new List<Vector2>();
            var delays = new List<float>();
            var settings = new BlobBreakupDefinition(10f, 18, 1f, 60, 65f, 3f, 1.5f, 4f);

            var count = BlobBreakupPlanner.Plan(positions, Vector2.zero, settings,
                new System.Random(19), new BlobBreakupPlanner.Cell[BlobBreakupPlanner.GridLength],
                selected, waypoints, delays);

            Assert.AreEqual(60, count);
            Assert.AreEqual(60, selected.Distinct().Count());
        }

        [Test]
        public void Plan_TwoDenseSquares_SelectsBothWithIndependentSixtyCaps()
        {
            var positions = new List<Vector2>();
            for (var i = 0; i < 100; i++)
                positions.Add(new Vector2(-5f + i % 10 * .1f, -.5f + i / 10 * .1f));
            for (var i = 0; i < 100; i++)
                positions.Add(new Vector2(4f + i % 10 * .1f, -.5f + i / 10 * .1f));
            var selected = new List<int>();
            var waypoints = new List<Vector2>();
            var delays = new List<float>();
            var settings = new BlobBreakupDefinition(10f, 18, 1f, 60, 65f, 3f, 1.5f, 4f);

            var count = BlobBreakupPlanner.Plan(positions, Vector2.zero, settings,
                new System.Random(19), new BlobBreakupPlanner.Cell[BlobBreakupPlanner.GridLength],
                selected, waypoints, delays);

            Assert.AreEqual(120, count);
            Assert.AreEqual(60, selected.Count(index => index < 100));
            Assert.AreEqual(60, selected.Count(index => index >= 100));
            Assert.AreEqual(count, selected.Distinct().Count());
        }

        [Test]
        public void Plan_DisjointSquares_DoNotCombineBelowThresholdGroups()
        {
            var positions = new List<Vector2>();
            for (var i = 0; i < 10; i++)
                positions.Add(new Vector2(-5.1f, -.8f + i * .08f));
            for (var i = 0; i < 10; i++)
                positions.Add(new Vector2(-5.4f, -.8f + i * .08f));
            var selected = new List<int>();
            var waypoints = new List<Vector2>();
            var delays = new List<float>();
            var settings = new BlobBreakupDefinition(10f, 11, 1f, 60, 65f, 3f, 1.5f, 4f);

            var count = BlobBreakupPlanner.Plan(positions, Vector2.zero, settings,
                new System.Random(19), new BlobBreakupPlanner.Cell[BlobBreakupPlanner.GridLength],
                selected, waypoints, delays);

            Assert.AreEqual(0, count);
            Assert.IsEmpty(selected);

            positions.Add(new Vector2(-5.1f, .8f));
            var atThreshold = BlobBreakupPlanner.Plan(positions, Vector2.zero, settings,
                new System.Random(19), new BlobBreakupPlanner.Cell[BlobBreakupPlanner.GridLength],
                selected, waypoints, delays);
            Assert.AreEqual(11, atThreshold);
            Assert.IsTrue(selected.All(index => index < 10 || index == 20));
        }

        [Test]
        public void Plan_RestrictedMembers_CountTowardDensityButReceiveNoManeuver()
        {
            var positions = new List<Vector2>();
            var eligible = new List<bool>();
            for (var i = 0; i < 11; i++)
            {
                positions.Add(new Vector2(-5f + i % 6 * .15f, -.5f + i / 6 * .15f));
                eligible.Add(i < 5);
            }
            var selected = new List<int>();
            var waypoints = new List<Vector2>();
            var delays = new List<float>();
            var settings = new BlobBreakupDefinition(10f, 11, 1f, 60, 65f, 3f, 1.5f, 4f);

            var count = BlobBreakupPlanner.Plan(positions, Vector2.zero, settings,
                new System.Random(19), new BlobBreakupPlanner.Cell[BlobBreakupPlanner.GridLength],
                selected, waypoints, delays, eligible);

            Assert.AreEqual(5, count);
            Assert.IsTrue(selected.All(index => eligible[index]));
        }

        [Test]
        public void Plan_PlayerCenteredSquare_IsExcludedWhileOtherDenseSquareIsProcessed()
        {
            var player = new Vector2(5f, 7f);
            var positions = new List<Vector2>();
            for (var i = 0; i < 30; i++)
                positions.Add(player + new Vector2(-.5f + i % 6 * .15f, -.4f + i / 6 * .15f));
            for (var i = 0; i < 11; i++)
                positions.Add(player + new Vector2(-5f + i % 6 * .15f, -.4f + i / 6 * .15f));
            var selected = new List<int>();
            var waypoints = new List<Vector2>();
            var delays = new List<float>();
            var settings = new BlobBreakupDefinition(10f, 11, 1f, 60, 65f, 3f, 1.5f, 6f);

            var count = BlobBreakupPlanner.Plan(positions, player, settings,
                new System.Random(19), new BlobBreakupPlanner.Cell[BlobBreakupPlanner.GridLength],
                selected, waypoints, delays);

            Assert.AreEqual(11, count);
            Assert.IsTrue(selected.All(index => index >= 30));
        }

        [Test]
        public void Plan_CentralSquareBoundary_IsCenteredOnPlayer()
        {
            var player = new Vector2(5f, 7f);
            var positions = new List<Vector2>();
            for (var i = 0; i < 11; i++)
                positions.Add(player + new Vector2(1.74f - i % 6 * .1f, -.3f + i / 6 * .2f));
            for (var i = 0; i < 11; i++)
                positions.Add(player + new Vector2(1.76f + i % 6 * .1f, -.3f + i / 6 * .2f));
            var selected = new List<int>();
            var waypoints = new List<Vector2>();
            var delays = new List<float>();
            var settings = new BlobBreakupDefinition(10f, 11, 1f, 60, 65f, 3f, 1.5f, 6f);

            var count = BlobBreakupPlanner.Plan(positions, player, settings,
                new System.Random(19), new BlobBreakupPlanner.Cell[BlobBreakupPlanner.GridLength],
                selected, waypoints, delays);

            Assert.AreEqual(11, count);
            Assert.IsTrue(selected.All(index => index >= 11));
        }

        [Test]
        public void Plan_DenseGroup_AssignsStaggeredWaypointsOnBothSides()
        {
            var positions = new List<Vector2>();
            for (var y = 0; y < 5; y++)
                for (var x = 0; x < 6; x++)
                    positions.Add(new Vector2(-5f + x * .25f, -.5f + y * .25f));
            var selected = new List<int>();
            var waypoints = new List<Vector2>();
            var delays = new List<float>();
            var count = BlobBreakupPlanner.Plan(positions, Vector2.zero, Trial(),
                new System.Random(19), new BlobBreakupPlanner.Cell[BlobBreakupPlanner.GridLength],
                selected, waypoints, delays);

            Assert.Greater(count, 2);
            Assert.Less(count, positions.Count);
            Assert.AreEqual(count, selected.Distinct().Count());
            Assert.IsTrue(waypoints.Any(point => point.y > 0f));
            Assert.IsTrue(waypoints.Any(point => point.y < 0f));
            Assert.IsTrue(delays.All(delay => delay >= 0f && delay <= 1.5f));
        }

        [Test]
        public void Plan_FanKeepsEnemiesOnTheirLateralSide()
        {
            var positions = new List<Vector2>();
            for (var i = 0; i < 12; i++)
                positions.Add(new Vector2(-5f + i % 4 * .1f, -.4f));
            for (var i = 0; i < 12; i++)
                positions.Add(new Vector2(-5f + i % 4 * .1f, .4f));
            var selected = new List<int>();
            var waypoints = new List<Vector2>();
            var delays = new List<float>();
            var settings = new BlobBreakupDefinition(10f, 11, 1f, 60, 65f, 3f, 1.5f, 6f);

            var count = BlobBreakupPlanner.Plan(positions, Vector2.zero, settings,
                new System.Random(19), new BlobBreakupPlanner.Cell[BlobBreakupPlanner.GridLength],
                selected, waypoints, delays);

            Assert.AreEqual(24, count);
            for (var i = 0; i < count; i++)
                Assert.Greater(positions[selected[i]].y * waypoints[i].y, 0f);
        }

        [Test]
        public void Plan_SparseGroup_DoesNotAssignManeuvers()
        {
            var positions = Enumerable.Range(0, 12)
                .Select(index => new Vector2(-12f + index % 4 * 7f, -10f + index / 4 * 7f))
                .ToArray();
            var selected = new List<int>();
            var waypoints = new List<Vector2>();
            var delays = new List<float>();
            var count = BlobBreakupPlanner.Plan(positions, Vector2.zero, Trial(),
                new System.Random(19), new BlobBreakupPlanner.Cell[BlobBreakupPlanner.GridLength],
                selected, waypoints, delays);

            Assert.AreEqual(0, count);
            Assert.IsEmpty(selected);
        }

        [Test]
        public void ProductionPhases_AllEnableTenSecondCheckForEveryOrdinaryType()
        {
            foreach (var path in new[]
            {
                "Content/Waves/ProductionWaveTimeline",
                "Content/Waves/ProductionWaveTimelineField002",
                "Content/Waves/ProductionWaveTimelineField003"
            })
            {
                var asset = Resources.Load<TextAsset>(path);
                Assert.IsNotNull(asset, path);
                var timeline = FixtureWaveTimelineCatalog.FromJson(asset.text);
                Assert.IsTrue(timeline.Phases.All(phase => phase.BlobBreakup != null &&
                    phase.BlobBreakup.CheckIntervalSeconds == 10f &&
                    phase.BlobBreakup.MinimumClusterCount == 11 &&
                    phase.BlobBreakup.SelectionFraction == .7f &&
                    phase.BlobBreakup.MaxSelected == 60 &&
                    phase.BlobBreakup.ManeuverSeconds == 6f &&
                    phase.BlobBreakup.EnemyIds.Count == 0), path);
            }
        }

        [Test]
        public void Definition_EnemyFilterAndDisabledPhase_AreExplicit()
        {
            var filtered = new BlobBreakupDefinition(10f, 12, .4f, 60, 65f, 3f, 1.5f, 4f,
                new[] { new Game.Content.ContentId("ENEMY-001") });
            Assert.IsTrue(filtered.Includes("ENEMY-001"));
            Assert.IsFalse(filtered.Includes("ENEMY-002"));
            var phase = new WavePhaseDefinition("FIXTURE-P", "P", WavePhaseTag.Ordinary,
                10f, 1f, new[] { WaveTestData.Entry("FIXTURE-ENEMY-A") });
            Assert.IsNull(phase.BlobBreakup);
            Assert.Throws<ArgumentOutOfRangeException>(() => new BlobBreakupDefinition(
                0f, 12, .4f, 60, 65f, 3f, 1.5f, 4f));
        }

        [Test]
        public void WaveJson_CanDisableOnePhaseAndFilterAnotherByEnemyId()
        {
            var asset = Resources.Load<TextAsset>("Content/Waves/ProductionWaveTimeline");
            var data = JObject.Parse(asset.text);
            var phases = (JArray)data["phases"];
            var profile = JObject.Parse(Resources.Load<TextAsset>(
                "Content/Waves/ProductionBlobBreakupProfile").text);
            Assert.AreEqual("BLOB-FAN-001", (string)profile["id"]);
            Assert.AreEqual(.7f, (float)profile["selectionFraction"]);
            Assert.AreEqual(60, (int)profile["maxSelected"]);
            Assert.IsTrue(phases.All(phase => ((JObject)phase["blobBreakup"]).Properties()
                .All(property => property.Name == "profileId" || property.Name == "enemyIds")));
            Assert.IsTrue(phases.All(phase =>
                (string)phase["blobBreakup"]["profileId"] == "BLOB-FAN-001"));
            phases[0]["blobBreakup"] = JValue.CreateNull();
            phases[1]["blobBreakup"]["enemyIds"] = new JArray("ENEMY-001");

            var timeline = FixtureWaveTimelineCatalog.FromJson(data.ToString());

            Assert.IsNull(timeline.Phases[0].BlobBreakup);
            Assert.IsTrue(timeline.Phases[1].BlobBreakup.Includes("ENEMY-001"));
            Assert.IsFalse(timeline.Phases[1].BlobBreakup.Includes("ENEMY-002"));
            Assert.IsNotNull(timeline.Phases[2].BlobBreakup);
        }

        [Test]
        public void WaveJson_RejectsNumbersInsidePhaseBindingOrUnknownProfile()
        {
            var asset = Resources.Load<TextAsset>("Content/Waves/ProductionWaveTimeline");
            var data = JObject.Parse(asset.text);
            ((JObject)((JArray)data["phases"])[0]["blobBreakup"])["maxSelected"] = 24;
            Assert.Throws<Newtonsoft.Json.JsonSerializationException>(() =>
                FixtureWaveTimelineCatalog.FromJson(data.ToString()));

            ((JObject)((JArray)data["phases"])[0]["blobBreakup"]).Remove("maxSelected");
            ((JObject)((JArray)data["phases"])[0]["blobBreakup"])["profileId"] = "UNKNOWN";
            Assert.Throws<InvalidOperationException>(() =>
                FixtureWaveTimelineCatalog.FromJson(data.ToString()));
        }
    }
}
