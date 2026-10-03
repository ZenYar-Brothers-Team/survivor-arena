using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Traps;
using Game.Traps.Json;
using NUnit.Framework;
using UnityEngine;
using static Game.Bootstrap.Tests.TrapTestLayouts;

namespace Game.Bootstrap.Tests
{
    /// <summary>DECISION-0156: trap simulation — activation radius, telegraph, rage, projectile endings, barrels.</summary>
    public sealed class TrapRuntimeTests
    {
        private static void RunUntil(TrapRuntime runtime, Func<bool> condition, float limitSeconds = 120f)
        {
            for (var elapsed = 0f; elapsed < limitSeconds && !condition(); elapsed += .02f) runtime.Tick(.02f, ScreenWidth);
            Assert.IsTrue(condition(), "Condition not reached in time.");
        }

        private static (TrapRuntime runtime, TrapPlacement trap, FakeTrapPlayerTarget player) Single(TrapTypeData type,
            Vector2 player, TrapProjectileData[] projectiles = null, float rotation = 0f, float cooldown = .1f,
            IReadOnlyList<IReadOnlyList<Vector2>> outlines = null, int maxActive = 100)
        {
            var layout = Layout(projectiles ?? new[] { Projectile("p") }, new[] { type }, maxActive);
            var target = new FakeTrapPlayerTarget { Position = player };
            var set = One(layout, 0, Vector2.zero, rotation, cooldown);
            return (Runtime(layout, set, target, outlines), set.Traps[0], target);
        }

        [Test]
        public void Telegraph_PrecedesTheFirstShot()
        {
            var (runtime, trap, _) = Single(Type("t", "aim", new[] { Shots("p") }, telegraph: .5f, cooldown: 100f), new Vector2(0f, 8f));
            Advance(runtime, .3f);
            Assert.AreEqual(TrapState.Telegraph, trap.State);
            Assert.AreEqual(0, runtime.Projectiles.Count);
            Assert.Greater(trap.TelegraphProgress, 0f);
            Advance(runtime, .5f);
            Assert.AreEqual(1, runtime.Projectiles.Count);
            Assert.AreEqual(1, trap.VolleyIndex);
        }

        [Test]
        public void AimedHeading_LocksWhenTheTelegraphStarts()
        {
            var (runtime, trap, player) = Single(Type("t", "aim", new[] { Shots("p") }, telegraph: .5f, cooldown: 100f), new Vector2(0f, 8f));
            Advance(runtime, .2f);
            Assert.AreEqual(90f, trap.LockedHeadingDegrees, .5f);
            player.Position = new Vector2(8f, 0f);
            Advance(runtime, .45f);
            Assert.AreEqual(1, runtime.Projectiles.Count);
            Assert.Greater(runtime.Projectiles[0].Direction.y, .99f, "The shot keeps the locked direction, not the new player position.");
        }

        [Test]
        public void Projectile_HitsThePlayer_ForItsDamage_AndEnds()
        {
            var (runtime, trap, player) = Single(Type("trap-x", "aim", new[] { Shots("p") }, telegraph: .3f, cooldown: 100f), new Vector2(0f, 3f));
            RunUntil(runtime, () => player.Hits.Count > 0);
            Assert.AreEqual(5f, player.Hits[0].amount);
            Assert.AreEqual(new ContentId("trap-x"), player.Hits[0].source);
            Assert.AreEqual(0, runtime.Projectiles.Count);
            Assert.AreEqual(1, player.Hits.Count);
        }

        [Test]
        public void DeadPlayer_IsNeverHit()
        {
            var (runtime, _, player) = Single(Type("t", "aim", new[] { Shots("p") }, telegraph: .3f, cooldown: 100f), new Vector2(0f, 3f));
            player.IsAlive = false;
            Advance(runtime, 3f);
            Assert.AreEqual(0, player.Hits.Count);
        }

