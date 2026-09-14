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

        private DraftPool _pool;
        private IDraftRandom _draftRandom;
        private int _offerCount;
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

            Debug.LogError("Level-up draft runtime must be initialized by the gameplay composition root.", this);
            enabled = false;
        }

        public void Initialize(
            PlayerExperienceRuntime experience,
            RunController controller,
            IEnumerable<BuildEntryDefinition> definitions,
            BuildEntryDefinition startingActive,
            int offerCount,
            IDraftRandom draftRandom = null)
        {
            if (_initialized)
                throw new InvalidOperationException("Level-up draft runtime is already initialized.");
            if (offerCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(offerCount));

            experienceRuntime = experience != null ? experience : throw new ArgumentNullException(nameof(experience));
            runController = controller != null ? controller : throw new ArgumentNullException(nameof(controller));
            _pool = new DraftPool(definitions);
            _draftRandom = draftRandom ?? new SeededDraftRandom(0);
            _offerCount = offerCount;
            Build = new PlayerBuild(startingActive);
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
                runController.Model.ReleasePause(RunPauseReasons.LevelUpDraft);
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
            var options = _pool.CreateOptions(Build, _offerCount, _draftRandom);
            if (options.Count == 0)
            {
                _pendingDrafts--;
                if (_pendingDrafts > 0)
                    OpenNextDraft();
                else if (runController.Model != null)
                    runController.Model.ReleasePause(RunPauseReasons.LevelUpDraft);
                return;
            }

            CurrentDraft = new DraftSession(Build, options);
            DraftOpened?.Invoke(options);
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
