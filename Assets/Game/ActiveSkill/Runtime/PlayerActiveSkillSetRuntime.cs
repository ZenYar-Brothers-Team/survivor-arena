using System;
using System.Collections.Generic;
using Game.Character;
using Game.Content;
using Game.Progression;
using Game.Run;
using UnityEngine;

namespace Game.ActiveSkill
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerCharacterRuntime))]
    public sealed class PlayerActiveSkillSetRuntime : MonoBehaviour
    {
        [SerializeField]
        private PlayerCharacterRuntime owner;

        [SerializeField]
        private RunController runController;

        [SerializeField]
        private LevelUpDraftRuntime draftRuntime;

        private readonly Dictionary<ContentId, ActiveSkillProgressionDefinition> _catalog =
            new Dictionary<ContentId, ActiveSkillProgressionDefinition>();
        private readonly Dictionary<ContentId, ActiveSkillInstance> _instances =
            new Dictionary<ContentId, ActiveSkillInstance>();
        private IActiveSkillTargetProvider _targetProvider;
        private IActiveSkillEffectExecutor _executor;
        private bool _initialized;

        public int SkillCount => _instances.Count;
        public IEnumerable<ActiveSkillInstance> Skills => _instances.Values;

        private void Start()
        {
            if (_initialized)
                return;
            Debug.LogError("Player active-skill set runtime must be initialized by the gameplay composition root.", this);
            enabled = false;
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void Initialize(
            PlayerCharacterRuntime skillOwner,
            RunController controller,
            LevelUpDraftRuntime drafts,
            IEnumerable<ActiveSkillProgressionDefinition> definitions,
            IActiveSkillTargetProvider targetProvider,
            IActiveSkillEffectExecutor executor)
        {
            if (_initialized)
                throw new InvalidOperationException("Player active-skill set runtime is already initialized.");
            owner = skillOwner != null ? skillOwner : throw new ArgumentNullException(nameof(skillOwner));
            runController = controller != null ? controller : throw new ArgumentNullException(nameof(controller));
            draftRuntime = drafts != null ? drafts : throw new ArgumentNullException(nameof(drafts));
            _targetProvider = targetProvider ?? throw new ArgumentNullException(nameof(targetProvider));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions));

            try
            {
                foreach (var definition in definitions)
                {
                    if (definition == null)
                        throw new ArgumentException("Skill catalog cannot contain null definitions.", nameof(definitions));
                    if (!_catalog.TryAdd(definition.Id, definition))
                        throw new ArgumentException($"Duplicate active skill id '{definition.Id}'.", nameof(definitions));
                }
                if (_catalog.Count == 0)
                    throw new ArgumentException("Skill catalog cannot be empty.", nameof(definitions));

                // Subscribe only after the build synchronized: a throw above or in
                // SynchronizeBuild() must not leave a live subscription behind, because
                // Shutdown() is a no-op until _initialized is set.
                SynchronizeBuild();
                draftRuntime.SelectionApplied += HandleSelectionApplied;
                _initialized = true;
            }
            catch
            {
                _catalog.Clear();
                _instances.Clear();
                throw;
            }
        }

        public bool Tick(float deltaTime)
        {
            if (!_initialized || runController.Model == null || draftRuntime.Build == null)
                return false;

            SynchronizeBuild();
            var isRunning = runController.Model.State == RunState.Running;
            _executor.Tick(deltaTime, isRunning);
            var triggered = false;
            foreach (var instance in _instances.Values)
            {
                if (instance.Tick(deltaTime, isRunning, owner, _targetProvider, _executor))
                    triggered = true;
            }
            _executor.Tick(0f, isRunning);
            return triggered;
        }

        public bool TryGetSkill(ContentId id, out ActiveSkillInstance instance)
        {
            return _instances.TryGetValue(id, out instance);
        }

        private void HandleSelectionApplied(BuildSelectionResult result)
        {
            if (result.Entry.Definition.Kind == BuildEntryKind.ActiveSkill)
                SynchronizeEntry(result.Entry);
        }

        private void SynchronizeBuild()
        {
            if (draftRuntime.Build == null)
                return;
            foreach (var entry in draftRuntime.Build.Entries)
            {
                if (entry.Definition.Kind == BuildEntryKind.ActiveSkill)
                    SynchronizeEntry(entry);
            }
        }

        private void SynchronizeEntry(BuildEntry entry)
        {
            if (!_catalog.TryGetValue(entry.Definition.Id, out var definition))
                throw new InvalidOperationException($"Active build entry '{entry.Definition.Id}' is missing from the runtime catalog.");
            if (!_instances.TryGetValue(definition.Id, out var instance))
            {
                instance = new ActiveSkillInstance(definition);
                _instances.Add(definition.Id, instance);
            }
            instance.SetLevel(entry.Level);
        }

        public void Shutdown()
        {
            if (!_initialized)
                return;

            if (draftRuntime != null)
                draftRuntime.SelectionApplied -= HandleSelectionApplied;
            if (_executor is IDisposable disposable)
                disposable.Dispose();
            _initialized = false;
        }

        private void OnDestroy()
        {
            Shutdown();
        }
    }
}