        [Test]
        public void Rage_GrowsWhileActive_AndResetsTheMomentTheTrapDeactivates()
        {
            var (runtime, trap, player) = Single(Type("t", "aim", new[] { Shots("p") }, cooldown: 3f), new Vector2(0f, 5f));
            Advance(runtime, 30f);
            Assert.AreEqual(2f, trap.RageMultiplier, .05f);
            Assert.AreEqual(.5f, trap.RageProgress, .01f);
            player.Position = new Vector2(0f, 50f);
            runtime.Tick(.02f, ScreenWidth);
            Assert.IsFalse(trap.IsActive);
            Assert.AreEqual(1f, trap.RageMultiplier);
            Assert.AreEqual(0f, trap.RageProgress);
            Assert.AreEqual(0f, trap.RageSeconds);
            player.Position = new Vector2(0f, 5f);
            runtime.Tick(.02f, ScreenWidth);
            Assert.Less(trap.RageSeconds, .05f, "Coming back starts the ramp from zero.");
        }

        [Test]
        public void Rage_ReachesTheMaximumAfterTheRampAndStaysThere()
        {
            var (runtime, trap, _) = Single(Type("t", "aim", new[] { Shots("p") }, cooldown: 3f), new Vector2(0f, 5f));
            Advance(runtime, 61f);
            Assert.AreEqual(3f, trap.RageMultiplier, 1e-4f);
            Advance(runtime, 20f);
            Assert.AreEqual(3f, trap.RageMultiplier, 1e-4f);
        }

        [Test]
        public void Rage_ShortensOnlyTheCooldown_NotTheTelegraphOrDelaysInsideAVolley()
        {
            var (runtime, trap, _) = Single(Type("t", "aim", new[] { Shots("p", 2, delayStep: .5f) }, telegraph: 1f, cooldown: 6f),
                new Vector2(0f, 5f), maxActive: 1000);
            Advance(runtime, 61f);
            RunUntil(runtime, () => trap.State == TrapState.Idle && trap.CooldownRemaining >= 5.99f);
            runtime.Tick(.1f, ScreenWidth);
            Assert.AreEqual(5.7f, trap.CooldownRemaining, .02f, "At x3 the cooldown drains three times faster.");
            RunUntil(runtime, () => trap.State == TrapState.Telegraph);
            Assert.AreEqual(1f, trap.TelegraphRemaining, 1e-4f);
            runtime.Tick(.1f, ScreenWidth);
            Assert.AreEqual(.9f, trap.TelegraphRemaining, 1e-3f, "The telegraph runs at normal speed.");
        }

        [Test]
        public void LeavingTheRadius_AbortsTheWindUpAndCancelsShotsThatHaveNotLeft()
        {
            var (runtime, trap, player) = Single(Type("t", "aim", new[] { Shots("p", 3, delayStep: .5f) }, telegraph: .3f, cooldown: 100f),
                new Vector2(0f, 8f), maxActive: 1000);
            RunUntil(runtime, () => trap.State == TrapState.Firing);
            player.Position = new Vector2(0f, 50f);
            runtime.Tick(.02f, ScreenWidth);
            Assert.AreEqual(TrapState.Idle, trap.State);
            Assert.AreEqual(0, runtime.Projectiles.Count, "The first shot is out of the cutoff radius, the rest were cancelled.");
            player.Position = new Vector2(0f, 8f);
            Advance(runtime, .6f);
            Assert.GreaterOrEqual(trap.VolleyIndex, 2, "Coming back, the aborted wind-up starts over at once.");
        }

        [Test]
        public void Projectile_EndsOutsideTheRadiusAroundThePlayer()
        {
            var (runtime, _, player) = Single(Type("t", "fixed", new[] { Shots("p") }, telegraph: .3f, cooldown: 100f), new Vector2(0f, -5f));
            RunUntil(runtime, () => runtime.Projectiles.Count > 0);
            Advance(runtime, 4f);
            Assert.AreEqual(0, runtime.Projectiles.Count);
            Assert.AreEqual(0, player.Hits.Count);
        }

