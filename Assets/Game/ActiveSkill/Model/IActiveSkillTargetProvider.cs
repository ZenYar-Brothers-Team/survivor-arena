using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill
{
    public interface IActiveSkillTargetProvider
    {
        bool TryGetTarget(Vector2 origin, out IEnemyDamageReceiver target);
    }
}
