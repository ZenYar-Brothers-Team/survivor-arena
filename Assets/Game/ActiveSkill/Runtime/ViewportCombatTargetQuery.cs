using System;
using System.Collections.Generic;
using Game.Diagnostics;
using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill
{
    /// <summary>Target query for player attack selection: only enemies whose position is on screen
    /// (DECISION-0058). Hit tests (beam, mines, areas) keep the unfiltered query.</summary>
    public sealed class ViewportCombatTargetQuery : ICombatTargetQuery
    {
        // Scans every live enemy for the nearest one on screen (DECISION-0008).
        private const float NearestWarningMilliseconds = 1f;

        private readonly ICombatTargetQuery _inner;
        private readonly ITargetViewport _viewport;
        private readonly List<IEnemyDamageReceiver> _buffer = new List<IEnemyDamageReceiver>();

        public ViewportCombatTargetQuery(ICombatTargetQuery inner, ITargetViewport viewport)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _viewport = viewport ?? throw new ArgumentNullException(nameof(viewport));
        }

        public void CopyAliveTo(List<IEnemyDamageReceiver> destination, EnemyTargetCategories categories = EnemyTargetCategories.All)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            _inner.CopyAliveTo(destination, categories);
            var screen = _viewport.Current;
            destination.RemoveAll(candidate => !screen.Contains(candidate.Position));
        }

        public bool TryFindNearest(Vector2 origin, out IEnemyDamageReceiver nearest, EnemyTargetCategories categories = EnemyTargetCategories.All)
        {
            using var guard = PerfGuard.Measure("ViewportCombatTargetQuery.TryFindNearest", NearestWarningMilliseconds);
            CopyAliveTo(_buffer, categories);
            nearest = null;
            var best = float.PositiveInfinity;
            for (var i = 0; i < _buffer.Count; i++)
            {
                var candidate = _buffer[i];
                if (!candidate.IsAlive) continue;
                var distance = (candidate.Position - origin).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                nearest = candidate;
            }
            _buffer.Clear();
            return nearest != null;
        }
    }
}
