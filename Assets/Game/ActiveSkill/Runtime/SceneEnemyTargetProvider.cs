using System.Collections.Generic;
using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill
{
    public sealed class SceneEnemyTargetProvider : IActiveSkillTargetProvider
    {
        private readonly List<IEnemyDamageReceiver> _candidates = new List<IEnemyDamageReceiver>();

        public bool TryGetTarget(Vector2 origin, out IEnemyDamageReceiver target)
        {
            _candidates.Clear();
            var enemies = Object.FindObjectsByType<EnemyRuntime>();
            for (var i = 0; i < enemies.Length; i++)
                _candidates.Add(enemies[i]);

            return NearestEnemyTargetSelector.TrySelect(origin, _candidates, out target);
        }
    }
}