        [Test]
        public void Projectile_EndsAtObstacles()
        {
            var wall = Box(3f, -1f, 4f, 1f);
            var (runtime, _, _) = Single(Type("t", "fixed", new[] { Shots("p") }, telegraph: .3f, cooldown: 100f), new Vector2(0f, -5f), outlines: wall);
            Advance(runtime, 1.1f);
            Assert.AreEqual(0, runtime.Projectiles.Count);
            var (open, _, _) = Single(Type("t", "fixed", new[] { Shots("p") }, telegraph: .3f, cooldown: 100f), new Vector2(0f, -5f));
            Advance(open, 1.1f);
            Assert.AreEqual(1, open.Projectiles.Count, "Without the wall the same projectile is still in flight.");
        }

        [Test]
        public void Projectile_EndsAtItsOwnRange()
        {
            var (runtime, _, _) = Single(Type("t", "fixed", new[] { Shots("p") }, telegraph: .3f, cooldown: 100f), new Vector2(0f, -5f),
                new[] { Projectile("p", maxDistance: 2f) });
            Advance(runtime, .7f);
            Assert.AreEqual(1, runtime.Projectiles.Count);
            Advance(runtime, .6f);
            Assert.AreEqual(0, runtime.Projectiles.Count);
        }

        [Test]
        public void Boomerang_FliesBackToItsTrapAndEnds()
        {
            var (runtime, _, _) = Single(Type("t", "fixed", new[] { Shots("p") }, telegraph: .3f, cooldown: 100f), new Vector2(0f, -5f),
                new[] { Projectile("p", returnAfter: .4f) });
            var farthest = 0f;
            for (var t = 0f; t < 3f; t += .02f)
            {
                runtime.Tick(.02f, ScreenWidth);
                foreach (var projectile in runtime.Projectiles) farthest = Mathf.Max(farthest, projectile.Position.x);
            }
            Assert.Greater(farthest, 2f);
            Assert.AreEqual(0, runtime.Projectiles.Count, "It reached the trap again and ended.");
        }

        [Test]
        public void Volley_ThatWouldOverfillTheSceneBudget_IsSkippedWhole()
        {
            var (runtime, trap, _) = Single(Type("t", "aim", new[] { Shots("p", 4) }, telegraph: .3f, cooldown: 100f), new Vector2(0f, 8f),
                maxActive: 3);
            Advance(runtime, 1.5f);
            Assert.AreEqual(0, runtime.Projectiles.Count);
            Assert.AreEqual(1, trap.VolleyIndex);
            Assert.AreEqual(TrapState.Idle, trap.State);
        }

        [Test]
        public void Cross_FiresFourSpearsAtNinetyDegrees_AndTheDiagonalVariantAlternates()
        {
            var layout = Layout(new[] { Projectile("p", speed: 1f) }, new[]
            {
                Type("diag", "fixed", new[] { Shots("p", 4, angleStep: 90f) }, telegraph: .3f, cooldown: .5f, headingStep: 45f, headingCycle: 2)
            }, 1000);
            var target = new FakeTrapPlayerTarget { Position = new Vector2(0f, 8f) };
            var set = One(layout, 0, Vector2.zero);
            var runtime = Runtime(layout, set, target);
            var trap = set.Traps[0];
            RunUntil(runtime, () => trap.VolleyIndex == 1);
            var first = runtime.Projectiles.Select(p => Mathf.Round(p.FacingDegrees)).OrderBy(v => v).ToArray();
            CollectionAssert.AreEqual(new[] { 0f, 90f, 180f, 270f }, first);
            RunUntil(runtime, () => trap.VolleyIndex == 2);
            var second = runtime.Projectiles.Skip(4).Select(p => Mathf.Round(p.FacingDegrees)).OrderBy(v => v).ToArray();
            CollectionAssert.AreEqual(new[] { 45f, 135f, 225f, 315f }, second);
        }

