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

        [SerializeField, Min(0.0001f)]
        private float baseMaxHealth = 100f;

        [SerializeField, Min(0f)]
        private float baseMovementSpeed = 3f;

        [SerializeField, Min(0f)]
        private float baseActiveSkillDamageMultiplier = 1f;

        [SerializeField, Min(0f)]
        private float baseActiveSkillCooldownMultiplier = 1f;

        [SerializeField, Min(0f)]
        private float baseIncomingDamageMultiplier = 1f;

        [SerializeField, Min(0f)]
        private float baseHealthRestorationMultiplier = 1f;

        [SerializeField, Min(0f)]
        private float baseHealthRegenerationPerSecond;

        [SerializeField, Range(0f, 1f)]
        private float baseDisappearingXpRecovery;

        [SerializeField, Min(0f)]
        private float basePickedUpXpMultiplier = 1f;

        [SerializeField, Min(0f)]
        private float baseXpDropLifetimeBonusSeconds;

        private CharacterRunBinding _runBinding;

        public CharacterStats Stats { get; private set; }
        public CharacterHealth Health { get; private set; }
        public float MovementSpeed => Stats != null ? Stats.MovementSpeed : baseMovementSpeed;

        private void Awake()
        {
            var baseStats = new CharacterBaseStats(
                baseMaxHealth,
                baseMovementSpeed,
                baseActiveSkillDamageMultiplier,
                baseActiveSkillCooldownMultiplier,
                baseIncomingDamageMultiplier,
                baseHealthRestorationMultiplier,
                baseHealthRegenerationPerSecond,
                baseDisappearingXpRecovery,
                basePickedUpXpMultiplier,
                baseXpDropLifetimeBonusSeconds);

            Stats = new CharacterStats(baseStats);
            Health = new CharacterHealth(Stats);
        }

        private void Start()
        {
            if (runController == null || runController.Model == null)
            {
                Debug.LogError("Player character run controller is not configured.", this);
                enabled = false;
                return;
            }

            _runBinding = new CharacterRunBinding(Health, runController.Model);
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
