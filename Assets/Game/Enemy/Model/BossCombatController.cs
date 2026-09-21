using System;
using Game.Content;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>IP-15 synthetic phase policy: highest crossed phase wins; cancel pending attack, re-telegraph.
    /// Already emitted projectiles retain their source/profile. Healing never reverses a phase.</summary>
    public sealed class BossCombatController
    {
        private readonly BossEncounterDefinition _definition;
        private int _attackIndex;
        private EnemyAttackController[] _sequence;
        public int PhaseIndex { get; private set; }
        public BossPhaseDefinition Phase => _definition.Phases[PhaseIndex];
        public EnemyDefinition AttackDefinition => Phase.Attacks[_attackIndex];
        public EnemyAttackController Attack => _sequence[_attackIndex];
        public event Action<int, int> PhaseChanged;

        public BossCombatController(BossEncounterDefinition definition)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            InitializeSequence();
        }

        private void InitializeSequence()
        {
            _sequence = new EnemyAttackController[Phase.Attacks.Count];
            for (var i = 0; i < _sequence.Length; i++)
                _sequence[i] = new EnemyAttackController(Phase.Attacks[i].Attack, repeat: false);
        }

        public EnemyShotCommand[] Tick(float deltaTime, bool isRunning, float healthFraction, Vector2 aim)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            NumericValidation.ValidateRange(healthFraction, 0, 1, nameof(healthFraction));
            if (!isRunning || healthFraction <= 0) return Array.Empty<EnemyShotCommand>();
            var previous = PhaseIndex;
            while (PhaseIndex + 1 < _definition.Phases.Count &&
                   healthFraction <= _definition.Phases[PhaseIndex + 1].HealthThreshold)
                PhaseIndex++;
            if (previous != PhaseIndex)
            {
                _attackIndex = 0;
                InitializeSequence();
                PhaseChanged?.Invoke(previous, PhaseIndex);
            }
            else if (Attack.CycleCompletesWithin(deltaTime))
            {
                _attackIndex = (_attackIndex + 1) % Phase.Attacks.Count;
                Attack.RestartCycle();
            }
            return Attack.Tick(deltaTime, true, aim);
        }
    }
}
