using Game.Enemy;
using System.Collections.Generic;
using UnityEngine;

namespace Game.ActiveSkill
{
    /// <summary>Player skill targets. With a viewport only on-screen enemies are selectable and point
    /// attacks can fall back to a random on-screen point (DECISION-0058).</summary>
    public sealed class SceneEnemyTargetProvider : IActiveSkillTargetSetProvider, IActiveSkillAimArea
    {
        private readonly ICombatTargetQuery _targets;
        private readonly ITargetViewport _viewport;

        public SceneEnemyTargetProvider(ICombatTargetQuery targets = null, ITargetViewport viewport = null)
        {
            var all = targets ?? new SceneCombatTargetQuery();
            _targets = viewport == null ? all : new ViewportCombatTargetQuery(all, viewport);
            _viewport = viewport;
        }

        public void CopyAliveTo(List<IEnemyDamageReceiver> destination) => _targets.CopyAliveTo(destination);

        public bool TryGetTarget(Vector2 origin, out IEnemyDamageReceiver target)
        {
            return _targets.TryFindNearest(origin, out target);
        }

        public bool IsScreenLimited => _viewport != null;

        public bool TryPickPoint(Vector2 origin, float radius, System.Random random, out Vector2 point)
        {
            point = default;
            return _viewport != null && _viewport.Current.TryPickPoint(origin, radius, random, out point);
        }
    }
}
