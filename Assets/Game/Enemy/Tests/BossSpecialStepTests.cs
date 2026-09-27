using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    /// <summary>DECISION-0066: a boss sequence step that starts a zone/beam/summon, phase dash overrides, authoring checks.</summary>
    public sealed class BossSpecialStepTests
    {
        private static readonly EnemyMovementProfile Dash = new EnemyMovementProfile(EnemyMovementKind.TelegraphedDash);

        private static EnemyDefinition Body(EnemyMovementProfile movement = null) =>
            new EnemyDefinition("FIXTURE-BOSS-SPECIAL", 100f, 2f, 1f, 1f, 1f, movement: movement);

        private static EnemyDefinition FanCarrier() => new EnemyDefinition("FIXTURE-BOSS-SPECIAL-FAN", 1f, 1f, 1f, 1f, 1f,
            attack: new EnemyAttackProfile(EnemyProjectilePattern.Fan, 10f, 2f, 5f, 3f, 3, 30f, telegraphSeconds: 0.5f,
                cadence: EnemyAttackCadence.WindupStartToStart));

        private static EnemyDefinition ZoneCarrier() => new EnemyDefinition("FIXTURE-BOSS-SPECIAL-ZONE", 1f, 1f, 1f, 1f, 1f);

        private static BossEncounterDefinition Encounter(BossSpecialAttack special) =>
            new BossEncounterDefinition("FIXTURE-BOSS-SPECIAL", "Special", WaveHookKind.FinalBoss, Body(), 8f, 0f, new[]
            {
                new BossPhaseDefinition("FIXTURE-BOSS-SPECIAL-P1", 1f, new[] { FanCarrier(), ZoneCarrier() }, new[] { null, special })
            });

        [Test]
        public void SpecialStep_StartsOnItsTurn_ThenHandsOverAfterItsCooldown()
        {
            var special = new BossSpecialAttack(3f, BossHazardTestData.Zone(BossZonePlacement.AtPlayer));
            var combat = new BossCombatController(Encounter(special));
            var events = new List<(float time, string what)>();
            const float dt = 0.02f;
            for (var t = dt; t <= 8f; t += dt)
            {
                var shots = combat.Tick(dt, true, 1f, Vector2.right);
                if (shots.Length > 0) events.Add((t, "fan"));
                if (combat.TriggeredSpecial != null)
                {
                    Assert.AreSame(special, combat.TriggeredSpecial);
                    Assert.AreEqual("FIXTURE-BOSS-SPECIAL-ZONE", combat.TriggeredSpecialId.ToString());
                    events.Add((t, "zone"));
                }
            }
            Assert.AreEqual(3, events.Count);
            Assert.AreEqual("fan", events[0].what);
            Assert.AreEqual(2.5f, events[0].time, 0.05f, "First wind-up after a full 2 s interval + 0.5 s telegraph.");
            Assert.AreEqual("zone", events[1].what);
            Assert.AreEqual(4f, events[1].time, 0.05f, "The zone step starts when the fan interval ends.");
            Assert.AreEqual("fan", events[2].what);
            Assert.AreEqual(7.5f, events[2].time, 0.05f, "Back to the fan 3 s later (+ its telegraph).");
        }

        [Test]
        public void SpecialStep_PausedRunStartsNothing()
        {
            var combat = new BossCombatController(Encounter(new BossSpecialAttack(1f, BossHazardTestData.Zone(BossZonePlacement.AtPlayer))));
            for (var i = 0; i < 1000; i++)
            {
                combat.Tick(0.02f, false, 1f, Vector2.right);
                Assert.IsNull(combat.TriggeredSpecial);
            }
        }

        [Test]
        public void Phase_RejectsMismatchedSteps_AndOverridesOnlyForDashingBosses()
        {
            var zone = new BossSpecialAttack(3f, BossHazardTestData.Zone(BossZonePlacement.AtPlayer));
            Assert.Throws<ArgumentException>(() => new BossPhaseDefinition("P", 1f, new[] { FanCarrier() }, new[] { zone }),
                "A special step cannot also carry projectiles.");
            Assert.Throws<ArgumentException>(() => new BossPhaseDefinition("P", 1f, new[] { ZoneCarrier() }),
                "A step without projectiles needs its special.");
            Assert.Throws<ArgumentException>(() => new BossPhaseDefinition("P", 1f, new[] { FanCarrier() }, new BossSpecialAttack[0]));
            var overridePhase = new BossPhaseDefinition("P2", .5f, new EnemyDefinition[0], movementOverride: Dash);
            var initial = new BossPhaseDefinition("P1", 1f, new EnemyDefinition[0]);
            Assert.Throws<ArgumentException>(() => new BossEncounterDefinition("FIXTURE-BOSS-SPECIAL", "S", WaveHookKind.FinalBoss,
                Body(), 0f, 0f, new[] { initial, overridePhase }), "A pursuing boss cannot switch to a dash series.");
            Assert.Throws<ArgumentException>(() => new BossEncounterDefinition("FIXTURE-BOSS-SPECIAL", "S", WaveHookKind.FinalBoss,
                Body(), 0f, 0f, new[] { initial }, holdAttacksDuringDash: true));
            var dashing = new BossEncounterDefinition("FIXTURE-BOSS-SPECIAL", "S", WaveHookKind.FinalBoss, Body(Dash), 0f, 0f,
                new[] { initial, overridePhase }, holdAttacksDuringDash: true);
            Assert.AreSame(Dash, dashing.Phases[1].MovementOverride);
            Assert.IsTrue(dashing.HoldAttacksDuringDash);
        }
    }
}
