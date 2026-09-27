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
    /// A step may be a zone/beam/summon special (DECISION-0066): it starts on its turn and is reported once through
    /// <see cref="TriggeredSpecial"/> for the tick it starts.
    /// </summary>
    public sealed class BossCombatController
    {
        private readonly BossEncounterDefinition _definition;
        private int _attackIndex;
        private EnemyAttackController[] _sequence;
        private BossSpecialTimer[] _specials;
        private EnemyAttackController _current;
        private BossSpecialTimer _currentSpecial;
        private BossSpecialAttack _currentSpecialAttack;
        private EnemyDefinition _currentDefinition;
        public int PhaseIndex { get; private set; }
        public BossPhaseDefinition Phase => _definition.Phases[PhaseIndex];
        /// <summary>Carrier of the step currently scheduled; null for a boss without attacks.</summary>
        public EnemyDefinition AttackDefinition => _currentDefinition;
        /// <summary>Projectile controller of the current step; null for a special step or no attacks.</summary>
        public EnemyAttackController Attack => _current;
        /// <summary>Special started during the last <see cref="Tick"/>; null otherwise.</summary>
        public BossSpecialAttack TriggeredSpecial { get; private set; }
        /// <summary>Carrier id of <see cref="TriggeredSpecial"/>.</summary>
        public ContentId TriggeredSpecialId { get; private set; }
        /// <summary>Time until the current special step hands over; 0 for projectile steps.</summary>
        public float SpecialRemaining => _currentSpecial?.Remaining ?? 0f;
        public event Action<int, int> PhaseChanged;

        public BossCombatController(BossEncounterDefinition definition)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            InitializeSequence();
            Select(0);
        }

        private void InitializeSequence()
        {
            var count = Phase.Attacks.Count;
            _sequence = new EnemyAttackController[count];
            _specials = new BossSpecialTimer[count];
            for (var i = 0; i < count; i++)
            {
                var special = Phase.Specials[i];
                if (special != null) _specials[i] = new BossSpecialTimer(special.CooldownSeconds);
                else _sequence[i] = new EnemyAttackController(Phase.Attacks[i].Attack, repeat: false,
                    random: new System.Random(StableSeed(Phase.Attacks[i].Id.ToString())));
            }
        }

        // Burst aim deviation (BOSS-006) needs a random source; seeded per step so encounters replay identically.
        private static int StableSeed(string text)
        {
            unchecked
            {
                var hash = (int)2166136261;
                foreach (var c in text) hash = (hash ^ c) * 16777619;
                return hash;
            }
        }

        private void Select(int index)
        {
            if (_sequence.Length == 0)
            {
                _current = null; _currentSpecial = null; _currentSpecialAttack = null; _currentDefinition = null;
                return;
            }
            _attackIndex = index % _sequence.Length;
            _current = _sequence[_attackIndex];
            _currentSpecial = _specials[_attackIndex];
            _currentSpecialAttack = Phase.Specials[_attackIndex];
            _currentDefinition = Phase.Attacks[_attackIndex];
        }

        private bool Crossed(float healthFraction, float threshold) =>
            _definition.StrictHealthThreshold ? healthFraction < threshold : healthFraction <= threshold;

        private bool CurrentCycleCompletesWithin(float deltaTime) =>
            _current != null ? _current.CycleCompletesWithin(deltaTime) : _currentSpecial != null && _currentSpecial.CycleCompletesWithin(deltaTime);

        public EnemyShotCommand[] Tick(float deltaTime, bool isRunning, float healthFraction, Vector2 aim)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            NumericValidation.ValidateRange(healthFraction, 0, 1, nameof(healthFraction));
            TriggeredSpecial = null;
            TriggeredSpecialId = default;
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
            else if (CurrentCycleCompletesWithin(deltaTime))
            {
                Select(_attackIndex + 1);
                _current?.RestartCycle();
                _currentSpecial?.RestartCycle();
            }
            if (_currentSpecial != null)
            {
                if (_currentSpecial.Tick(deltaTime, true))
                {
                    TriggeredSpecial = _currentSpecialAttack;
                    TriggeredSpecialId = _currentDefinition.Id;
                }
                return Array.Empty<EnemyShotCommand>();
            }
            return _current == null ? Array.Empty<EnemyShotCommand>() : _current.Tick(deltaTime, true, aim);
        }
    }
}
