using System.Collections.Generic;
using Game.Enemy;

namespace Game.ActiveSkill
{
    /// <summary>Distinct target lives already used by one activation's RandomEnemy strike waves.</summary>
    internal sealed class StrikeTargetSet
    {
        public HashSet<EnemyTargetLife> Used { get; } = new HashSet<EnemyTargetLife>();
    }
}
