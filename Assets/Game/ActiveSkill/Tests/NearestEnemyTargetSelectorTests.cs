using System.Collections.Generic;
using Game.Enemy;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    public class NearestEnemyTargetSelectorTests
    {
        [Test]
        public void Selector_ChoosesNearestAliveTarget()
        {
            var deadNearest = new FakeReceiver(new Vector2(0.25f, 0f), isAlive: false);
            var expected = new FakeReceiver(new Vector2(1f, 0f));
            var farther = new FakeReceiver(new Vector2(3f, 0f));
            var candidates = new List<IEnemyDamageReceiver> { farther, deadNearest, expected };

            var found = NearestEnemyTargetSelector.TrySelect(Vector2.zero, candidates, out var target);

            Assert.IsTrue(found);
            Assert.AreSame(expected, target);
        }

        [Test]
        public void Selector_ReturnsFalseWithoutAliveTargets()
        {
            var candidates = new List<IEnemyDamageReceiver>
            {
                new FakeReceiver(Vector2.zero, isAlive: false)
            };

            Assert.IsFalse(NearestEnemyTargetSelector.TrySelect(Vector2.zero, candidates, out var target));
            Assert.IsNull(target);
        }

        private sealed class FakeReceiver : IEnemyDamageReceiver
        {
            public bool IsAlive { get; }
            public Vector2 Position { get; }

            public FakeReceiver(Vector2 position, bool isAlive = true)
            {
                Position = position;
                IsAlive = isAlive;
            }

            public float ApplyDamage(EnemyDamageRequest request) => request.Amount;
        }
    }
}
