using System.Collections.Generic;
using Game.Enemy;

namespace Game.ActiveSkill
{
    public interface IActiveSkillTargetSetProvider : IActiveSkillTargetProvider
    {
        void CopyAliveTo(List<IEnemyDamageReceiver> destination);
    }
}
