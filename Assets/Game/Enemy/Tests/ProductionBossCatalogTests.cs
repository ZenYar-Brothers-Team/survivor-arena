using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    /// <summary>F1-06: BOSS-001 and MIDBOSS-001 production encounters (baseline v1).</summary>
    public sealed class ProductionBossCatalogTests
    {
        private static BossEncounterDefinition Encounter(string id) => ProductionBossCatalog.Create().Single(b => b.Id.ToString() == id);

        [Test]
        public void Catalog_DefinesFinalAndMidBoss_WithApprovedBodies()
        {
            var boss = Encounter("BOSS-001");
            Assert.AreEqual(WaveHookKind.FinalBoss, boss.Hook);
            Assert.AreEqual("Староста-герой", boss.DisplayName);
            Assert.AreEqual(4500f, boss.Body.MaxHealth);
            Assert.AreEqual(150f, boss.Body.ExperienceReward);
            Assert.AreEqual(0.6f, boss.Body.KnockbackResistance, 1e-5f);
            Assert.AreEqual(8f, boss.SpawnOffsetX);
            Assert.IsTrue(boss.KeepAttackOrderOnPhaseChange);
            Assert.IsTrue(boss.StrictHealthThreshold);
            Assert.AreEqual(4, boss.OwnedAttacks.Count, "Inline fan/ring for both intervals; no ordinary enemy import.");
            var fan = boss.Phases[0].Attacks[0].Attack;
            Assert.AreEqual(EnemyProjectilePattern.Fan, fan.Pattern);
            Assert.AreEqual(5, fan.ProjectileCount);
            Assert.AreEqual(70f, fan.SpreadDegrees);
            Assert.AreEqual(18f, fan.Damage);
            Assert.AreEqual(3.6f, fan.CooldownSeconds, 1e-5f);
            var ring = boss.Phases[0].Attacks[1].Attack;
            Assert.AreEqual(10, ring.ProjectileCount);
            Assert.IsTrue(ring.FixedOrientation);
            Assert.AreEqual(2.88f, boss.Phases[1].Attacks[0].Attack.CooldownSeconds, 1e-5f);
            Assert.AreEqual(0.5f, boss.Phases[1].HealthThreshold, 1e-5f);
            var teleport = boss.Teleport;
            Assert.IsNotNull(teleport, "DECISION-0059: BOSS-001 teleport-slams a player who keeps away.");
            Assert.AreEqual(5f, teleport.FarDistance, 1e-5f, "Half of the 10-unit reference screen height.");
            Assert.AreEqual(5f, teleport.FarSeconds, 1e-5f);
            Assert.Less(teleport.LandingDistance, teleport.ImpactRadius, "A player standing still is caught by the slam.");
            Assert.AreEqual(20f, teleport.ImpactDamage, 1e-5f);

            var mid = Encounter("MIDBOSS-001");
            Assert.AreEqual(WaveHookKind.MidBoss, mid.Hook);
            Assert.AreEqual(1200f, mid.Body.MaxHealth);
            Assert.AreEqual(60f, mid.Body.ExperienceReward);
            Assert.AreEqual(2, mid.Body.Movement.DashCount);
            Assert.AreEqual(0.35f, mid.Body.Movement.FollowUpTelegraphSeconds, 1e-5f);
            Assert.AreEqual(0.8f, mid.Body.DashContactControls.KnockbackDistance, 1e-5f);
            Assert.AreEqual(0, mid.Phases.Single().Attacks.Count);
            Assert.IsNull(mid.Teleport, "Only the final boss teleports.");
        }

        private static List<(float time, int count, Vector2 first)> Run(BossCombatController combat, float seconds, float health,
            Vector2 aim, float start = 0f, List<(float, int, Vector2)> shots = null)
        {
            shots = shots ?? new List<(float, int, Vector2)>();
            const float dt = 0.02f;
            for (var t = start; t < start + seconds - 1e-4f; t += dt)
            {
                var fired = combat.Tick(dt, true, health, aim);
                if (fired.Length > 0) shots.Add((t + dt, fired.Length, fired[0].Direction));
            }
            return shots;
        }

        [Test]
        public void FinalBoss_AlternatesFanAndRing_StartToStartAfterFirstFullInterval()
        {
            var combat = new BossCombatController(Encounter("BOSS-001"));
            var shots = Run(combat, 12f, 1f, Vector2.up);
            Assert.AreEqual(3, shots.Count);
            Assert.AreEqual(4.3f, shots[0].time, 0.05f, "First wind-up 3.6 s after spawn + 0.7 s telegraph.");
            Assert.AreEqual(5, shots[0].count);
            Assert.AreEqual(1f, shots[0].first.y, 0.6f, "Fan aims at the player.");
            Assert.AreEqual(10, shots[1].count);
            Assert.AreEqual(1f, shots[1].first.x, 1e-4f, "Ring starts at 0° regardless of aim.");
            Assert.AreEqual(3.6f, shots[1].time - shots[0].time, 0.05f);
            Assert.AreEqual(5, shots[2].count);
        }

        [Test]
        public void FinalBoss_EnragesStrictlyBelowHalf_WithoutResettingOrderOrRunningInterval()
        {
            var combat = new BossCombatController(Encounter("BOSS-001"));
            var shots = Run(combat, 4.4f, 0.5f, Vector2.up);
            Assert.AreEqual(0, combat.PhaseIndex, "Exactly 50% is not below 50%.");
            Assert.AreEqual(1, shots.Count);
            var phaseChanges = 0;
            combat.PhaseChanged += (a, b) => phaseChanges++;
            shots = Run(combat, 3.6f, 0.49f, Vector2.up, 4.4f, shots); // enrage during the running 3.6 s interval (t=4.4..8.0)
            Assert.AreEqual(1, phaseChanges);
            Assert.AreEqual(2, shots.Count);
            Assert.AreEqual(10, shots[1].count, "Order continues with the ring; the queue is not reset to the fan.");
            Assert.AreEqual(3.6f, shots[1].time - shots[0].time, 0.05f, "The already running interval keeps 3.6 s.");
            shots = Run(combat, 3.0f, 0.49f, Vector2.up, 8.0f, shots); // t=8.0..11.0
            Assert.AreEqual(3, shots.Count);
            Assert.AreEqual(5, shots[2].count);
            Assert.AreEqual(2.88f, shots[2].time - shots[1].time, 0.05f, "Later intervals are 20% shorter.");
        }

        [Test]
        public void MidBoss_DoubleDash_SnapshotsEachDirection_ThenRecovers()
        {
            var body = Encounter("MIDBOSS-001").Body;
            var movement = new EnemyMovementController(body.Movement);
            var self = Vector2.zero;
            var target = new Vector2(5f, 0f);
            var phases = new List<EnemyMovementPhase>();
            EnemyMovementFrame frame = default;
            for (var t = 0f; t < 5.5f - 1e-4f; t += 0.05f) frame = movement.Tick(self, target, body.MovementSpeed, 0.05f, true);
            Assert.AreEqual(EnemyMovementPhase.TelegraphingDash, frame.Phase, "First pair after 5.5 s of pursuit.");
            Assert.AreEqual(Vector2.zero, frame.Velocity, "Stands during telegraph.");
            for (var t = 0f; t < 0.65f; t += 0.05f) frame = movement.Tick(self, target, body.MovementSpeed, 0.05f, true);
            Assert.AreEqual(EnemyMovementPhase.Dashing, frame.Phase);
            Assert.AreEqual(body.MovementSpeed * 5f, frame.Velocity.magnitude, 1e-3f);
            Assert.AreEqual(1f, frame.Velocity.normalized.x, 1e-3f);
            target = new Vector2(0f, 5f); // player moved: the second dash re-aims at its own telegraph start
            for (var t = 0f; t < 0.45f; t += 0.05f) frame = movement.Tick(self, target, body.MovementSpeed, 0.05f, true);
            Assert.AreEqual(EnemyMovementPhase.TelegraphingDash, frame.Phase);
            for (var t = 0f; t < 0.35f; t += 0.05f) frame = movement.Tick(self, target, body.MovementSpeed, 0.05f, true);
            Assert.AreEqual(EnemyMovementPhase.Dashing, frame.Phase);
            Assert.AreEqual(1f, frame.Velocity.normalized.y, 1e-3f);
            for (var t = 0f; t < 0.5f; t += 0.05f) frame = movement.Tick(self, target, body.MovementSpeed, 0.05f, true);
            Assert.AreEqual(EnemyMovementPhase.Seeking, frame.Phase, "No hidden third dash.");
        }
    }
}
