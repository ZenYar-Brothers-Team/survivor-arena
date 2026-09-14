using System;
using System.Collections.Generic;
using Game.Content;
using Game.Run;
using UnityEngine;

namespace Game.Progression
{
    [DisallowMultipleComponent]
    public sealed class LevelUpDraftRuntime : MonoBehaviour
    {
        [SerializeField]
        private PlayerExperienceRuntime experienceRuntime;

        [SerializeField]
        private RunController runController;

        [SerializeField]
        private string fixtureStartingActiveId = "FIXTURE-SKILL-BOLT";

        [SerializeField]
        private string[] fixtureActiveIds =
        {
            "FIXTURE-SKILL-BOLT",
            "FIXTURE-SKILL-ARC",
            "FIXTURE-SKILL-ORBIT"
        };

        [SerializeField]
        private string[] fixturePassiveIds =
        {
            "FIXTURE-PASSIVE-VITALITY",
            "FIXTURE-PASSIVE-HASTE",
            "FIXTURE-PASSIVE-MEMORY"
        };

        [SerializeField, Min(1)]
        private int fixtureOfferCount = 3;

        private DraftPool _pool;
        private int _draftOffset;
        private int _pendingDrafts;
        private bool _initialized;

        public PlayerBuild Build { get; private set; }
        public DraftSession CurrentDraft { get; private set; }
        public bool IsDraftOpen => CurrentDraft != null && CurrentDraft.IsOpen;
        public int PendingDraftCount => _pendingDrafts;

        public event Action<IReadOnlyList<DraftOption>> DraftOpened;
        public event Action<BuildSelectionResult> SelectionApplied;

        private void Start()
        {
            if (_initialized)
                return;

            if (experienceRuntime == null || runController == null || runController.Model == null)
            {
                Debug.LogError("Level-up draft runtime is not configured.", this);
                enabled = false;
                return;
            }

            try
            {
                var definitions = CreateFixtureDefinitions();
                var startingActive = FindDefinition(definitions, new ContentId(fixtureStartingActiveId));
                Initialize(experienceRuntime, runController, definitions, startingActive, fixtureOfferCount);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Invalid level-up draft fixture configuration: {exception.Message}", this);
                enabled = false;
            }
        }

        public void Initialize(
            PlayerExperienceRuntime experience,
            RunController controller,
            IEnumerable<BuildEntryDefinition> definitions,
            BuildEntryDefinition startingActive,
            int offerCount)
        {
            if (_initialized)
                throw new InvalidOperationException("Level-up draft runtime is already initialized.");
            if (offerCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(offerCount));

            experienceRuntime = experience != null ? experience : throw new ArgumentNullException(nameof(experience));
            runController = controller != null ? controller : throw new ArgumentNullException(nameof(controller));
            _pool = new DraftPool(definitions);
            Build = new PlayerBuild(startingActive);
            fixtureOfferCount = offerCount;
            experienceRuntime.LevelUp += HandleLevelUp;
            _initialized = true;
        }

        public bool Select(ContentId id)
        {
            if (!IsDraftOpen || !CurrentDraft.TrySelect(id, out var result))
                return false;

            _pendingDrafts--;
            SelectionApplied?.Invoke(result);

            if (_pendingDrafts > 0)
                OpenNextDraft();
            else if (runController.Model != null)
                runController.Model.Resume();
            return true;
        }

        private void HandleLevelUp(int _)
        {
            _pendingDrafts++;
            if (!IsDraftOpen)
                OpenNextDraft();
        }

        private void OpenNextDraft()
        {
            var options = _pool.CreateOptions(Build, fixtureOfferCount, _draftOffset++);
            if (options.Count == 0)
            {
                _pendingDrafts--;
                if (_pendingDrafts > 0)
                    OpenNextDraft();
                else if (runController.Model != null)
                    runController.Model.Resume();
                return;
            }

            CurrentDraft = new DraftSession(Build, options);
            DraftOpened?.Invoke(options);
        }

        private List<BuildEntryDefinition> CreateFixtureDefinitions()
        {
            var definitions = new List<BuildEntryDefinition>();
            var activeCount = fixtureActiveIds != null ? fixtureActiveIds.Length : 0;
            var passiveCount = fixturePassiveIds != null ? fixturePassiveIds.Length : 0;
            var maxCount = Math.Max(activeCount, passiveCount);
            for (var i = 0; i < maxCount; i++)
            {
                if (i < activeCount)
                    AddFixtureDefinition(definitions, fixtureActiveIds[i], BuildEntryKind.ActiveSkill);
                if (i < passiveCount)
                    AddFixtureDefinition(definitions, fixturePassiveIds[i], BuildEntryKind.PassiveItem);
            }
            return definitions;
        }

        private static void AddFixtureDefinition(
            ICollection<BuildEntryDefinition> destination,
            string id,
            BuildEntryKind kind)
        {
            if (string.IsNullOrWhiteSpace(id) || !id.StartsWith("FIXTURE-", StringComparison.Ordinal))
                throw new ArgumentException("Prototype draft entries must use FIXTURE-* ids.", nameof(id));
            destination.Add(new BuildEntryDefinition(new ContentId(id), kind, id));
        }

        private static BuildEntryDefinition FindDefinition(
            IEnumerable<BuildEntryDefinition> definitions,
            ContentId id)
        {
            foreach (var definition in definitions)
            {
                if (definition.Id == id)
                    return definition;
            }
            throw new ArgumentException("Starting active id must exist in the active fixture pool.");
        }

        private void OnGUI()
        {
            if (!IsDraftOpen)
                return;

            const float width = 520f;
            var height = 90f + CurrentDraft.Options.Count * 48f;
            var rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label("LEVEL UP — выберите улучшение");
            for (var i = 0; i < CurrentDraft.Options.Count; i++)
            {
                var option = CurrentDraft.Options[i];
                var typeLabel = option.Definition.Kind == BuildEntryKind.ActiveSkill ? "Активное" : "Пассивное";
                var actionLabel = option.IsUpgrade ? $"уровень {option.ResultingLevel}" : "новое, уровень 1";
                if (GUILayout.Button($"{typeLabel}: {option.Definition.DisplayName} — {actionLabel}", GUILayout.Height(40f)))
                    Select(option.Definition.Id);
            }
            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            if (_initialized && experienceRuntime != null)
                experienceRuntime.LevelUp -= HandleLevelUp;
        }
    }
}
