using System;
using System.Collections.Generic;
using Game.Character;
using Game.Combat;
using Game.Content;
using Game.Movement;
using Game.Progression;
using Game.Run;
using UnityEngine;
namespace Game.ActiveSkill
{
    /// <summary>Composition adapter. Set effects reuse skill executors while retaining set source and fixed timing.</summary>
    public sealed class SetEffectHost : ISetEffectHost, IDisposable
    {
        private readonly PlayerCharacterRuntime _player;
        private readonly RunController _run;
        private readonly PlayerActiveSkillSetRuntime _skills;
        private readonly ExperienceProgression _experience;
        private readonly Dictionary<ContentId, ActiveSkillProgressionDefinition> _catalog = new Dictionary<ContentId, ActiveSkillProgressionDefinition>();
        private readonly Dictionary<string, (ContentId skill, CharacterStatModifier modifier)> _modifiers = new Dictionary<string, (ContentId, CharacterStatModifier)>();
        private readonly Dictionary<string, (ActiveSkillInstance instance, SceneActiveSkillEffectExecutor executor)> _attacks = new Dictionary<string, (ActiveSkillInstance, SceneActiveSkillEffectExecutor)>();
        private readonly IActiveSkillTargetProvider _targets = new SceneEnemyTargetProvider();
        private bool _disposed;
        public CharacterStats Stats => _player.Stats;
        public bool IsRunning => !_disposed && _run.Model.State == RunState.Running;
        public bool IsTerminal => _disposed || _run.Model.State == RunState.Won || _run.Model.State == RunState.Lost || _run.Model.State == RunState.Stopped;
        public event Action<CombatSource> ActiveSkillActivated;
        public event Action<SetRewardEvent> Rewarded;
        public event Action<int> LevelEarned;
        public SetEffectHost(PlayerCharacterRuntime player, RunController run, PlayerActiveSkillSetRuntime skills,
            ExperienceProgression experience, IEnumerable<ActiveSkillProgressionDefinition> catalog)
        {
            _player = player; _run = run; _skills = skills; _experience = experience;
            foreach (var definition in catalog) _catalog.Add(definition.Id, definition);
            _skills.SetSkillModifier = GetSkillModifier;
            _skills.Activated += OnActivation; _experience.LevelUp += OnLevel;
        }
        private void OnActivation(CombatSource source) => ActiveSkillActivated?.Invoke(source);
        private void OnLevel(int level) => LevelEarned?.Invoke(level);
        public void PublishReward(SetRewardEvent reward) { if (IsRunning) Rewarded?.Invoke(reward); }
        public void SetSkillModifier(string key, ContentId skill, CharacterStatModifier modifier)
        {
            if (!_catalog.ContainsKey(skill)) throw new InvalidOperationException("Missing transform skill " + skill);
            _modifiers[key] = (skill, modifier);
        }
        public void RemoveSkillModifier(string key) => _modifiers.Remove(key);
        public CharacterStatModifier GetSkillModifier(ContentId skill)
        {
            float damage = 0, speed = 0, size = 0, range = 0, knockback = 0;
            foreach (var entry in _modifiers.Values)
                if (entry.skill == skill)
                {
                    damage += entry.modifier.ActiveSkillDamageMultiplierBonus;
                    speed += entry.modifier.ActionSpeedBonus; size += entry.modifier.EffectSizeMultiplierBonus;
                    range += entry.modifier.EffectRangeMultiplierBonus; knockback += entry.modifier.OutgoingKnockbackBonus;
                }
            return new CharacterStatModifier(activeSkillDamageMultiplierBonus: damage, actionSpeedBonus: speed,
                effectSizeMultiplierBonus: size, effectRangeMultiplierBonus: range, outgoingKnockbackBonus: knockback);
        }
        public void Attack(string key, ContentId set, ContentId template)
        {
            if (!IsRunning) return;
            if (!_attacks.TryGetValue(key, out var attack))
            {
                if (!_catalog.TryGetValue(template, out var definition)) throw new InvalidOperationException("Missing set attack template " + template);
                attack = (new ActiveSkillInstance(definition), new SceneActiveSkillEffectExecutor(_run));
                _attacks.Add(key, attack);
            }
            var mover = _player.GetComponent<PlayerMover>();
            var source = new CombatSource(_player.Identity, set, CombatSourceOrigin.Set);
            if (attack.instance.Tick(0, true, _player, _targets, attack.executor,
                mover != null ? mover.MovementDirection : Vector2.zero, sourceOverride: source, forceActivation: true))
            {
                // Expose the typed source to observers; set abilities explicitly reject it.
                ActiveSkillActivated?.Invoke(source);
                attack.executor.Tick(0, true);
            }
        }
        public void TickAttack(string key, float deltaTime, bool isRunning)
        {
            if (!_attacks.TryGetValue(key, out var attack)) return;
            attack.instance.HitLedger.Tick(deltaTime, isRunning);
            attack.executor.Tick(deltaTime, isRunning);
        }
        public void ClearAttack(string key)
        {
            if (!_attacks.TryGetValue(key, out var attack)) return;
            attack.executor.Dispose(); attack.instance.HitLedger.Clear(); _attacks.Remove(key);
        }
        public void Heal(ContentId set, float maxHealthFraction)
        {
            if (!IsTerminal) _player.Heal(Stats.MaxHealth * maxHealthFraction, new CombatSource(_player.Identity, set, CombatSourceOrigin.Set));
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _skills.Activated -= OnActivation; _skills.SetSkillModifier = null; _experience.LevelUp -= OnLevel;
            foreach (var attack in _attacks.Values) { attack.executor.Dispose(); attack.instance.HitLedger.Clear(); }
            _attacks.Clear(); _modifiers.Clear();
        }
    }
}
