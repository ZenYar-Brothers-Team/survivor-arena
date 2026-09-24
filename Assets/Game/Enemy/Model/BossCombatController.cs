using System;
using Game.Content;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// IP-15 phase policy: highest crossed phase wins; healing never reverses a phase. Default (fixture) phase
    /// changes cancel the pending attack and restart the new sequence. With KeepAttackOrderOnPhaseChange the
    /// running wind-up/interval finishes unchanged and the order continues with the new phase's profiles
    /// (BOSS-001, DECISION-0053/0054). Already emitted projectiles retain their source/profile.
    /// </summary>
    public sealed class BossCombatController
    {
        private readonly BossEncounterDefinition _definition;
        private int _attackIndex;
        private EnemyAttackController[] _sequence;
        private EnemyAttackController _current;
        private EnemyDefinition _currentDefinition;
        public int PhaseIndex { get; private set; }
        public BossPhaseDefinition Phase => _definition.Phases[PhaseIndex];
        /// <summary>Carrier of the attack currently scheduled; null for a boss without ranged attacks.</summary>
        public EnemyDefinition AttackDefinition => _currentDefinition;
        public EnemyAttackController Attack => _current;
        public event Action<int, int> PhaseChanged;

        public BossCombatController(BossEncounterDefinition definition)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            InitializeSequence();
            Select(0);
        }

        private void InitializeSequence()
        {
            _sequence = new EnemyAttackController[Phase.Attacks.Count];
            for (var i = 0; i < _sequence.Length; i++)
                _sequence[i] = new EnemyAttackController(Phase.Attacks[i].Attack, repeat: false);
        }

        private void Select(int index)
        {
            if (_sequence.Length == 0) { _current = null; _currentDefinition = null; return; }
            _attackIndex = index % _sequence.Length;
            _current = _sequence[_attackIndex];
            _currentDefinition = Phase.Attacks[_attackIndex];
        }

        private bool Crossed(float healthFraction, float threshold) =>
            _definition.StrictHealthThreshold ? healthFraction < threshold : healthFraction <= threshold;

        public EnemyShotCommand[] Tick(float deltaTime, bool isRunning, float healthFraction, Vector2 aim)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            NumericValidation.ValidateRange(healthFraction, 0, 1, nameof(healthFraction));
            if (!isRunning || healthFraction <= 0) return Array.Empty<EnemyShotCommand>();
            var previous = PhaseIndex;
            while (PhaseIndex + 1 < _definition.Phases.Count &&
                   Crossed(healthFraction, _definition.Phases[PhaseIndex + 1].HealthThreshold))
                PhaseIndex++;
            if (previous != PhaseIndex)
            {
                InitializeSequence();
                if (!_definition.KeepAttackOrderOnPhaseChange) Select(0);
                PhaseChanged?.Invoke(previous, PhaseIndex);
            }
            else if (_current != null && _current.CycleCompletesWithin(deltaTime))
            {
                Select(_attackIndex + 1);
                _current.RestartCycle();
            }
            return _current == null ? Array.Empty<EnemyShotCommand>() : _current.Tick(deltaTime, true, aim);
        }
    }
}
