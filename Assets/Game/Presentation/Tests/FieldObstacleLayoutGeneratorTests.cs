using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Game.Presentation.Tests
{
    /// <summary>DECISION-0068: per-run obstacle layouts from patterns, uniform by cells, with a clear start and passages.</summary>
    public sealed class FieldObstacleLayoutGeneratorTests
    {
        private const float Side = 200f;

        private static FieldObstaclePattern Row(string id, int count) => new FieldObstaclePattern(id, 1f, new[] { 0, 90 },
            Enumerable.Range(0, count).Select(i => new FieldObstaclePiece(FieldObstacleKind.Column, (i - (count - 1) * .5f) * 2.2f, 0f, .9f, .9f)));

        private static FieldObstacleLayoutDefinition Layout(int perCell = 1, float cell = 38.4f, float margin = 3f, float clear = 8f) =>
            new FieldObstacleLayoutDefinition(cell, perCell, 4f, margin, clear, 4f, 30, 7, new[] { Row("ROW-3", 3), Row("ROW-5", 5) });

        private static IReadOnlyList<FieldObstacleDefinition> Generate(FieldObstacleLayoutDefinition layout, int seed) =>
            FieldObstacleLayoutGenerator.Generate(layout, Side, Vector2.zero, seed, "FIXTURE-ENVIRONMENT");

        private static Rect RectOf(FieldObstacleDefinition o) => new Rect(o.X - o.Width / 2, o.Y - o.Height / 2, o.Width, o.Height);

        [Test]
        public void Generate_SameSeedSameLayout_DifferentSeedsDiffer()
        {
            var layout = Layout();
            var first = Generate(layout, 11).Select(o => (o.X, o.Y, o.Width)).ToList();
            CollectionAssert.AreEqual(first, Generate(layout, 11).Select(o => (o.X, o.Y, o.Width)).ToList());
            CollectionAssert.AreNotEqual(first, Generate(layout, 12).Select(o => (o.X, o.Y, o.Width)).ToList());
        }

        [TestCase(1)]
        [TestCase(2)]
        public void Generate_EveryCellGetsItsPatterns_InsideTheArena_AwayFromTheStart(int perCell)
        {
            var layout = Layout(perCell, cell: 48f, clear: 10f);
            for (var seed = 0; seed < 25; seed++)
            {
                var obstacles = Generate(layout, seed);
                var cells = obstacles.GroupBy(o => (Mathf.FloorToInt((o.X + 96f) / 48f), Mathf.FloorToInt((o.Y + 96f) / 48f))).ToList();
                Assert.AreEqual(16, cells.Count, "4×4 cells over the 192-unit inner square, none empty.");
                foreach (var cell in cells)
                    Assert.GreaterOrEqual(cell.Count(), 3 * perCell, "Each cell holds its patterns (3–5 pieces each).");
                foreach (var o in obstacles)
                {
                    Assert.LessOrEqual(Mathf.Abs(o.X) + o.Width / 2, Side / 2 - 4f + 1e-3f);
                    Assert.LessOrEqual(Mathf.Abs(o.Y) + o.Height / 2, Side / 2 - 4f + 1e-3f);
                    Assert.GreaterOrEqual(FieldObstacleLayoutGenerator.Distance(RectOf(o), Vector2.zero), 10f - 1e-3f, "Clear start.");
                }
                Assert.AreEqual(obstacles.Count, obstacles.Select(o => o.Id).Distinct().Count(), "Unique obstacle ids.");
            }
        }

        [Test]
        public void Generate_NeighbouringCellsLeaveAPassageOfTwiceTheCellMargin()
        {
            var layout = Layout(margin: 3f);
            for (var seed = 0; seed < 25; seed++)
            {
                var obstacles = Generate(layout, seed);
                int CellOf(FieldObstacleDefinition o) =>
                    Mathf.FloorToInt((o.X + 96f) / 38.4f) * 100 + Mathf.FloorToInt((o.Y + 96f) / 38.4f);
                for (var i = 0; i < obstacles.Count; i++)
                    for (var j = i + 1; j < obstacles.Count; j++)
                        if (CellOf(obstacles[i]) != CellOf(obstacles[j]))
                            Assert.GreaterOrEqual(FieldObstacleLayoutGenerator.Gap(RectOf(obstacles[i]), RectOf(obstacles[j])), 6f - 1e-3f);
            }
        }

        [Test]
        public void Rotation_TurnsPositionsAndSwapsSize()
        {
            var piece = new FieldObstaclePiece(FieldObstacleKind.Fence, 2f, 1f, 3.2f, .8f,
                "FIELD-TEST-VISUAL-PROP");
            var turned = piece.Rotated(90);
            Assert.AreEqual(-1f, turned.X, 1e-5f);
            Assert.AreEqual(2f, turned.Y, 1e-5f);
            Assert.AreEqual(.8f, turned.Width, 1e-5f);
            Assert.AreEqual(3.2f, turned.Height, 1e-5f);
            Assert.AreEqual("FIELD-TEST-VISUAL-PROP", turned.VisualId.ToString());
            var back = piece.Rotated(360 - 90).Rotated(90);
            Assert.AreEqual(piece.X, back.X, 1e-5f);
            Assert.AreEqual(piece.Y, back.Y, 1e-5f);
        }

        [Test]
        public void Generate_PieceVisualOverride_IsCarriedToEveryGeneratedObstacle()
        {
            var pattern = new FieldObstaclePattern("OVERRIDE", 1f, new[] { 0, 90 }, new[]
            {
                new FieldObstaclePiece(FieldObstacleKind.Stump, 0f, 0f, 1f, 1f, "FIELD-TEST-VISUAL-PROP")
            });
            var layout = new FieldObstacleLayoutDefinition(48f, 1, 4f, 3f, 8f, 4f, 30, 1,
                new[] { pattern });

            var obstacles = Generate(layout, 17);

            Assert.IsNotEmpty(obstacles);
            Assert.IsTrue(obstacles.All(item => item.VisualId.ToString() == "FIELD-TEST-VISUAL-PROP"));
        }

        [Test]
        public void Definitions_RejectPatternsThatDoNotFitOverlapOrTurnOddly()
        {
            Assert.Throws<ArgumentException>(() => new FieldObstacleLayoutDefinition(10f, 1, 4f, 3f, 5f, 2f, 10, 1,
                new[] { Row("TOO-LONG", 5) }), "5 columns with 2.2 steps do not fit a 4-unit cell interior.");
            Assert.Throws<ArgumentException>(() => new FieldObstaclePattern("OVERLAP", 1f, new[] { 0 }, new[]
            {
                new FieldObstaclePiece(FieldObstacleKind.Stump, 0f, 0f, 2f, 2f), new FieldObstaclePiece(FieldObstacleKind.Stump, 1f, 0f, 2f, 2f)
            }));
            Assert.Throws<ArgumentException>(() => new FieldObstaclePattern("ODD", 1f, new[] { 45 },
                new[] { new FieldObstaclePiece(FieldObstacleKind.Stump, 0f, 0f, 1f, 1f) }));
            Assert.Throws<ArgumentException>(() => new FieldObstacleLayoutDefinition(38.4f, 1, 4f, 3f, 8f, 4f, 30, 1,
                new[] { Row("SAME", 3), Row("SAME", 4) }), "Pattern ids are unique.");
        }
    }
}
