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
        void Attack(string key, ContentId set, ContentId template, bool scalesWithSizeAndRange = false);
        /// <summary>Registers a bonus against already slowed targets; <paramref name="skill"/> null = all player skills.</summary>
        void SetSlowedTargetBonus(string key, ContentId? skill, SlowedTargetBonus bonus);
        void RemoveSlowedTargetBonus(string key);
        /// <summary>Refreshing slow inside the live orbit radius of <paramref name="orbitSkill"/>; ticked through TickAttack(key).</summary>
        void SetOrbitSlowAura(string key, ContentId set, ContentId orbitSkill, float slowFraction, float slowSeconds, float refreshSeconds);
        void RemoveOrbitSlowAura(string key);
        void ClearAttack(string key);
        void TickAttack(string key, float deltaTime, bool isRunning);
        void Heal(ContentId set, float maxHealthFraction);
    }
}
