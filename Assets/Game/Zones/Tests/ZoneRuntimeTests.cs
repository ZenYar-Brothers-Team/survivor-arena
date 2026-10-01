using System.Collections.Generic;
using System.Linq;
using Game.Zones.Json;
using NUnit.Framework;
using UnityEngine;

namespace Game.Zones.Tests
{
    public sealed class ZoneRuntimeTests
    {
        private const float Dt = 0.1f;
        private FakeZonePlayer _player;
        private FakeZoneEnemies _enemies;

        [SetUp]
        public void SetUp()
        {
            _player = new FakeZonePlayer();
            _enemies = new FakeZoneEnemies();
        }

        private static ZoneEffectDefinition Effect(ZoneEffectData data) => new ZoneEffectDefinition(data);

        private ZoneRuntime Build(int seed, params (ZoneEffectData effect, Vector2 center, float phase)[] zones)
        {
            var layoutData = ZoneTestData.Layout(zones.Select(z => z.effect).GroupBy(e => e.Id).Select(g => g.First()).ToArray(),
                zones.Select(z => z.effect.Id).Distinct().Select(id => (id, zones.Count(z => z.effect.Id == id))).ToArray());
            var layout = new ZoneLayoutDefinition(layoutData);
            var placements = new List<ZonePlacement>();
            for (var i = 0; i < zones.Length; i++)
                placements.Add(new ZonePlacement(i, layout.Effects[Effect(zones[i].effect).Id], zones[i].center, zones[i].phase));
            return new ZoneRuntime(placements, new ZonePlacementRules(layout, 120f, Vector2.zero, null), seed, _player, _enemies);
        }

        private static void Run(ZoneRuntime runtime, float seconds)
        {
            for (var t = 0f; t < seconds - 1e-4f; t += Dt) runtime.Tick(Dt);
        }

        [Test]
        public void Slow_SlowsThePlayerAndEnemiesInside_AndReleasesThemOutside()
        {
            var runtime = Build(1, (ZoneTestData.Slow(), Vector2.zero, 0f));
            _enemies.Add(new Vector2(2, 0)).Add(new Vector2(30, 0));
            _player.Position = new Vector2(1, 0);
            runtime.Tick(Dt);
            Assert.AreEqual(-0.4f, _player.Zone.MovementSpeedMultiplierBonus, 1e-4f);
            Assert.AreEqual(0.5f, _enemies.SlowFractions[0], 1e-4f);
            Assert.AreEqual(0f, _enemies.SlowFractions[1], "An enemy outside the rim is untouched.");
            _player.Position = new Vector2(20, 0);
            runtime.Tick(Dt);
            Assert.IsFalse(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey), "Leaving removes the modifier.");
        }

        [Test]
        public void Slow_WithoutEnemyFraction_NeverEnumeratesEnemies()
        {
            var data = ZoneTestData.Slow(); data.EnemySlowFraction = 0f;
            var runtime = Build(1, (data, Vector2.zero, 0f));
            _enemies.Add(Vector2.zero);
            runtime.Tick(Dt);
            Assert.AreEqual(0, _enemies.Refreshes, "Only zones that touch enemies pay for the enemy scan.");
            Assert.AreEqual(0f, _enemies.SlowFractions[0]);
        }

        [Test]
        public void Haste_Regeneration_AndArcane_FeedThePlayerModifier_AndStack()
        {
            var runtime = Build(1,
                (ZoneTestData.Haste(), new Vector2(0, 0), 0f),
                (ZoneTestData.Regeneration(), new Vector2(3, 0), 0f),
                (ZoneTestData.Arcane(), new Vector2(0, 3), 0f),
                (ZoneTestData.Slow(), new Vector2(-3, 0), 0f));
            _player.Position = Vector2.zero; // inside all four overlapping discs
            runtime.Tick(Dt);
            var modifier = _player.Zone;
            Assert.AreEqual(0.35f - 0.4f, modifier.MovementSpeedMultiplierBonus, 1e-4f, "Haste and slow add up.");
            Assert.AreEqual(4f, modifier.HealthRegenerationPerSecondBonus, 1e-4f);
            Assert.AreEqual(0.5f, modifier.ActiveSkillDamageMultiplierBonus, 1e-4f);
            Assert.AreEqual(0.25f, modifier.ActionSpeedBonus, 1e-4f);
            Assert.AreEqual(4, runtime.PlayerActiveZoneCount);
        }

        [Test]
        public void Rift_DamagesThePlayerAndEnemiesPerSecond_ByTheFrameTime()
        {
            var runtime = Build(1, (ZoneTestData.Rift(), Vector2.zero, 0f));
            _enemies.Add(new Vector2(1, 1)).Add(new Vector2(40, 0));
            _player.Position = new Vector2(1, 0);
            Run(runtime, 2f);
            Assert.AreEqual(6f * 2f, _player.DamageTaken, 0.01f);
            Assert.AreEqual(20f * 2f, _enemies.DamageTaken[0], 0.01f);
            Assert.AreEqual(0f, _enemies.DamageTaken[1]);
        }

