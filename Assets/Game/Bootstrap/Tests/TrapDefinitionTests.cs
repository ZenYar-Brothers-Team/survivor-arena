using System;
using System.Linq;
using Game.Traps;
using Game.Traps.Json;
using NUnit.Framework;
using static Game.Bootstrap.Tests.TrapTestLayouts;

namespace Game.Bootstrap.Tests
{
    /// <summary>DECISION-0156: trap data is validated by its domain types; patterns expand from shot groups.</summary>
    public sealed class TrapDefinitionTests
    {
        private static TrapProjectileData[] OneProjectile() => new[] { Projectile("p") };

        [Test]
        public void ShotGroup_ExpandsAnglesLateralOffsetsAndDelays()
        {
            var layout = Layout(OneProjectile(), new[]
            {
                Type("row", "fixed", new[] { Shots("p", 4, lateralStart: -1.35f, lateralStep: .9f) }),
                Type("ring", "fixed", new[] { Shots("p", 32, angleStep: 11.25f) }),
                Type("spiral", "fixed", new[] { Shots("p", 14, angleStep: 25f, delayStep: .22f) })
            });
            var row = layout.Types[0].Shots;
            CollectionAssert.AreEqual(new[] { -1.35f, -.45f, .45f, 1.35f }, row.Select(s => s.Lateral).ToArray()
                .Select(v => (float)Math.Round(v, 2)).ToArray());
            Assert.IsTrue(row.All(s => s.AngleDegrees == 0f && s.DelaySeconds == 0f));
            var ring = layout.Types[1].Shots;
            Assert.AreEqual(32, ring.Count);
            Assert.AreEqual(348.75f, ring[31].AngleDegrees, 1e-3f);
            var spiral = layout.Types[2];
            Assert.AreEqual(13 * .22f, spiral.LastShotDelaySeconds, 1e-4f);
            Assert.AreEqual(13 * 25f, spiral.Shots[13].AngleDegrees, 1e-3f);
        }

        [Test]
        public void HeadingOffset_AccumulatesOrCycles()
        {
            var layout = Layout(OneProjectile(), new[]
            {
                Type("alternate", "fixed", new[] { Shots("p") }, headingStep: 45f, headingCycle: 2),
                Type("sweep", "fixed", new[] { Shots("p") }, headingStep: 22f, headingCycle: 0),
                Type("still", "aim", new[] { Shots("p") })
            });
            CollectionAssert.AreEqual(new[] { 0f, 45f, 0f, 45f }, Enumerable.Range(0, 4).Select(layout.Types[0].HeadingOffset).ToArray());
            CollectionAssert.AreEqual(new[] { 0f, 22f, 44f }, Enumerable.Range(0, 3).Select(layout.Types[1].HeadingOffset).ToArray());
            Assert.AreEqual(0f, layout.Types[2].HeadingOffset(5));
        }

        [Test]
        public void RageDefinition_GrowsLinearlyAndCaps()
        {
            var rage = Layout(OneProjectile(), new[] { Type("t", "aim", new[] { Shots("p") }) }).Rage;
            Assert.AreEqual(1f, rage.Multiplier(0f), 1e-5f);
            Assert.AreEqual(2f, rage.Multiplier(30f), 1e-5f);
            Assert.AreEqual(3f, rage.Multiplier(60f), 1e-5f);
            Assert.AreEqual(3f, rage.Multiplier(600f), 1e-5f);
            Assert.AreEqual(1f, rage.Multiplier(-5f), 1e-5f);
        }

