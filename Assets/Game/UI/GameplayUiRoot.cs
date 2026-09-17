using System;
using System.Collections.Generic;
using Game.Character;
using Game.Enemy;
using Game.Progression;
using Game.Presentation;
using Game.Run;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    [DefaultExecutionOrder(-900)]
    [DisallowMultipleComponent]
    public sealed class GameplayUiRoot : MonoBehaviour
    {
        private const float HudRefreshIntervalSeconds = 0.1f;

        private UIDocument _document;
        private PanelSettings _panelSettings;
        private GameplayUiRuntimeModel _model;
        private UiToolkitGameplayView _view;
        private GameplayUiPresenter _presenter;
        private float _hudRefreshRemaining;
        private bool _initialized;

        public UIDocument Document => _document;
        public bool IsInitialized => _initialized;

        private void Start()
        {
            if (_initialized)
                return;
            Debug.LogError("Gameplay UI root must be initialized by the gameplay composition root.", this);
            enabled = false;
        }

        public void Initialize(
            PlayerCharacterRuntime player,
            PlayerExperienceRuntime experience,
            LevelUpDraftRuntime draft,
            RunController run,
            SpritePresentationRuntime presentation,
            IReadOnlyList<CharacterDefinition> unlockedCharacters = null,
            ContinuousFixtureEnemySpawner enemySpawner = null)
        {
            if (_initialized)
                throw new InvalidOperationException("Gameplay UI root is already initialized.");

            var visualTree = Resources.Load<VisualTreeAsset>("UI/GameplayUi");
            var styleSheet = Resources.Load<StyleSheet>("UI/GameplayUi");
            var themeStyleSheet = Resources.Load<ThemeStyleSheet>("UI/GameplayTheme");
            if (visualTree == null || styleSheet == null || themeStyleSheet == null)
                throw new InvalidOperationException("Gameplay UI UXML/USS/theme resources are missing.");

            _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            _panelSettings.name = "Runtime Gameplay UI Panel Settings";
            _panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            _panelSettings.referenceResolution = new Vector2Int(1920, 1080);
            _panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            _panelSettings.match = 0.5f;
            _panelSettings.themeStyleSheet = themeStyleSheet;

            _document = GetComponent<UIDocument>();
            if (_document == null)
                _document = gameObject.AddComponent<UIDocument>();
            _document.panelSettings = _panelSettings;
            _document.sortingOrder = 100;
            _document.visualTreeAsset = visualTree;
            _document.rootVisualElement.styleSheets.Add(styleSheet);

            _view = new UiToolkitGameplayView(_document.rootVisualElement);
            _model = new GameplayUiRuntimeModel(
                player,
                experience,
                draft,
                run,
                presentation,
                Debug.isDebugBuild || Application.isEditor,
                unlockedCharacters,
                enemySpawner);
            _presenter = new GameplayUiPresenter(_model, _view);
            _presenter.Start();
            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized)
                return;
            _hudRefreshRemaining -= Time.unscaledDeltaTime;
            if (_hudRefreshRemaining > 0f)
                return;
            _hudRefreshRemaining = HudRefreshIntervalSeconds;
            _presenter.RefreshHud();
        }

        public void Shutdown()
        {
            if (!_initialized)
                return;

            _presenter?.Dispose();
            _view?.Dispose();
            _model?.Dispose();
            if (_panelSettings != null)
            {
                if (Application.isPlaying)
                    Destroy(_panelSettings);
                else
                    DestroyImmediate(_panelSettings);
            }
            _presenter = null;
            _view = null;
            _model = null;
            _panelSettings = null;
            _initialized = false;
        }

        private void OnDestroy()
        {
            _presenter?.Dispose();
            _view?.Dispose();
            _model?.Dispose();
            if (_panelSettings == null)
                return;
            if (Application.isPlaying)
                Destroy(_panelSettings);
            else
                DestroyImmediate(_panelSettings);
        }
    }
}
