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
            }
        }

        // Base stats are supplied by the caller (the gameplay composition root in
        // production) rather than loaded here, so this runtime doesn't need to know
        // where content comes from — see FixtureCharacterCatalog and AGENTS.md's
        // content-config rule for how the composition root sources them.
        //
        // Health and its RunController binding are created together here, atomically,
        // so a Health that can take damage and die can never exist without its death
        // already being wired to end the run.
        public void Initialize(CharacterBaseStats baseStats, RunController controller)
        {
            if (_initialized)
                throw new InvalidOperationException("Player character runtime is already initialized.");
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));
            if (controller.Model == null)
                throw new InvalidOperationException("Player character run controller is not configured.");

            runController = controller;
            Stats = new CharacterStats(baseStats);
            Health = new Health(Stats);
            _runBinding = new CharacterRunBinding(Health, controller.Model);
            _initialized = true;
        }

        private void Update()
        {
            var isRunning = runController != null &&
                            runController.Model != null &&
                            runController.Model.State == RunState.Running;
            Health.Regenerate(Time.deltaTime, isRunning);
        }

        // Undoes exactly what Initialize() set up, so a partially-initialized
        // GameplayCompositionRoot can roll this subsystem back without destroying
        // the GameObject. Safe to call whether or not Initialize() ever ran.
        public void Shutdown()
        {
            if (!_initialized)
                return;

            _runBinding?.Dispose();
            Health?.Dispose();
            _runBinding = null;
            Health = null;
            Stats = null;
            _initialized = false;
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