        [Test]
        public void Validation_RejectsUnknownOrUnusedProjectiles()
        {
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { Type("t", "aim", new[] { Shots("missing") }) }));
            Assert.Throws<ArgumentException>(() => Layout(new[] { Projectile("p"), Projectile("unused") },
                new[] { Type("t", "aim", new[] { Shots("p") }) }), "Every projectile kind must be used (no orphans).");
        }

        [Test]
        public void Validation_RejectsBadHeadingAndRotations()
        {
            var fixedWithoutRotations = Type("t", "fixed", new[] { Shots("p") });
            fixedWithoutRotations.RotationsDegrees = new float[0];
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { fixedWithoutRotations }));
            var aimWithRotations = Type("t", "aim", new[] { Shots("p") });
            aimWithRotations.RotationsDegrees = new[] { 0f };
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { aimWithRotations }));
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { Type("t", "sideways", new[] { Shots("p") }) }));
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { Type("t", "aim", new TrapShotGroupData[0]) }));
        }

        [Test]
        public void Validation_RejectsDuplicatesAndBadRageOrBarrels()
        {
            var type = Type("t", "aim", new[] { Shots("p") });
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { type, type }));
            var slowing = Data(OneProjectile(), new[] { type });
            slowing.Rage.MaxMultiplier = .5f;
            Assert.Throws<ArgumentOutOfRangeException>(() => new TrapLayoutDefinition(slowing));
            Assert.Throws<ArgumentOutOfRangeException>(() => Layout(OneProjectile(), new[] { type }, barrels: Barrels(3, 1.5f)));
            var missing = Data(OneProjectile(), new[] { type });
            missing.Rage = null;
            Assert.Throws<ArgumentException>(() => new TrapLayoutDefinition(missing));
        }

        [Test]
        public void Validation_RejectsASpinningAimedTrap_AndBadStartTraps()
        {
            var shots = new[] { Shots("p") };
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { Type("t", "aim", shots, spin: 30f) }));
            var fixedType = Type("t", "fixed", shots, count: 2);
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { fixedType }, starts: new[] { Start("missing") }));
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { fixedType },
                starts: new[] { Start("t", model: "nope") }), "A start trap names a model that does not exist.");
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { fixedType },
                starts: new[] { Start("t"), Start("t", 4f), Start("t", 5f) }), "More start traps than the type count.");
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { fixedType }, starts: new[] { Start("t", 0f, 0f) }));
            var layout = Layout(OneProjectile(), new[] { fixedType }, models: new[] { Model() }, starts: new[] { Start("t", model: "cross") });
            Assert.AreEqual("cross", layout.StartTraps[0].ModelKey);
            Assert.AreEqual(54f, layout.Models["cross"].TiltDegrees);
        }

        [Test]
        public void Validation_RejectsUnknownTypeModels_IncompleteSprites_AndTooManyStartBarrels()
        {
            var shots = new[] { Shots("p") };
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { Type("t", "aim", shots, model: "nope") }));
            var ok = Layout(OneProjectile(), new[] { Type("t", "aim", shots, model: "cross") }, models: new[] { Model() });
            Assert.AreEqual("cross", ok.Types[0].ModelKey);
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { Type("t", "aim", shots) },
                sprites: new System.Collections.Generic.Dictionary<string, string> { ["barrel"] = "B" }), "The projectile visual has no sprite.");
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { Type("t", "aim", shots) },
                sprites: new System.Collections.Generic.Dictionary<string, string> { ["spikeball"] = "S" }), "The barrel has no sprite.");
            var withSprites = Layout(OneProjectile(), new[] { Type("t", "aim", shots) },
                sprites: new System.Collections.Generic.Dictionary<string, string> { ["spikeball"] = "S", ["barrel"] = "B" });
            Assert.AreEqual("S", withSprites.Sprites["spikeball"]);
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { Type("t", "aim", shots) },
                barrels: Barrels(1, 0f), startBarrels: new[] { StartBarrel(3f), StartBarrel(4f) }));
        }

        [Test]
        public void Validation_RejectsBadDensity()
        {
            var type = Type("t", "aim", new[] { Shots("p") });
            var noWeights = Density();
            noWeights.CountWeights = new Game.Traps.Json.TrapCountWeightData[0];
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { type }, density: noWeights));
            var twice = Density();
            twice.CountWeights[1].Count = 1;
            Assert.Throws<ArgumentException>(() => Layout(OneProjectile(), new[] { type }, density: twice));
            var flat = Density();
            flat.CellWidth = 0f;
            Assert.Throws<ArgumentOutOfRangeException>(() => Layout(OneProjectile(), new[] { type }, density: flat));
            Assert.IsNotNull(Layout(OneProjectile(), new[] { type }, density: Density()).Density);
        }

        [Test]
        public void Validation_RejectsANegativeAimResumeDelay()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Layout(OneProjectile(), new[] { Type("t", "aim", new[] { Shots("p") }, aimResume: -.1f) }));
            Assert.AreEqual(.4f, Layout(OneProjectile(), new[] { Type("t", "aim", new[] { Shots("p") }, aimResume: .4f) }).Types[0].AimResumeDelaySeconds);
        }

        [Test]
        public void Barrels_ExplosiveCountRoundsTheShare_ForAFixedTotal()
        {
            var layout = Layout(OneProjectile(), new[] { Type("t", "aim", new[] { Shots("p") }) }, barrels: Barrels(30, 1f / 3f));
            Assert.AreEqual(10, layout.Barrels.ExplosiveCount);
        }
    }
}
