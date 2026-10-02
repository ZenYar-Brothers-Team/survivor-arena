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
            _view = WholeArena;
            _player = new FakeZonePlayer();
            _enemies = new FakeZoneEnemies();
        }

        private static ZoneEffectDefinition Effect(ZoneEffectData data) => new ZoneEffectDefinition(data);

        // A screen far larger than the arena, so tests that are not about the active window see every zone near.
        private static readonly Rect WholeArena = new Rect(-100f, -100f, 200f, 200f);
        private Rect _view = WholeArena;

        private ZoneRuntime Build(int seed, params (ZoneEffectData effect, Vector2 center, float phase)[] zones)
        {
            var layoutData = ZoneTestData.Layout(zones.Select(z => z.effect).GroupBy(e => e.Id).Select(g => g.First()).ToArray(),
                zones.Select(z => z.effect.Id).Distinct().Select(id => (id, zones.Count(z => z.effect.Id == id))).ToArray());
            var layout = new ZoneLayoutDefinition(layoutData);
            var placements = new List<ZonePlacement>();
            for (var i = 0; i < zones.Length; i++)
                placements.Add(new ZonePlacement(i, layout.Effects[Effect(zones[i].effect).Id], zones[i].center, zones[i].phase));
            return new ZoneRuntime(placements, new ZonePlacementRules(layout, 120f, Vector2.zero, null), seed, _player, _enemies,
                () => _view);
        }

        private static void Run(ZoneRuntime runtime, float seconds)
        {
            for (var t = 0f; t < seconds - 1e-4f; t += Dt) runtime.Tick(Dt);
        }

        [Test]
        public void SpeedBuffProjection_OutlastsZone_DecreasesAndClearsOnDispose()
        {
            var runtime = Build(1, (ZoneTestData.SpeedBurst(), Vector2.zero, 0f));
            _player.Position = Vector2.zero;
            runtime.Tick(4f);
            Assert.AreEqual(1f, runtime.SpeedBuffRemaining01);
            _player.Position = new Vector2(40f, 40f);
            runtime.Tick(4f);
            Assert.AreEqual(.5f, runtime.SpeedBuffRemaining01);
            runtime.Dispose();
            Assert.AreEqual(0f, runtime.SpeedBuffRemaining01);
            Assert.IsFalse(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey));
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
        public void Protection_CutsIncomingDamageWhileInside_AndStacksWithOtherBonuses()
        {
            var runtime = Build(1, (ZoneTestData.Protection(), Vector2.zero, 0f));
            _player.Position = Vector2.zero;
            runtime.Tick(Dt);
            Assert.AreEqual(0.8f, _player.Zone.IncomingDamageReductionBonus, 1e-4f);
            _player.Position = new Vector2(30, 0);
            runtime.Tick(Dt);
            Assert.IsFalse(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey));
        }

        [Test]
        public void Protection_PulsingFadesOutSlowly_ThenReleasesThePlayer()
        {
            var ward = ZoneTestData.Protection(mode: ZoneLifetimeMode.Pulsing);
            ward.PulsePeriodSeconds = 40f; ward.PulseVisibleSeconds = 26f; ward.PulseFadeSeconds = 6f;
            var runtime = Build(1, (ward, Vector2.zero, 0f));
            _player.Position = Vector2.zero;
            Run(runtime, 10f);
            Assert.AreEqual(0.8f, _player.Zone.IncomingDamageReductionBonus, 1e-4f);
            Run(runtime, 12.5f); // 22.5 s: well into the 6 s fade-out but still more than half there
            Assert.IsTrue(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey), "Still working while it fades.");
            Run(runtime, 1.5f);
            Assert.IsFalse(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey), "Gone once it has mostly faded.");
        }

        [Test]
        public void SpeedBurst_GoesOffAtTheEndOfItsTelegraph_GivingATimedBuffThatOutlastsTheZone()
        {
            var effect = ZoneTestData.SpeedBurst();
            var runtime = Build(1, (effect, Vector2.zero, 0f));
            _player.Position = new Vector2(3, 0);
            Run(runtime, 3.5f);
            Assert.IsFalse(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey), "Nothing happens while it is still swelling.");
            Run(runtime, 0.8f);
            Assert.AreEqual(0.6f, _player.Zone.MovementSpeedMultiplierBonus, 1e-4f, "The buff lands the instant the telegraph ends.");
            _player.Position = new Vector2(50, 0); // leaving does not cancel it
            Run(runtime, 5f);
            Assert.AreEqual(0.6f, _player.Zone.MovementSpeedMultiplierBonus, 1e-4f, "The buff outlasts the zone and the player's position.");
            var definition = runtime.Zones[0].Effect;
            Assert.Greater(runtime.BuffRemaining(definition), 0f);
            Run(runtime, 4f);
            Assert.IsFalse(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey), "The buff runs out after its 8 s.");
            Assert.AreEqual(0f, runtime.BuffRemaining(definition));
        }

        [Test]
        public void SpeedBurst_BuffsOnlyAPlayerInsideTheRadius()
        {
            var runtime = Build(1, (ZoneTestData.SpeedBurst(), Vector2.zero, 0f));
            _player.Position = new Vector2(30, 0); // outside the 8-unit radius when it goes off
            Run(runtime, 5f);
            Assert.IsFalse(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey), "A player outside the radius gets nothing.");
            Run(runtime, 35.5f); // the next cycle begins: the zone has moved on to a new random spot
            _player.Position = runtime.Zones[0].Center;
            Run(runtime, 4f);
            Assert.AreEqual(0.6f, _player.Zone.MovementSpeedMultiplierBonus, 1e-4f, "Waiting inside the new spot earns the buff.");
        }

        [Test]
        public void SpeedBurst_TwoBurstsOfOneEffect_RefreshTheBuffInsteadOfStacking()
        {
            var effect = ZoneTestData.SpeedBurst();
            var runtime = Build(1, (effect, new Vector2(0, 0), 0f), (effect, new Vector2(2, 0), 1f)); // they go off at 4 s and 3 s
            _player.Position = new Vector2(1, 0);
            Run(runtime, 4.5f);
            Assert.AreEqual(0.6f, _player.Zone.MovementSpeedMultiplierBonus, 1e-4f, "Not +120%: one buff per effect.");
            Assert.Greater(runtime.BuffRemaining(runtime.Zones[0].Effect), 7.4f, "The later burst refreshed the duration.");
        }

        [Test]
        public void SpeedBurst_ZoneReappearsElsewhereEachCycle_ButStaysGoneBetweenBursts()
        {
            var runtime = Build(3, (ZoneTestData.SpeedBurst(), new Vector2(40, 40), 0f));
            var zone = runtime.Zones[0];
            var spots = new List<Vector2> { zone.Center };
            for (var cycle = 1; cycle <= 4; cycle++)
            {
                Run(runtime, 40.5f);
                Assert.AreEqual(cycle, zone.Cycle);
                spots.Add(zone.Center);
            }
            Assert.Greater(spots.Distinct().Count(), 3, "A new random point each cycle.");
            Run(runtime, 18f);
            Assert.AreEqual(0f, zone.Visibility(runtime.Time), "Mid-cycle, after the flash, it is hidden.");
        }

        [Test]
        public void ActiveWindow_IsTheScreenPlusHalfAScreenOnEverySide()
        {
            var window = ZoneRuntime.WindowOf(new Rect(-8.9f, -5f, 17.8f, 10f), 0.5f);
            Assert.AreEqual(35.6f, window.width, 1e-3f, "Two screens wide: the screen and half a screen on each side.");
            Assert.AreEqual(20f, window.height, 1e-3f);
            Assert.AreEqual(-17.8f, window.xMin, 1e-3f);
            Assert.AreEqual(-10f, window.yMin, 1e-3f);
            var none = ZoneRuntime.WindowOf(default, 0.5f);
            Assert.Greater(none.width, 1e5f, "Without a camera there is no limit.");
        }

        [Test]
        public void FarZones_DoNothing_NoEffectsNoEnemyScanNoDiscs()
        {
            _view = new Rect(-8.9f, -5f, 17.8f, 10f); // window: x -17.8..17.8, y -10..10
            var runtime = Build(1, (ZoneTestData.Slow(), new Vector2(45, 0), 0f), (ZoneTestData.Rift(), new Vector2(0, 0), 0f));
            _enemies.Add(new Vector2(45, 0)); // inside the far slow zone, outside the window
            _player.Position = new Vector2(45, 0); // the player stands in it too, but the camera has not followed (test setup)
            runtime.Tick(Dt);
            Assert.IsFalse(runtime.Zones[0].IsNear);
            Assert.IsTrue(runtime.Zones[1].IsNear);
            Assert.AreEqual(1, runtime.NearZoneCount);
            Assert.AreEqual(0f, _enemies.SlowFractions[0], "A zone outside the window slows nobody.");
            Assert.IsFalse(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey), "...and does not touch the player either.");
        }

        [Test]
        public void Zones_WakeUpWhenTheScreenReachesThem()
        {
            _view = new Rect(-8.9f, -5f, 17.8f, 10f);
            var runtime = Build(1, (ZoneTestData.Haste(), new Vector2(26, 0), 0f)); // radius 5: its rim reaches x = 21, window edge 17.8
            _player.Position = new Vector2(26, 0);
            runtime.Tick(Dt);
            Assert.IsFalse(runtime.Zones[0].IsNear);
            _view = new Rect(8.5f, -5f, 17.8f, 10f); // the camera follows the player toward the zone
            runtime.Tick(Dt);
            Assert.IsTrue(runtime.Zones[0].IsNear);
            Assert.AreEqual(0.35f, _player.Zone.MovementSpeedMultiplierBonus, 1e-4f);
        }

        [Test]
        public void Relocation_PutsAReappearingZoneInsideTheActiveWindow()
        {
            _view = new Rect(30f, 30f, 17.8f, 10f); // window around (38.9, 35): x 21.1..56.7, y 25..45, inside the 54-unit arena half
            var runtime = Build(2, (ZoneTestData.Haste(mode: ZoneLifetimeMode.Pulsing), new Vector2(-40, -40), 0f));
            var zone = runtime.Zones[0];
            Run(runtime, 30.5f);
            Assert.AreEqual(1, zone.Cycle);
            Assert.IsTrue(runtime.ActiveWindow.Contains(zone.Center), $"It reappeared at {zone.Center}, inside {runtime.ActiveWindow}.");
            Assert.IsTrue(zone.IsNear);
        }

        [Test]
        public void CyclingAltar_NeverMoves_AndWorksOnlyInItsShortWindowOfTheLongCycle()
        {
            var runtime = Build(1, (ZoneTestData.Altar(), new Vector2(30, 30), 0f));
            var zone = runtime.Zones[0];
            _player.Position = new Vector2(30, 30);
            Run(runtime, 10f);
            Assert.AreEqual(0.5f, _player.Zone.MovementSpeedMultiplierBonus, 1e-4f, "On at 10 s.");
            Run(runtime, 25f); // 35 s: resting
            Assert.IsFalse(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey), "Resting at 35 s: the player has to go elsewhere.");
            Run(runtime, 60f); // 95 s: the next cycle is on again
            Assert.AreEqual(0.5f, _player.Zone.MovementSpeedMultiplierBonus, 1e-4f);
            Assert.AreEqual(new Vector2(30, 30), zone.Center, "An altar never relocates, cycle after cycle.");
            Assert.AreEqual(1, zone.CycleAt(runtime.Time), "It has crossed into the second cycle without moving.");
        }

        [Test]
        public void Strike_HitsEveryoneInsideItsCircleOnce_WhenTheWarningEnds_AndNoOneElse()
        {
            var runtime = Build(1, (ZoneTestData.Strike(), Vector2.zero, 0f));
            var effect = runtime.Zones[0].Effect;
            var circle = effect.StrikeCenter(Vector2.zero, 1, 0, 0, 0);
            Assert.LessOrEqual(circle.magnitude, effect.Radius - effect.StrikeRadius + 1e-3f, "Circles land fully inside the altar.");
            _player.Position = circle;
            _enemies.Add(circle).Add(new Vector2(40, 0));
            Run(runtime, 2.9f);
            Assert.AreEqual(0f, _player.HitTaken, "Nothing lands while the warning still fills.");
            Run(runtime, 0.3f);
            Assert.GreaterOrEqual(_player.HitTaken, 10f, "The player standing in the circle is hit when the warning ends.");
            Assert.GreaterOrEqual(_enemies.StrikeTaken[0], 50f, "So is an enemy inside it.");
            Assert.AreEqual(0f, _enemies.StrikeTaken[1], "An enemy far away is spared.");
            var taken = _player.HitTaken;
            Run(runtime, 10f);
            Assert.AreEqual(taken, _player.HitTaken, "One hit per volley, not per frame.");
        }

        [Test]
        public void Strike_NextVolleyLandsElsewhere_AndThePlayerCanDodge()
        {
            var runtime = Build(1, (ZoneTestData.Strike(), Vector2.zero, 0f));
            var effect = runtime.Zones[0].Effect;
            Assert.AreNotEqual(effect.StrikeCenter(Vector2.zero, 1, 0, 0, 0), effect.StrikeCenter(Vector2.zero, 1, 0, 1, 0));
            Assert.AreEqual(effect.StrikeCenter(Vector2.zero, 1, 0, 3, 1), effect.StrikeCenter(Vector2.zero, 1, 0, 3, 1), "Deterministic.");
            _player.Position = new Vector2(60, 0); // far outside every circle
            Run(runtime, 25f);
            Assert.AreEqual(0f, _player.HitTaken);
        }

        [Test]
        public void Strike_EnemiesOnlyAltar_NeverHurtsThePlayer_AndCyclingAltarStrikesOnlyWhileOn()
        {
            var data = ZoneTestData.Strike(polarity: ZoneAltarPolarity.Positive);
            data.StrikePlayerDamage = 0f;
            var runtime = Build(1, (data, Vector2.zero, 0f));
            var circle = runtime.Zones[0].Effect.StrikeCenter(Vector2.zero, 1, 0, 0, 0);
            _player.Position = circle;
            _enemies.Add(circle);
            Run(runtime, 3.2f);
            Assert.AreEqual(0f, _player.HitTaken);
            Assert.GreaterOrEqual(_enemies.StrikeTaken[0], 50f);

            var cycling = ZoneTestData.Strike();
            cycling.Lifetime = ZoneLifetimeMode.Cycling;
            cycling.PulsePeriodSeconds = 100f; cycling.PulseVisibleSeconds = 10f; cycling.PulseFadeSeconds = 1f;
            _player = new FakeZonePlayer(); _enemies = new FakeZoneEnemies();
            runtime = Build(1, (cycling, Vector2.zero, 0f));
            Run(runtime, 4f); // the first volley (t = 3) goes off while the altar is on
            _player.HitTaken = 0f;
            _player.Position = runtime.Zones[0].Effect.StrikeCenter(Vector2.zero, 1, 0, 1, 0);
            Run(runtime, 20f); // the volley at t = 23 (second volley, 20 s period) falls in the resting part of the cycle
            Assert.AreEqual(0f, _player.HitTaken, "A resting altar does not strike.");
        }

        [Test]
        public void StrikeCircles_AreCollectedWhileWarning_AndEmptyOtherwise()
        {
            var runtime = Build(1, (ZoneTestData.Strike(), Vector2.zero, 0f));
            var circles = new List<StrikeCircle>();
            Run(runtime, 1.5f);
            runtime.CollectStrikeCircles(circles);
            Assert.AreEqual(2, circles.Count);
            Assert.AreEqual(0.5f, circles[0].Telegraph, 0.05f);
            Assert.AreEqual(0f, circles[0].Flash);
            Run(runtime, 2.0f); // 3.5 s: flashing
            runtime.CollectStrikeCircles(circles);
            Assert.AreEqual(2, circles.Count);
            Assert.Greater(circles[0].Flash, 0f);
            Run(runtime, 3f); // 6.5 s: quiet
            runtime.CollectStrikeCircles(circles);
            Assert.AreEqual(0, circles.Count);
        }

        [Test]
        public void EnemyAltars_StrengthenOnlyEnemiesInside_AndNeverTouchThePlayer()
        {
            var runtime = Build(1, (ZoneTestData.EnemyAltar(ZoneEffectKind.EnemyHaste, "T-EH"), Vector2.zero, 0f),
                (ZoneTestData.EnemyAltar(ZoneEffectKind.EnemyRegeneration, "T-ER"), Vector2.zero, 0f),
                (ZoneTestData.EnemyAltar(ZoneEffectKind.EnemyProtection, "T-EP"), Vector2.zero, 0f),
                (ZoneTestData.EnemyAltar(ZoneEffectKind.EnemyPower, "T-EW"), Vector2.zero, 0f));
            _enemies.Add(new Vector2(1, 0));
            _player.Position = new Vector2(1, 0);
            Run(runtime, 5f);
            Assert.AreEqual(0.5f, _enemies.MovementBonus, 1e-4f);
            Assert.AreEqual(5f, _enemies.Regeneration, 1e-4f);
            Assert.AreEqual(0.8f, _enemies.Defense, 1e-4f);
            Assert.AreEqual(0.5f, _enemies.DamageBonus, 1e-4f);
            Assert.AreEqual(0, _player.Modifiers.Count, "The player is not affected by enemy altars.");
            Assert.AreEqual(0f, _player.DamageTaken);
            _enemies.Positions[0] = new Vector2(50, 0);
            Run(runtime, 0.2f);
            Assert.AreEqual(0f, _enemies.MovementBonus); Assert.AreEqual(0f, _enemies.DamageBonus);
        }

        [Test]
        public void ExperienceShrine_MultipliesPickedUpExperienceForItsDuration()
        {
            var runtime = Build(1, (ZoneTestData.ExperienceShrine(), Vector2.zero, 0f));
            var zone = runtime.Zones[0];
            _player.Position = new Vector2(1, 0);
            Run(runtime, 4.3f);
            Assert.AreEqual(4f, _player.Zone.PickedUpXpMultiplierBonus, 1e-4f, "x5 is a +4 bonus on the picked-up multiplier.");
            Assert.Greater(runtime.ExperienceRemaining(zone.Effect), 25f);
            _player.Position = new Vector2(40, 40);
            Run(runtime, 31f);
            Assert.AreEqual(0f, _player.Zone.PickedUpXpMultiplierBonus, 1e-4f, "The multiplier ends after 30 s.");
        }

        [Test]
        public void Shrine_FiresEveryRewardOnceWhenFull_ThenRestsForItsCooldown()
        {
            var runtime = Build(1, (ZoneTestData.Shrine(), Vector2.zero, 0f));
            var zone = runtime.Zones[0];
            _player.Position = new Vector2(1, 0);
            _enemies.Add(new Vector2(3, 0)).Add(new Vector2(30, 0));
            Run(runtime, 9f);
            Assert.AreEqual(0.9f, zone.Charge, 0.03f);
            Assert.AreEqual(0f, _player.Healed, "Nothing before it is full.");
            Run(runtime, 1.2f);
            Assert.AreEqual(0.25f, _player.Healed, 1e-4f, "Healing fires once.");
            Assert.AreEqual(100f, _enemies.StrikeTaken[0], 1e-3f, "The blast hits the enemy near the shrine.");
            Assert.AreEqual(0f, _enemies.StrikeTaken[1], "An enemy beyond the blast radius is spared.");
            Run(runtime, 0.2f);
            Assert.AreEqual(0.5f, _player.Zone.MovementSpeedMultiplierBonus, 1e-4f, "The timed buff is on.");
            Assert.AreEqual(0.5f, _player.Zone.IncomingDamageReductionBonus, 1e-4f, "So is the shield.");
            Assert.Greater(runtime.BuffRemaining(zone.Effect), 7f);
            Assert.Greater(runtime.ShieldRemaining(zone.Effect), 4f);
            Assert.Greater(zone.ShrineCooldownRemaining, 59f);
            Assert.IsFalse(zone.IsActive(runtime.Time), "Resting: it does nothing.");
            Assert.AreEqual(0f, zone.RestProgress(runtime.Time), 0.05f, "The cooldown ring starts empty.");

            Run(runtime, 30f);
            Assert.AreEqual(0.25f, _player.Healed, 1e-4f, "No second reward while it rests, even standing inside.");
            Assert.AreEqual(0f, zone.Charge);
            Assert.AreEqual(0.5f, zone.RestProgress(runtime.Time), 0.05f);
            Assert.IsFalse(_player.Modifiers.TryGetValue(ZoneRuntime.ModifierKey, out _) && _player.Zone.MovementSpeedMultiplierBonus > 0f,
                "The timed buff has run out by now.");
            Run(runtime, 31f);
            Assert.AreEqual(0f, zone.ShrineCooldownRemaining, 1e-3f);
            Run(runtime, 11f);
            Assert.AreEqual(0.5f, _player.Healed, 1e-4f, "It fires again after the cooldown and a new fill.");
        }

        [Test]
        public void Shrine_ProgressDrainsWhenTheCharacterLeaves_AndDeathResetsIt()
        {
            var runtime = Build(1, (ZoneTestData.Shrine(), Vector2.zero, 0f));
            var zone = runtime.Zones[0];
            _player.Position = Vector2.zero;
            Run(runtime, 5f);
            Assert.AreEqual(0.5f, zone.Charge, 0.03f);
            _player.Position = new Vector2(40, 0);
            Run(runtime, 1.25f);
            Assert.AreEqual(0.25f, zone.Charge, 0.03f, "A full charge drains over 5 s outside, so a quarter in 1.25 s.");
            _player.Position = Vector2.zero;
            Run(runtime, 2f);
            _player.IsAlive = false;
            runtime.Tick(Dt);
            Assert.AreEqual(0f, zone.Charge, "Death resets the progress.");
            Assert.AreEqual(0f, _player.Healed);
        }

        [Test]
        public void Shrine_CooldownKeepsRunningWhileFarAway()
        {
            var runtime = Build(1, (ZoneTestData.Shrine(), Vector2.zero, 0f));
            _player.Position = Vector2.zero;
            Run(runtime, 10.2f);
            Assert.Greater(runtime.Zones[0].ShrineCooldownRemaining, 59f);
            _view = new Rect(500f, 500f, 10f, 10f); // the shrine is outside the active window
            Run(runtime, 30f);
            Assert.AreEqual(30f, runtime.Zones[0].ShrineCooldownRemaining, 1.5f, "A shrine's cooldown does not pause out of view.");
        }

        [Test]
        public void CyclingAltar_RestProgress_RunsFromZeroToOneBetweenWindows()
        {
            var runtime = Build(1, (ZoneTestData.Altar(), new Vector2(30, 30), 0f));
            var zone = runtime.Zones[0];
            Run(runtime, 10f);
            Assert.AreEqual(-1f, zone.RestProgress(runtime.Time), "No ring while it is on.");
            Run(runtime, 24f); // 34 s: 10 s into the 66 s rest
            Assert.AreEqual(10f / 66f, zone.RestProgress(runtime.Time), 0.02f);
        }

        [Test]
        public void Charge_GrowsWhileInside_ClampsAtFull_AndBonusFollowsTheCharge()
        {
            var runtime = Build(1, (ZoneTestData.Charge(), Vector2.zero, 0f));
            _player.Position = new Vector2(1, 0);
            Run(runtime, 10f);
            Assert.AreEqual(0.5f, runtime.Zones[0].Charge, 0.02f, "Half full after half the fill time.");
            Assert.AreEqual(0.5f, _player.Zone.ActiveSkillDamageMultiplierBonus, 0.02f, "+100% at full means +50% at half.");
            Assert.AreEqual(0.15f, _player.Zone.ActionSpeedBonus, 0.01f);
            Run(runtime, 15f);
            Assert.AreEqual(1f, runtime.Zones[0].Charge, 1e-4f, "Capped at full.");
            Assert.AreEqual(1f, _player.Zone.ActiveSkillDamageMultiplierBonus, 1e-3f);
            Assert.AreEqual(1, runtime.PlayerActiveZoneCount);
        }

        [Test]
        public void Charge_DrainsAfterLeaving_KeepingATrailingBonusUntilEmpty()
        {
            var runtime = Build(1, (ZoneTestData.Charge(), Vector2.zero, 0f));
            _player.Position = Vector2.zero;
            Run(runtime, 20f);
            _player.Position = new Vector2(25, 0); // outside: it drains over 8 s
            Run(runtime, 4f);
            Assert.AreEqual(0.5f, runtime.Zones[0].Charge, 0.02f);
            Assert.AreEqual(0.5f, _player.Zone.ActiveSkillDamageMultiplierBonus, 0.02f, "The bonus lingers while it drains.");
            Run(runtime, 5f);
            Assert.AreEqual(0f, runtime.Zones[0].Charge);
            Assert.IsFalse(_player.Modifiers.ContainsKey(ZoneRuntime.ModifierKey), "Empty: nothing left.");
        }

        [Test]
        public void Charge_AltarsStack_AndAFarAltarForgetsItsCharge()
        {
            var runtime = Build(1, (ZoneTestData.Charge("T-CHARGE-A"), new Vector2(0, 0), 0f), (ZoneTestData.Charge("T-CHARGE-B"), new Vector2(9, 0), 0f));
            _player.Position = new Vector2(4.5f, 0); // inside both 5-unit discs
            Run(runtime, 20f);
            Assert.AreEqual(2f, _player.Zone.ActiveSkillDamageMultiplierBonus, 1e-3f, "Two full altars add up.");
            Assert.AreEqual(2, runtime.PlayerActiveZoneCount);
            _view = new Rect(60f, 60f, 17.8f, 10f); // the camera moved far away: both altars fall asleep
            Run(runtime, 0.2f);
            Assert.AreEqual(0f, runtime.Zones[0].Charge, "A sleeping altar drops its charge.");
        }

        [Test]
        public void Charge_OnlyFillsWhileTheAltarIsOn_AndDeathResetsIt()
        {
            var cyclingCharge = ZoneTestData.Charge(mode: ZoneLifetimeMode.Cycling);
            cyclingCharge.PulsePeriodSeconds = 90f; cyclingCharge.PulseVisibleSeconds = 24f; cyclingCharge.PulseFadeSeconds = 3f;
            var runtime = Build(1, (cyclingCharge, Vector2.zero, 0f));
            _player.Position = Vector2.zero;
            Run(runtime, 40f); // on for the first 24 s only
            Assert.Less(runtime.Zones[0].Charge, 0.2f, "Standing in a resting altar earns nothing (it drained after switching off).");
            var always = Build(1, (ZoneTestData.Charge(), Vector2.zero, 0f));
            Run(always, 10f);
            Assert.Greater(always.Zones[0].Charge, 0.4f);
            _player.IsAlive = false;
            always.Tick(Dt);
            Assert.AreEqual(0f, always.Zones[0].Charge, "Death empties every charge.");
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
