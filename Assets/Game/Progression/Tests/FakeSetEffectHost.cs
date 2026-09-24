using System;
using System.Collections.Generic;
using Game.Character;
using Game.Combat;
using Game.Content;
namespace Game.Progression.Tests
{
    public sealed class FakeSetEffectHost : ISetEffectHost
    {
        public CharacterStats Stats { get; } = new CharacterStats(new CharacterBaseStats(100, 3));
        public bool IsRunning { get; set; } = true;
        public bool IsTerminal { get; set; }
        public event Action<CombatSource> ActiveSkillActivated;
        public event Action<SetRewardEvent> Rewarded;
        public event Action<int> LevelEarned;
        public Dictionary<string, CharacterStatModifier> SkillModifiers { get; } = new Dictionary<string, CharacterStatModifier>();
        public int Attacks { get; private set; }
        public int Clears { get; private set; }
        public float Healed { get; private set; }
        public bool ThrowOnSkillModifier { get; set; }
        public void SetSkillModifier(string key, ContentId skill, CharacterStatModifier modifier)
        {
            if (ThrowOnSkillModifier) throw new InvalidOperationException("Injected binding failure");
            SkillModifiers[key] = modifier;
        }
        public void RemoveSkillModifier(string key) => SkillModifiers.Remove(key);
        public Dictionary<string, (ContentId? skill, SlowedTargetBonus bonus)> SlowedBonuses { get; } = new Dictionary<string, (ContentId?, SlowedTargetBonus)>();
        public Dictionary<string, (ContentId set, ContentId skill, float fraction, float seconds, float refresh)> Auras { get; } =
            new Dictionary<string, (ContentId, ContentId, float, float, float)>();
        public bool LastAttackScalesWithSizeAndRange { get; private set; }
        public void SetSlowedTargetBonus(string key, ContentId? skill, SlowedTargetBonus bonus) => SlowedBonuses[key] = (skill, bonus);
        public void RemoveSlowedTargetBonus(string key) => SlowedBonuses.Remove(key);
        public void SetOrbitSlowAura(string key, ContentId set, ContentId orbitSkill, float slowFraction, float slowSeconds, float refreshSeconds) =>
            Auras[key] = (set, orbitSkill, slowFraction, slowSeconds, refreshSeconds);
        public void RemoveOrbitSlowAura(string key) => Auras.Remove(key);
        public void Attack(string key, ContentId set, ContentId template, bool scalesWithSizeAndRange = false)
        {
            LastAttackScalesWithSizeAndRange = scalesWithSizeAndRange;
            Attacks++;
            ActiveSkillActivated?.Invoke(new CombatSource(default, set, CombatSourceOrigin.Set));
        }
        public void ClearAttack(string key) { Clears++; }
        public void TickAttack(string key, float deltaTime, bool isRunning) { }
        public void Heal(ContentId set, float fraction) { Healed += Stats.MaxHealth * fraction; }
        public void Activate(CombatSourceOrigin origin = CombatSourceOrigin.ActiveSkill) =>
            ActiveSkillActivated?.Invoke(new CombatSource(default, new ContentId("FIXTURE-SKILL"), origin));
        public void Reward(CombatSourceOrigin origin = CombatSourceOrigin.Unknown) =>
            Rewarded?.Invoke(new SetRewardEvent(new CombatSource(default, null, origin)));
        public void Level() => LevelEarned?.Invoke(2);
    }
}
