using System;
using Game.Character;
using Game.Content;
using Game.Enemy;
using Game.Run;
using UnityEngine;

namespace Game.ActiveSkill
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerCharacterRuntime))]
    public sealed class PlayerActiveSkillRuntime : MonoBehaviour
    {
        [SerializeField]
        private PlayerCharacterRuntime owner;

        [SerializeField]
        private RunController runController;

        [SerializeField]
        private string fixtureContentId = "FIXTURE-SKILL-BOLT";

        [SerializeField, Min(0f)]
        private float fixtureBaseDamage = 5f;

        [SerializeField, Min(0.0001f)]
        private float fixtureCooldownSeconds = 0.75f;

        [SerializeField, Min(0.0001f)]
        private float fixtureProjectileSpeed = 10f;

        [SerializeField, Min(0.0001f)]
        private float fixtureProjectileLifetimeSeconds = 2f;

        [SerializeField, Min(0.0001f)]
        private float fixtureProjectileCollisionRadius = 0.15f;

        [SerializeField, Min(0f)]
        private float fixtureImpactAreaRadius = 0.5f;

        private ActiveSkillCooldown _cooldown;
        private IActiveSkillTargetProvider _targetProvider;
        private IActiveSkillProjectileLauncher _projectileLauncher;
        private bool _initialized;

        public ActiveSkillDefinition Definition { get; private set; }
        public int TriggerCount { get; private set; }

        private void Start()
        {
            if (_initialized)
                return;

            if (owner == null || runController == null)
            {
                Debug.LogError("Player active skill runtime is not configured.", this);
                enabled = false;
                return;
            }

            Initialize(
                new ActiveSkillDefinition(
                    new ContentId(fixtureContentId),
                    fixtureBaseDamage,
                    fixtureCooldownSeconds,
                    fixtureProjectileSpeed,
                    fixtureProjectileLifetimeSeconds,
                    fixtureProjectileCollisionRadius,
                    fixtureImpactAreaRadius),
                owner,
                runController,
                new SceneEnemyTargetProvider(),
                new SceneProjectileLauncher(runController));
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void Initialize(
            ActiveSkillDefinition definition,
            PlayerCharacterRuntime skillOwner,
            RunController controller,
            IActiveSkillTargetProvider targetProvider,
            IActiveSkillProjectileLauncher projectileLauncher)
        {
            if (_initialized)
                throw new InvalidOperationException("Player active skill runtime is already initialized.");

            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            owner = skillOwner != null ? skillOwner : throw new ArgumentNullException(nameof(skillOwner));
            runController = controller != null ? controller : throw new ArgumentNullException(nameof(controller));
            _targetProvider = targetProvider ?? throw new ArgumentNullException(nameof(targetProvider));
            _projectileLauncher = projectileLauncher ?? throw new ArgumentNullException(nameof(projectileLauncher));
            _cooldown = new ActiveSkillCooldown();
            _initialized = true;
        }

        public bool Tick(float deltaTime)
        {
            if (!_initialized || runController.Model == null || owner.Stats == null)
                return false;

            var isRunning = runController.Model.State == RunState.Running;
            _cooldown.Tick(deltaTime, isRunning);
            if (!isRunning || !_cooldown.IsReady)
                return false;

            var origin = (Vector2)owner.transform.position;
            if (!_targetProvider.TryGetTarget(origin, out var target) || target == null || !target.IsAlive)
                return false;

            var direction = target.Position - origin;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                direction = Vector2.right;

            var damage = new EnemyDamageRequest(
                Definition.Id,
                Definition.BaseDamage * owner.Stats.ActiveSkillDamageMultiplier);
            _projectileLauncher.Launch(new ActiveSkillProjectile(
                origin,
                direction,
                Definition.ProjectileSpeed,
                Definition.ProjectileLifetimeSeconds,
                Definition.ProjectileCollisionRadius,
                Definition.ImpactAreaRadius,
                damage));
            _cooldown.Consume(
                Definition.CooldownSeconds,
                owner.Stats.ActiveSkillCooldownMultiplier);
            TriggerCount++;
            return true;
        }
    }
}