        [Test]
        public void Portal_TeleportsBesideThePartner_ThenWaitsForTheCooldown()
        {
            var runtime = Build(1, (ZoneTestData.Portal(), new Vector2(-30, 0), 0f), (ZoneTestData.Portal(), new Vector2(30, 0), 0f));
            // The generator pairs portals; the manual placements here are paired by hand.
            runtime.Zones[0].LinkPartner(1);
            runtime.Zones[1].LinkPartner(0);
            _player.Position = new Vector2(-30, 0);
            runtime.Tick(Dt);
            Assert.AreEqual(1, _player.Teleports.Count);
            Assert.AreEqual(new Vector2(25f, 0f).x, _player.Teleports[0].x, 1e-3f, "Arrives radius 3 + exit 2 from the partner, toward the arena center.");
            Assert.AreEqual(3f, runtime.PortalCooldownRemaining, 1e-3f);
            _player.Position = new Vector2(30, 0); // stepping into the partner during the cooldown does nothing
            Run(runtime, 1f);
            Assert.AreEqual(1, _player.Teleports.Count);
            Run(runtime, 2.2f);
            Assert.AreEqual(2, _player.Teleports.Count, "After the cooldown the partner portal sends the player back.");
            Assert.AreEqual(-25f, _player.Teleports[1].x, 1e-3f);
        }

        [Test]
        public void Pulsing_ZoneWorksOnlyWhileShownEnough_AndReleasesWhenItVanishes()
        {
            var runtime = Build(1, (ZoneTestData.Haste(mode: ZoneLifetimeMode.Pulsing), Vector2.zero, 0f));
            _player.Position = Vector2.zero;
            runtime.Tick(Dt);
            Assert.IsFalse(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey), "Invisible at the start of its cycle.");
            Run(runtime, 2f);
            Assert.AreEqual(0.35f, _player.Zone.MovementSpeedMultiplierBonus, 1e-4f, "Half faded in: the effect works.");
            Run(runtime, 15f);
            Assert.IsTrue(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey), "Fully shown at 17 s.");
            Run(runtime, 3f);
            Assert.IsFalse(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey), "Vanished by 20 s.");
        }

        [Test]
        public void Pulsing_ZoneReappearsAtANewRandomPoint_EachCycle_ObeyingTheClearances()
        {
            var runtime = Build(7, (ZoneTestData.Haste(mode: ZoneLifetimeMode.Pulsing), new Vector2(40, 40), 0f),
                (ZoneTestData.Regeneration(), new Vector2(-40, -40), 0f));
            var zone = runtime.Zones[0];
            var spots = new List<Vector2> { zone.Center };
            for (var cycle = 1; cycle <= 6; cycle++)
            {
                Run(runtime, 30.5f);
                Assert.AreEqual(cycle, zone.Cycle);
                spots.Add(zone.Center);
                Assert.GreaterOrEqual(zone.Center.magnitude, 8f + zone.Effect.Radius - 1e-3f, "Never on the start circle.");
                Assert.LessOrEqual(Mathf.Abs(zone.Center.x), 54f - zone.Effect.Radius + 1e-3f);
                Assert.GreaterOrEqual(Vector2.Distance(zone.Center, runtime.Zones[1].Center), zone.Effect.Radius + 5f + 4f - 1e-3f, "Clear of the other zone.");
            }
            Assert.Greater(spots.Distinct().Count(), 5, "A fresh random spot each cycle.");
            Assert.AreEqual(new Vector2(-40, -40), runtime.Zones[1].Center, "A permanent zone never moves.");
        }

        [Test]
        public void Pulsing_Relocation_IsDeterministicForASeed_AndDiffersBetweenSeeds()
        {
            Vector2 After(int seed)
            {
                var runtime = Build(seed, (ZoneTestData.Haste(mode: ZoneLifetimeMode.Pulsing), new Vector2(40, 40), 0f));
                Run(runtime, 30.5f);
                return runtime.Zones[0].Center;
            }
            Assert.AreEqual(After(5), After(5));
            Assert.AreNotEqual(After(5), After(6));
        }

        [Test]
        public void DeadPlayer_GetsNoEffectsAndNoModifier_AndDisposeRemovesTheModifier()
        {
            var runtime = Build(1, (ZoneTestData.Regeneration(), Vector2.zero, 0f), (ZoneTestData.Rift(), new Vector2(40, 0), 0f));
            _player.Position = Vector2.zero;
            runtime.Tick(Dt);
            Assert.IsTrue(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey));
            _player.IsAlive = false;
            runtime.Tick(Dt);
            Assert.IsFalse(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey));
            _player.IsAlive = true;
            runtime.Tick(Dt);
            runtime.Dispose();
            Assert.IsFalse(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey), "Dispose releases the zones' hold on the player.");
        }

        [Test]
        public void Tick_ZeroTimeChangesNothing_SoAPausedRunStandsStill()
        {
            var runtime = Build(1, (ZoneTestData.Rift(), Vector2.zero, 0f));
            _player.Position = Vector2.zero;
            runtime.Tick(0f);
            Assert.AreEqual(0f, runtime.Time);
            Assert.AreEqual(0f, _player.DamageTaken);
        }
    }
}
