using System;
using Game.Combat;
using Game.Movement;
using Game.Run;
using UnityEngine;

namespace Game.Character
{
    [DisallowMultipleComponent]
    public sealed class PlayerCharacterRuntime : MonoBehaviour, IMovementSpeedSource
    {
        [SerializeField]
        private RunController runController;

        private bool _initialized;
        private CharacterRunBinding _runBinding;

        public CharacterStats Stats { get; private set; }
        public Health Health { get; private set; }
        public float MovementSpeed => Stats != null ? Stats.MovementSpeed : 0f;

        private void Start()
        {
            if (!_initialized)
            {
                Debug.LogError("Player character runtime must be initialized by the gameplay composition root.", this);
                enabled = false;
                return;
            }
            if (runController == null || runController.Model == null)
            {
                Debug.LogError("Player character run controller is not configured.", this);
                enabled = false;
                return;
            }

            _runBinding = new CharacterRunBinding(Health, runController.Model);
        }

        // Base stats are supplied by the caller (the gameplay composition root in
        // production) rather than loaded here, so this runtime doesn't need to know
        // where content comes from — see FixtureCharacterCatalog and AGENTS.md's
        // content-config rule for how the composition root sources them.
        public void Initialize(CharacterBaseStats baseStats)
        {
            if (_initialized)
                throw new InvalidOperationException("Player character runtime is already initialized.");

            Stats = new CharacterStats(baseStats);
            Health = new Health(Stats);
            _initialized = true;
        }

        private void Update()
        {
            var isRunning = runController != null &&
                            runController.Model != null &&
                            runController.Model.State == RunState.Running;
            Health.Regenerate(Time.deltaTime, isRunning);
        }

        private void OnDestroy()
        {
            _runBinding?.Dispose();
            Health?.Dispose();
        }

        public float TakeDamage(float amount)
        {
            return Health.TakeDamage(amount);
        }

        public float Heal(float amount)
        {
            return Health.Heal(amount);
        }

        public void SetModifier(string sourceKey, CharacterStatModifier modifier)
        {
            Stats.SetModifier(sourceKey, modifier);
        }

        public bool RemoveModifier(string sourceKey)
        {
            return Stats.RemoveModifier(sourceKey);
        }
    }
}
