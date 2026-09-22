using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    public sealed class CombatTargetQueryTests
    {
        [Test]
        public void Query_FindsFutureCategoriesWithoutConcreteRuntime_AndFiltersThem()
        {
            var ordinary = new FakeCombatTarget { Category = EnemyCategory.Ordinary, Position = Vector2.right * 3f };
            var boss = new FakeCombatTarget { Category = EnemyCategory.Boss, Position = Vector2.right * 2f };
            var traveler = new FakeCombatTarget { Category = EnemyCategory.Traveler, Position = Vector2.right };
            var entries = new[] { ordinary, boss, traveler };
            try
            {
                foreach (var entry in entries) EnemyRegistry.Register(entry);
                ICombatTargetQuery query = new SceneCombatTargetQuery();
                Assert.IsTrue(query.TryFindNearest(Vector2.zero, out var nearest));
                Assert.AreSame(traveler, nearest);
                Assert.IsTrue(query.TryFindNearest(Vector2.zero, out nearest, EnemyTargetCategories.Boss));
                Assert.AreSame(boss, nearest);
                var results = new List<IEnemyDamageReceiver>();
                query.CopyAliveTo(results, EnemyTargetCategories.Ordinary | EnemyTargetCategories.Traveler);
                CollectionAssert.AreEquivalent(new IEnemyDamageReceiver[] { ordinary, traveler }, results);
                traveler.IsAlive = false;
                query.CopyAliveTo(results, EnemyTargetCategories.Traveler);
                Assert.IsEmpty(results);
            }
            finally { foreach (var entry in entries) EnemyRegistry.Unregister(entry); }
        }

        [Test]
        public void CapturedLife_DoesNotTrackReusedObject_AndNewLifeCanBeHitAgain()
        {
            var target = new FakeCombatTarget();
            var first = new EnemyTargetLife(target);
            Assert.IsTrue(first.IsAlive);
            target.LifeId = Guid.NewGuid();
            Assert.IsFalse(first.IsAlive);
            Assert.AreNotEqual(first, new EnemyTargetLife(target));
        }
    }
}
