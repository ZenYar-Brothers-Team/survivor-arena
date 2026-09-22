using System;
using Game.Character;
using Game.Combat;
using Game.Content;
namespace Game.Progression
{
    public interface ISetEffectHost
    {
        CharacterStats Stats { get; }
        bool IsRunning { get; }
        bool IsTerminal { get; }
        event Action<CombatSource> ActiveSkillActivated;
        event Action<SetRewardEvent> Rewarded;
        event Action<int> LevelEarned;
        void SetSkillModifier(string key, ContentId skill, CharacterStatModifier modifier);
        void RemoveSkillModifier(string key);
        void Attack(string key, ContentId set, ContentId template);
        void ClearAttack(string key);
        void TickAttack(string key, float deltaTime, bool isRunning);
        void Heal(ContentId set, float maxHealthFraction);
    }
}