        [Test]
        public void ExplosiveBarrel_FusesWhenThePlayerComesClose_ThenHurtsOnlyWhoIsInTheBlast()
        {
            var layout = Layout(new[] { Projectile("p") }, new[] { Type("t", "aim", new[] { Shots("p") }) });
            var explosive = new TrapBarrelPlacement(Vector2.zero, true);
            var plain = new TrapBarrelPlacement(new Vector2(20f, 0f), false);
            var set = new TrapPlacementSet(new List<TrapPlacement>(), new List<TrapBarrelPlacement> { explosive, plain }, 0, 0);
            var target = new FakeTrapPlayerTarget { Position = new Vector2(1.5f, 0f) };
            var runtime = Runtime(layout, set, target);
            runtime.Tick(.02f, ScreenWidth);
            Assert.AreEqual(TrapBarrelState.Fusing, explosive.State);
            Assert.AreEqual(TrapBarrelState.Intact, plain.State);
            Advance(runtime, 1.1f);
            Assert.AreEqual(TrapBarrelState.Exploded, explosive.State);
            Assert.AreEqual(1, target.Hits.Count);
            Assert.AreEqual(18f, target.Hits[0].amount);
            Assert.AreEqual(new ContentId("TRAP-BARREL"), target.Hits[0].source);
        }

        [Test]
        public void ExplosiveBarrel_MissesAPlayerWhoRanOutOfTheBlastRadius_AndPlainBarrelsNeverFuse()
        {
            var layout = Layout(new[] { Projectile("p") }, new[] { Type("t", "aim", new[] { Shots("p") }) });
            var explosive = new TrapBarrelPlacement(Vector2.zero, true);
            var plain = new TrapBarrelPlacement(new Vector2(0f, 1f), false);
            var set = new TrapPlacementSet(new List<TrapPlacement>(), new List<TrapBarrelPlacement> { explosive, plain }, 0, 0);
            var target = new FakeTrapPlayerTarget { Position = new Vector2(1.5f, 0f) };
            var runtime = Runtime(layout, set, target);
            runtime.Tick(.02f, ScreenWidth);
            target.Position = new Vector2(6f, 0f);
            Advance(runtime, 1.2f);
            Assert.AreEqual(TrapBarrelState.Exploded, explosive.State);
            Assert.AreEqual(0, target.Hits.Count);
            Assert.AreEqual(TrapBarrelState.Intact, plain.State, "A plain barrel looks the same and does nothing.");
        }

        [Test]
        public void SpinningHead_TurnsWhileResting_StopsForTheVolley_AndFiresAlongItsPose()
        {
            var (runtime, trap, _) = Single(Type("t", "fixed", new[] { Shots("p") }, telegraph: 1f, cooldown: 100f, spin: 90f),
                new Vector2(0f, -8f), cooldown: 2f);
            Advance(runtime, 1f);
            Assert.AreEqual(90f, trap.HeadAngleDegrees, 1f, "A resting head turns at its spin speed.");
            RunUntil(runtime, () => trap.State == TrapState.Telegraph);
            var frozen = trap.HeadAngleDegrees;
            Assert.AreEqual(frozen, trap.LockedHeadingDegrees, 1e-3f, "The volley takes the pose the head stopped in.");
            Advance(runtime, .5f);
            Assert.AreEqual(frozen, trap.HeadAngleDegrees, 1e-3f, "The head stands still during the telegraph.");
            RunUntil(runtime, () => runtime.Projectiles.Count > 0);
            var radians = frozen * Mathf.Deg2Rad;
            Assert.AreEqual(Mathf.Cos(radians), runtime.Projectiles[0].Direction.x, 1e-3f);
            Assert.AreEqual(Mathf.Sin(radians), runtime.Projectiles[0].Direction.y, 1e-3f);
            Advance(runtime, .5f);
            Assert.AreNotEqual(frozen, trap.HeadAngleDegrees, "After the volley the head turns again.");
        }

