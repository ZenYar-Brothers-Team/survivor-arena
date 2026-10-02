using Game.Zones.Json;
using NUnit.Framework;
using UnityEngine;

namespace Game.Zones.Tests
{
    public sealed class SharedZoneTargetsTests
    {
        private static ZoneRuntime Build(ZoneEffectData data, FakeZoneEnemies enemies, FakeZonePlayer player)
        {
            data.AffectsBothSides = true;
            var layout = new ZoneLayoutDefinition(ZoneTestData.Layout(new[] { data }, new[] { (data.Id, 1) }));
            var effect = layout.Effects[new Game.Content.ContentId(data.Id)];
            return new ZoneRuntime(new[] { new ZonePlacement(0, effect, Vector2.zero) },
                new ZonePlacementRules(layout, 120f, Vector2.zero, null), 6, player, enemies,
                () => new Rect(-100f, -100f, 200f, 200f));
        }
        [Test]
        public void SharedAreas_ApplyPlayerValuesToEnemies_ExitClearsBonuses()
        {
            var player = new FakeZonePlayer(); var enemies = new FakeZoneEnemies().Add(Vector2.zero);
            foreach (var data in new[] { ZoneTestData.Haste(), ZoneTestData.Regeneration(), ZoneTestData.Arcane(), ZoneTestData.Protection() })
            {
                using var runtime = Build(data, enemies, player); runtime.Tick(.1f);
                Assert.AreEqual(data.PlayerMovementBonus ?? 0f, enemies.MovementBonus);
                Assert.AreEqual(data.PlayerActionSpeedBonus ?? 0f, enemies.ActionBonus);
                Assert.AreEqual(data.PlayerRegenerationPerSecond ?? 0f, enemies.Regeneration);
                Assert.AreEqual(data.PlayerIncomingDamageReduction ?? 0f, enemies.Defense);
                enemies.Positions[0] = Vector2.right * 30f; runtime.Tick(.1f);
                Assert.AreEqual(0f, enemies.MovementBonus + enemies.ActionBonus + enemies.Regeneration + enemies.Defense);
                enemies.Positions[0] = Vector2.zero;
            }
        }
        [Test]
        public void SharedBurst_HitsEnemiesOnceAtFiring_LeavesSameTimedSpeedBuff()
        {
            var player = new FakeZonePlayer(); var enemies = new FakeZoneEnemies().Add(Vector2.zero);
            var data = ZoneTestData.SpeedBurst();
            using var runtime = Build(data, enemies, player);
            runtime.Tick(3.9f); Assert.AreEqual(0f, enemies.BuffSeconds);
            runtime.Tick(.2f); Assert.AreEqual(.6f, enemies.BuffBonus); Assert.AreEqual(8f, enemies.BuffSeconds);
            enemies.Positions[0] = Vector2.right * 30f; runtime.Tick(.1f);
            Assert.Greater(runtime.BuffRemaining(runtime.Zones[0].Effect), 0f);
        }
        [Test]
        public void SharedPortal_TransportsEnemyWithoutPlayer_RecordsFlashOnBothEnds()
        {
            var data = ZoneTestData.Portal(); data.AffectsBothSides = true;
            var layout = new ZoneLayoutDefinition(ZoneTestData.Layout(new[] { data }, (data.Id, 2)));
            var placements = ZoneLayoutGenerator.Generate(layout, 120f, Vector2.zero, null, 6);
            var enemies = new FakeZoneEnemies().Add(placements[0].Center);
            var player = new FakeZonePlayer { Position = Vector2.one * 1000f };
            using var runtime = new ZoneRuntime(placements, new ZonePlacementRules(layout, 120f, Vector2.zero, null), 6,
                player, enemies, () => new Rect(-100f, -100f, 200f, 200f));
            runtime.Tick(.1f);
            Assert.AreEqual(1, enemies.Teleports); Assert.AreEqual(0, player.Teleports.Count);
            Assert.AreEqual(.1f, placements[0].LastApplicationSeconds);
            Assert.AreEqual(.1f, placements[1].LastApplicationSeconds);
            runtime.Tick(.1f); Assert.AreEqual(1, enemies.Teleports, "Exit is outside the paired entry area.");
        }
    }
}
