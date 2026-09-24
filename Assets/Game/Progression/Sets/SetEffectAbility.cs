using System;
using Game.Combat;
using Game.Content;
namespace Game.Progression
{
    /// <summary>GDD Sets: keyed ownership, ordinary-activation counters, no set-to-set recursion.</summary>
    public sealed class SetEffectAbility : ISetExtraAbility
    {
        private readonly ISetEffectHost _host;
        private readonly SetDefinition _definition;
        private readonly float[] _cooldowns;
        private readonly float[] _buffs;
        private readonly int[] _counts;
        private bool _disposed;
        public int ProcCount { get; private set; }
        public int OrdinaryActivationCount { get; private set; }
        public int RejectedSourceCount { get; private set; }
        private string Key(int index) => "set:" + _definition.Id + ":" + index;
        public SetEffectAbility(SetDefinition definition, ISetEffectHost host)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _cooldowns = new float[definition.Effects.Count]; _buffs = new float[_cooldowns.Length]; _counts = new int[_cooldowns.Length];
            try
            {
                for (var i = 0; i < definition.Effects.Count; i++)
                {
                    var effect = definition.Effects[i];
                    if (effect.Kind == SetEffectKind.StatBuff) _host.Stats.SetModifier(Key(i), effect.Modifier);
                    if (effect.Kind == SetEffectKind.SkillTransform) _host.SetSkillModifier(Key(i), effect.Skill.Value, effect.Modifier);
                    if (effect.Kind == SetEffectKind.IndependentAttack) _cooldowns[i] = effect.CooldownSeconds;
                    if (effect.Kind == SetEffectKind.SlowedTargetBonus)
                        _host.SetSlowedTargetBonus(Key(i), effect.Skill, new SlowedTargetBonus(
                            effect.Modifier.ActiveSkillDamageMultiplierBonus, effect.Modifier.OutgoingKnockbackBonus));
                    if (effect.Kind == SetEffectKind.OrbitSlowAura)
                        _host.SetOrbitSlowAura(Key(i), _definition.Id, effect.Skill.Value, effect.SlowFraction, effect.SlowSeconds, effect.RefreshSeconds);
                }
                _host.ActiveSkillActivated += OnActivation; _host.Rewarded += OnReward; _host.LevelEarned += OnLevel;
            }
            catch { Dispose(); throw; }
        }
        private void OnActivation(CombatSource source)
        {
            if (_disposed || !_host.IsRunning) return;
            if (source.Origin != CombatSourceOrigin.ActiveSkill) { RejectedSourceCount++; return; }
            OrdinaryActivationCount++;
            for (var i = 0; i < _counts.Length; i++)
            {
                var effect = _definition.Effects[i];
                if (effect.Kind != SetEffectKind.ActivationProc || (effect.Skill.HasValue && effect.Skill != source.ContentId)) continue;
                if (++_counts[i] >= effect.ActivationCount) { _counts[i] = 0; Proc(i); }
            }
        }
        private void OnReward(SetRewardEvent reward)
        {
            if (_disposed || !_host.IsRunning) return;
            if (reward.Source.Origin == CombatSourceOrigin.Set || reward.Source.Origin == CombatSourceOrigin.SecondaryProc)
            { RejectedSourceCount++; return; }
            for (var i = 0; i < _counts.Length; i++) if (_definition.Effects[i].Kind == SetEffectKind.RewardProc) Proc(i);
        }
        private void OnLevel(int level)
        {
            // XP commits before the draft pause; heal remains valid while the resulting draft is open.
            if (_disposed || _host.IsTerminal) return;
            foreach (var effect in _definition.Effects)
                if (effect.Kind == SetEffectKind.LevelHeal) { _host.Heal(_definition.Id, effect.HealFraction); ProcCount++; }
        }
        private void Proc(int i)
        {
            if (_cooldowns[i] > 0) return;
            var effect = _definition.Effects[i];
            _cooldowns[i] = effect.CooldownSeconds; // Commit before callbacks: reentrant sources cannot proc recursively.
            if (effect.BuffSeconds > 0) { _host.Stats.SetModifier(Key(i), effect.Modifier); _buffs[i] = effect.BuffSeconds; }
            _host.Attack(Key(i), _definition.Id, effect.AttackTemplate.Value, effect.ScalesWithSizeAndRange);
            ProcCount++;
        }
        public void Tick(float deltaTime, bool isRunning)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            if (_disposed) return;
            if (_host.IsTerminal) { Dispose(); return; }
            for (var i = 0; i < _counts.Length; i++) _host.TickAttack(Key(i), deltaTime, isRunning && _host.IsRunning);
            if (!isRunning || !_host.IsRunning) return;
            for (var i = 0; i < _counts.Length; i++)
            {
                _cooldowns[i] = Math.Max(0, _cooldowns[i] - deltaTime);
                if (_buffs[i] > 0)
                {
                    _buffs[i] = Math.Max(0, _buffs[i] - deltaTime);
                    if (_buffs[i] == 0) _host.Stats.RemoveModifier(Key(i));
                }
                if (_definition.Effects[i].Kind == SetEffectKind.IndependentAttack) Proc(i);
            }
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _host.ActiveSkillActivated -= OnActivation; _host.Rewarded -= OnReward; _host.LevelEarned -= OnLevel;
            for (var i = 0; i < _counts.Length; i++)
            {
                _host.Stats.RemoveModifier(Key(i)); _host.RemoveSkillModifier(Key(i)); _host.ClearAttack(Key(i));
                _host.RemoveSlowedTargetBonus(Key(i)); _host.RemoveOrbitSlowAura(Key(i));
                _counts[i] = 0; _cooldowns[i] = 0; _buffs[i] = 0;
            }
        }
    }
}