        [Test]
        public void SeriesTrap_HeadSweepsWithTheShotsOfItsSpiral()
        {
            var (runtime, trap, _) = Single(Type("spiral", "fixed", new[] { Shots("p", 4, angleStep: 30f, delayStep: .3f) },
                telegraph: .3f, cooldown: 100f), new Vector2(0f, -8f), maxActive: 1000);
            var player = new Vector2(0f, -8f);
            Assert.AreEqual(0f, trap.HeadTargetDegrees(player), "At rest it shows the pose of the next volley.");
            RunUntil(runtime, () => trap.State == TrapState.Firing);
            var first = trap.HeadTargetDegrees(player);
            Assert.AreEqual(0f, first, 1e-3f, "The first shot leaves along the base heading.");
            Advance(runtime, .32f);
            Assert.AreEqual(30f, trap.HeadTargetDegrees(player), 1e-3f, "The head turned with the second shot.");
            Advance(runtime, .3f);
            Assert.AreEqual(60f, trap.HeadTargetDegrees(player), 1e-3f);
            Advance(runtime, .3f);
            Assert.AreEqual(TrapState.Idle, trap.State, "The last shot has left: the series is over.");
            Assert.AreEqual(90f, trap.LastShotAngleDegrees, 1e-3f, "The last shot of the spiral went out at 90 degrees.");
            Assert.AreEqual(0f, trap.HeadTargetDegrees(player), 1e-3f, "At rest the head returns to the pose of the next volley.");
        }

        [Test]
        public void AimedTrap_KeepsItsFiringPoseBrieflyAfterTheShot_ThenTurnsToThePlayer()
        {
            var (runtime, trap, player) = Single(Type("bow", "aim", new[] { Shots("p") }, telegraph: .3f, cooldown: 100f, aimResume: .4f),
                new Vector2(0f, 8f));
            RunUntil(runtime, () => trap.VolleyIndex == 1);
            var aimedAtShot = trap.LockedHeadingDegrees;
            Assert.AreEqual(90f, aimedAtShot, .5f);
            player.Position = new Vector2(8f, 0f); // the player moved away from the line of fire
            Advance(runtime, .2f);
            Assert.AreEqual(aimedAtShot, trap.HeadTargetDegrees(player.Position), 1e-3f, "Just after the shot the head has not turned yet.");
            Advance(runtime, .3f);
            Assert.AreEqual(0f, trap.HeadTargetDegrees(player.Position), .5f, "Shortly after, it tracks the player again.");
        }

        [Test]
        public void AimedTrap_WithoutResumeDelay_TracksAtOnce()
        {
            var (runtime, trap, player) = Single(Type("bow", "aim", new[] { Shots("p") }, telegraph: .3f, cooldown: 100f),
                new Vector2(0f, 8f));
            RunUntil(runtime, () => trap.VolleyIndex == 1);
            player.Position = new Vector2(8f, 0f);
            Assert.AreEqual(0f, trap.HeadTargetDegrees(player.Position), .5f);
        }

        [Test]
        public void NonSpinningHead_NeverTurns()
        {
            var (runtime, trap, _) = Single(Type("t", "fixed", new[] { Shots("p") }, cooldown: 100f), new Vector2(0f, -8f));
            Advance(runtime, 3f);
            Assert.AreEqual(0f, trap.HeadAngleDegrees);
        }

        [Test]
        public void Tick_WithZeroTime_ChangesNothing()
        {
            var (runtime, trap, _) = Single(Type("t", "aim", new[] { Shots("p") }), new Vector2(0f, 5f));
            var cooldown = trap.CooldownRemaining;
            runtime.Tick(0f, ScreenWidth);
            Assert.AreEqual(cooldown, trap.CooldownRemaining);
            Assert.AreEqual(0f, trap.RageSeconds);
            Assert.Throws<ArgumentOutOfRangeException>(() => runtime.Tick(-1f, ScreenWidth));
        }

        [Test]
        public void Clear_RemovesEveryProjectileAndDeactivatesTraps()
        {
            var (runtime, trap, _) = Single(Type("t", "aim", new[] { Shots("p", 3) }, telegraph: .3f, cooldown: 100f), new Vector2(0f, 8f));
            RunUntil(runtime, () => runtime.Projectiles.Count == 3);
            runtime.Clear();
            Assert.AreEqual(0, runtime.Projectiles.Count);
            Assert.IsFalse(trap.IsActive);
        }
    }
}
