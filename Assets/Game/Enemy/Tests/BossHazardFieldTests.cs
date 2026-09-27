using System;
using System.Linq;
using Game.Content;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    /// <summary>DECISION-0066 F1…F3: zones, beams and summons of <see cref="BossHazardField"/>.</summary>
    public sealed class BossHazardFieldTests
    {
        private static readonly ContentId Source = new ContentId("FIXTURE-BOSS-STEP");
        private const float Dt = 0.02f;

        private static BossHazardField Field(int seed = 7) => new BossHazardField(new System.Random(seed));

        private static int Hits(BossHazardField field, float seconds, Vector2 boss, Func<float, Vector2> player, bool running = true)
        {
            var hits = 0;
            for (var t = Dt; t <= seconds + 1e-4f; t += Dt)
            {
                field.Tick(Dt, running, boss, player(t));
                hits += field.Hits.Count;
            }
            return hits;
        }

        [Test]
        public void AtPlayer_HitsOnceWhenFilled_OnlyInside_AndPauseFreezesTheFill()
        {
            var field = Field();
            var zone = BossHazardTestData.Zone(BossZonePlacement.AtPlayer, radius: 2f, fill: 1f, damage: 26f);
            field.Start(BossSpecialRequest.ForZone(zone, Source), Vector2.zero, new Vector2(5f, 0f));
            Assert.AreEqual(0, Hits(field, 3f, Vector2.zero, _ => new Vector2(5f, 0f), running: false), "Pause freezes the fill.");
            Assert.AreEqual(0, Hits(field, 0.98f, Vector2.zero, _ => new Vector2(5f, 0f)), "No hit before the fill completes.");
            field.Tick(0.05f, true, Vector2.zero, new Vector2(6.5f, 0f));
            Assert.AreEqual(1, field.Hits.Count, "Inside the 2-unit circle when it completes.");
            Assert.AreEqual(26f, field.Hits[0].Damage);
            Assert.AreEqual(Source, field.Hits[0].SourceId);
            Assert.AreEqual(0, Hits(field, 1f, Vector2.zero, _ => new Vector2(6.5f, 0f)), "Hits once.");

            var outside = Field();
            outside.Start(BossSpecialRequest.ForZone(zone, Source), Vector2.zero, Vector2.zero);
            Assert.AreEqual(0, Hits(outside, 1.5f, Vector2.zero, _ => new Vector2(2.1f, 0f)), "Left the circle in time.");
            Assert.IsTrue(outside.IsEmpty, "Finished after the impact flash.");
        }

        [Test]
        public void AroundPlayer_PlacesSeparatedCirclesWithinScatter()
        {
            var field = Field();
            var zone = BossHazardTestData.Zone(BossZonePlacement.AroundPlayer, count: 5, radius: 1.2f, fill: 1.2f, scatter: 3f, spacing: 1.8f);
            var player = new Vector2(10f, -4f);
            field.Start(BossSpecialRequest.ForZone(zone, Source), Vector2.zero, player);
            var centers = field.Visuals.Where(v => v.Kind == BossHazardVisualKind.ZoneEdge).Select(v => v.Center).ToArray();
            Assert.AreEqual(5, centers.Length);
            foreach (var center in centers) Assert.LessOrEqual((center - player).magnitude, 3f + 1e-4f);
            for (var i = 0; i < centers.Length; i++)
                for (var j = i + 1; j < centers.Length; j++)
                    Assert.GreaterOrEqual((centers[i] - centers[j]).magnitude, 1.8f - 1e-4f);
        }

        [Test]
        public void Trail_EachCircleLandsWhereThePlayerIsWhenItAppears()
        {
            var field = Field();
            var zone = BossHazardTestData.Zone(BossZonePlacement.Trail, count: 4, radius: 1.4f, fill: 2f, interval: 0.5f);
            field.Start(BossSpecialRequest.ForZone(zone, Source), Vector2.zero, Vector2.zero);
            Vector2 Path(float t) => new Vector2(3f * t, 0f); // base speed 3 u/s straight line
            for (var t = Dt; t <= 1.55f; t += Dt) field.Tick(Dt, true, Vector2.zero, Path(t));
            var centers = field.Visuals.Where(v => v.Kind == BossHazardVisualKind.ZoneEdge).Select(v => v.Center.x).OrderBy(x => x).ToArray();
            Assert.AreEqual(4, centers.Length, "All four are placed and still filling at 1.55 s.");
            Assert.AreEqual(0f, centers[0], 0.07f);
            Assert.AreEqual(1.5f, centers[1], 0.07f);
            Assert.AreEqual(3.0f, centers[2], 0.07f);
            Assert.AreEqual(4.5f, centers[3], 0.07f);
        }

        [Test]
        public void SafeCircles_HitOutsideEveryCircle_NotInside()
        {
            var zone = BossHazardTestData.Zone(BossZonePlacement.SafeCircles, count: 2, radius: 1.8f, fill: 2f, scatter: 4.5f, spacing: 4f,
                damage: 50f);
            var outside = Field();
            outside.Start(BossSpecialRequest.ForZone(zone, Source), Vector2.zero, Vector2.zero);
            Assert.AreEqual(1, Hits(outside, 2.1f, Vector2.zero, _ => new Vector2(40f, 40f)));

            var inside = Field();
            inside.Start(BossSpecialRequest.ForZone(zone, Source), Vector2.zero, Vector2.zero);
            var safe = inside.Visuals.First(v => v.Kind == BossHazardVisualKind.SafeCircle).Center;
            Assert.AreEqual(2, inside.Visuals.Count(v => v.Kind == BossHazardVisualKind.SafeCircle));
            Assert.AreEqual(1, inside.Visuals.Count(v => v.Kind == BossHazardVisualKind.DangerWash));
            Assert.AreEqual(0, Hits(inside, 2.1f, Vector2.zero, _ => safe));
        }

        [Test]
        public void BurningGround_HitsEveryTickWhileStandingInIt_ForItsDuration()
        {
            var field = Field();
            var zone = BossHazardTestData.Zone(BossZonePlacement.AroundSelf, radius: 2f, fill: 0.5f, damage: 30f, linger: 3f, lingerDps: 11f,
                tick: 0.5f);
            field.Start(BossSpecialRequest.ForZone(zone, Source), Vector2.zero, Vector2.zero);
            var total = 0f;
            var count = 0;
            for (var t = Dt; t <= 4f; t += Dt)
            {
                field.Tick(Dt, true, Vector2.zero, new Vector2(1f, 0f));
                foreach (var hit in field.Hits) { total += hit.Damage; count++; }
            }
            Assert.AreEqual(1 + 6, count, "Impact plus six burns (3 s / 0.5 s).");
            Assert.AreEqual(30f + 6 * 5.5f, total, 1e-3f);
            Assert.IsTrue(field.IsEmpty);
        }

        [Test]
        public void Beam_HitsOnlyWhileActive_WithinHalfWidth_OnceAndSweeps()
        {
            var beam = BossHazardTestData.Beam(width: 1.2f, telegraph: 1f, active: 0.5f, damage: 36f);
            var field = Field();
            field.Start(BossSpecialRequest.ForBeam(beam, Source), Vector2.zero, new Vector2(10f, 0f));
            Assert.AreEqual(BossHazardVisualKind.BeamTelegraph, field.Visuals.Single().Kind);
            Assert.AreEqual(0, Hits(field, 0.98f, Vector2.zero, _ => new Vector2(10f, 0f)), "Telegraph never hits.");
            Assert.AreEqual(1, Hits(field, 0.6f, Vector2.zero, _ => new Vector2(10f, 0.5f)), "Within 0.6 of the line: one hit.");
            Assert.IsTrue(field.IsEmpty);

            var miss = Field();
            miss.Start(BossSpecialRequest.ForBeam(beam, Source), Vector2.zero, new Vector2(10f, 0f));
            Assert.AreEqual(0, Hits(miss, 1.6f, Vector2.zero, _ => new Vector2(10f, 0.7f)), "Outside half the width.");

            var sweeping = Field();
            sweeping.Start(BossSpecialRequest.ForBeam(BossHazardTestData.Beam(width: 1f, telegraph: 1f, active: 1f, sweep: 60f), Source),
                Vector2.zero, new Vector2(10f, 0f));
            var target = new Vector2(Mathf.Cos(50f * Mathf.Deg2Rad), Mathf.Sin(50f * Mathf.Deg2Rad)) * 8f;
            Assert.AreEqual(1, Hits(sweeping, 2.05f, Vector2.zero, _ => target), "The sweep reaches a player 50° off the start line.");
        }

        [Test]
        public void Beam_SeveralAngles_FromTheDirectionToThePlayer()
        {
            var field = Field();
            field.Start(BossSpecialRequest.ForBeam(BossHazardTestData.Beam(new[] { -30f, 0f, 30f }), Source), Vector2.zero, Vector2.up * 5f);
            var angles = field.Visuals.Select(v => v.AngleDegrees).OrderBy(a => a).ToArray();
            CollectionAssert.AreEqual(new[] { 60f, 90f, 120f }, angles.Select(a => Mathf.Round(a)).ToArray());
        }

        [Test]
        public void Summon_ReleasesAfterTelegraph_AroundThePlayer_AndRespectsTheAliveLimit()
        {
            var field = Field();
            var summon = BossHazardTestData.Summon(count: 4, maxAlive: 8);
            var player = new Vector2(3f, 3f);
            field.Start(BossSpecialRequest.ForSummon(summon, Source), Vector2.zero, player);
            Assert.AreEqual(4, field.PendingSummons(summon));
            field.Start(BossSpecialRequest.ForSummon(summon, Source), Vector2.zero, player, aliveSummons: 3);
            Assert.AreEqual(5, field.PendingSummons(summon), "3 alive + 4 pending leave room for one more.");
            var released = 0;
            for (var t = Dt; t <= 0.9f; t += Dt)
            {
                field.Tick(Dt, true, Vector2.zero, player);
                foreach (var spawn in field.Spawns)
                {
                    released++;
                    Assert.AreEqual(6f, (spawn.Position - player).magnitude, 1e-3f);
                }
            }
            Assert.AreEqual(5, released);
            Assert.AreEqual(0, field.PendingSummons(summon));
        }

        [Test]
        public void Clear_DropsEverything()
        {
            var field = Field();
            field.Start(BossSpecialRequest.ForZone(BossHazardTestData.Zone(BossZonePlacement.AtPlayer), Source), Vector2.zero, Vector2.zero);
            field.Start(BossSpecialRequest.ForBeam(BossHazardTestData.Beam(), Source), Vector2.zero, Vector2.right);
            field.Start(BossSpecialRequest.ForSummon(BossHazardTestData.Summon(), Source), Vector2.zero, Vector2.zero);
            field.Clear();
            Assert.IsTrue(field.IsEmpty);
            Assert.AreEqual(0, field.Visuals.Count);
            Assert.AreEqual(0, Hits(field, 3f, Vector2.zero, _ => Vector2.zero));
        }

        [Test]
        public void Profiles_RejectInconsistentAuthoring()
        {
            Assert.Throws<ArgumentException>(() => BossHazardTestData.Zone(BossZonePlacement.AtPlayer, count: 2));
            Assert.Throws<ArgumentException>(() => BossHazardTestData.Zone(BossZonePlacement.AroundPlayer, count: 3));
            Assert.Throws<ArgumentException>(() => BossHazardTestData.Zone(BossZonePlacement.Trail, count: 3));
            Assert.Throws<ArgumentException>(() => BossHazardTestData.Zone(BossZonePlacement.AtPlayer, linger: 3f));
            Assert.Throws<ArgumentException>(() => BossHazardTestData.Zone(BossZonePlacement.SafeCircles, count: 2, scatter: 4f, spacing: 4f,
                linger: 1f, lingerDps: 1f, tick: .5f));
            Assert.Throws<ArgumentException>(() => BossHazardTestData.Beam(new float[0]));
            Assert.Throws<ArgumentException>(() => BossHazardTestData.Summon(count: 5, maxAlive: 4));
            Assert.Throws<ArgumentException>(() => new BossSpecialAttack(3f));
            Assert.Throws<ArgumentException>(() => new BossSpecialAttack(3f, BossHazardTestData.Zone(BossZonePlacement.AtPlayer),
                BossHazardTestData.Beam()));
        }
    }
}
