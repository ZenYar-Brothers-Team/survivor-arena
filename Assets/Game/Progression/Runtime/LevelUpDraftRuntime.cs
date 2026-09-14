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
        public DraftRunControls Controls { get; private set; }
        public int RemainingRerolls => Controls?.RemainingRerolls ?? 0;
        public int RemainingBanishes => Controls?.RemainingBanishes ?? 0;

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
            IDraftRandom draftRandom = null,
            int initialRerolls = 0,
            int initialBanishes = 0)
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
            Controls = new DraftRunControls(initialRerolls, initialBanishes);
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

        public bool Reroll()
        {
            if (!IsDraftOpen || !Controls.TryConsumeReroll())
                return false;

            var options = _pool.CreateRerolledOptions(
                Build,
                _offerCount,
                _draftRandom,
                Controls.BanishedIds,
                CurrentDraft.Options);
            ReplaceCurrentDraft(options);
            return true;
        }

        public bool Banish(ContentId id)
        {
            if (!IsDraftOpen || !IsCurrentOption(id) || !Controls.TryBanish(id))
                return false;

            OpenDraft();
            return true;
        }

        public void ResetControlsForNewRun()
        {
            if (!_initialized)
                throw new InvalidOperationException("Level-up draft runtime must be initialized before reset.");
            if (IsDraftOpen || _pendingDrafts > 0)
                throw new InvalidOperationException("Draft controls cannot be reset while a level-up draft is pending.");

            Controls.Reset();
        }

        private void HandleLevelUp(int _)
        {
            _pendingDrafts++;
            if (!IsDraftOpen)
                OpenNextDraft();
        }

        private void OpenNextDraft()
        {
            OpenDraft();
        }

        private void OpenDraft()
        {
            var options = _pool.CreateOptions(Build, _offerCount, _draftRandom, Controls.BanishedIds);
            if (options.Count == 0)
            {
                CurrentDraft = null;
                _pendingDrafts--;
                if (_pendingDrafts > 0)
                    OpenNextDraft();
                else if (runController.Model != null)
                    runController.Model.ReleasePause(RunPauseReasons.LevelUpDraft);
                return;
            }

            ReplaceCurrentDraft(options);
        }

        private void ReplaceCurrentDraft(IReadOnlyList<DraftOption> options)
        {
            CurrentDraft = new DraftSession(Build, options);
            DraftOpened?.Invoke(options);
        }

        private bool IsCurrentOption(ContentId id)
        {
            for (var i = 0; i < CurrentDraft.Options.Count; i++)
            {
                if (CurrentDraft.Options[i].Definition.Id == id)
                    return true;
            }
            return false;
        }

        private void OnGUI()
        {
            if (!IsDraftOpen)
                return;

            const float width = 520f;
            var height = 130f + CurrentDraft.Options.Count * 48f;
            var rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label("LEVEL UP — выберите улучшение");
            GUILayout.BeginHorizontal();
            GUI.enabled = RemainingRerolls > 0;
            if (GUILayout.Button($"Reroll ({RemainingRerolls})", GUILayout.Height(32f)))
                Reroll();
            GUI.enabled = true;
            GUILayout.Label($"Banish: {RemainingBanishes}");
            GUILayout.EndHorizontal();
            for (var i = 0; i < CurrentDraft.Options.Count; i++)
            {
                var option = CurrentDraft.Options[i];
                var typeLabel = option.Definition.Kind == BuildEntryKind.ActiveSkill ? "Активное" : "Пассивное";
                var actionLabel = option.IsUpgrade ? $"уровень {option.ResultingLevel}" : "новое, уровень 1";
                var draftChanged = false;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button($"{typeLabel}: {option.Definition.DisplayName} — {actionLabel}", GUILayout.Height(40f)))
                {
                    Select(option.Definition.Id);
                    draftChanged = true;
                }
                GUI.enabled = RemainingBanishes > 0;
                if (GUILayout.Button("Banish", GUILayout.Width(90f), GUILayout.Height(40f)))
                {
                    Banish(option.Definition.Id);
                    draftChanged = true;
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                if (draftChanged)
                    break;
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
