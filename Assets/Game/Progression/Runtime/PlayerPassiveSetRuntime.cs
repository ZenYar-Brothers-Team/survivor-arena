using System;
using System.Collections.Generic;
using Game.Character;
using Game.Content;
using UnityEngine;

namespace Game.Progression
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerCharacterRuntime))]
    public sealed class PlayerPassiveSetRuntime : MonoBehaviour
    {
        private const string ModifierSourcePrefix = "passive:";

        [SerializeField]
        private PlayerCharacterRuntime owner;

        [SerializeField]
        private LevelUpDraftRuntime draftRuntime;

        private readonly Dictionary<ContentId, PassiveProgressionDefinition> _catalog =
            new Dictionary<ContentId, PassiveProgressionDefinition>();
        private readonly Dictionary<ContentId, int> _appliedLevels = new Dictionary<ContentId, int>();
        private bool _initialized;

        public int PassiveCount => _appliedLevels.Count;

        private void Start()
        {
            if (_initialized)
                return;
            Debug.LogError("Player passive-set runtime must be initialized by the gameplay composition root.", this);
            enabled = false;
        }

        public void Initialize(
            PlayerCharacterRuntime characterOwner,
            LevelUpDraftRuntime drafts,
            IEnumerable<PassiveProgressionDefinition> definitions)
        {
            if (_initialized)
                throw new InvalidOperationException("Player passive-set runtime is already initialized.");
            owner = characterOwner != null ? characterOwner : throw new ArgumentNullException(nameof(characterOwner));
            draftRuntime = drafts != null ? drafts : throw new ArgumentNullException(nameof(drafts));
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions));

            foreach (var definition in definitions)
            {
                if (definition == null)
                    throw new ArgumentException("Passive catalog cannot contain null definitions.", nameof(definitions));
                if (!_catalog.TryAdd(definition.Id, definition))
                    throw new ArgumentException($"Duplicate passive id '{definition.Id}'.", nameof(definitions));
            }
            if (_catalog.Count == 0)
                throw new ArgumentException("Passive catalog cannot be empty.", nameof(definitions));

            draftRuntime.SelectionApplied += HandleSelectionApplied;
            SynchronizeBuild();
            _initialized = true;
        }

        public bool TryGetAppliedLevel(ContentId id, out int level)
        {
            return _appliedLevels.TryGetValue(id, out level);
        }

        private void HandleSelectionApplied(BuildSelectionResult result)
        {
            if (result.Entry.Definition.Kind == BuildEntryKind.PassiveItem)
                SynchronizeEntry(result.Entry);
        }

        private void SynchronizeBuild()
        {
            if (draftRuntime.Build == null)
                return;
            foreach (var entry in draftRuntime.Build.Entries)
            {
                if (entry.Definition.Kind == BuildEntryKind.PassiveItem)
                    SynchronizeEntry(entry);
            }
        }

        private void SynchronizeEntry(BuildEntry entry)
        {
            if (!_catalog.TryGetValue(entry.Definition.Id, out var definition))
                throw new InvalidOperationException($"Passive build entry '{entry.Definition.Id}' is missing from the runtime catalog.");
            if (_appliedLevels.TryGetValue(definition.Id, out var appliedLevel) && appliedLevel == entry.Level)
                return;

            owner.SetModifier(ModifierSourcePrefix + definition.Id, definition.GetLevel(entry.Level));
            _appliedLevels[definition.Id] = entry.Level;
        }

        public void Shutdown()
        {
            if (!_initialized)
                return;

            if (draftRuntime != null)
                draftRuntime.SelectionApplied -= HandleSelectionApplied;
            if (owner != null)
            {
                foreach (var id in _appliedLevels.Keys)
                    owner.RemoveModifier(ModifierSourcePrefix + id);
            }
            _appliedLevels.Clear();
            _initialized = false;
        }

        private void OnDestroy()
        {
            Shutdown();
        }
    }
}
