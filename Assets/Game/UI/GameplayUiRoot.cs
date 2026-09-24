using Game.Traveler;
using System;
using System.Collections.Generic;
using Game.Character;
using Game.Enemy;
using Game.Progression;
using Game.Presentation;
using Game.Run;
using UnityEngine;
using UnityEngine.UIElements;
using Game.Telemetry;
using Game.Pickup;
using Game.Content;

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
        private PlaytestPresenter _playtestPresenter;
        private UiToolkitPlaytestView _playtestView;
        private PickupPresenter _pickupPresenter;
        private TravelerPresenter _travelerPresenter;
        private UiToolkitTravelerView _travelerView;
        private UiToolkitPickupView _pickupView;
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
            ContentRegistry registry,
            IReadOnlyList<CharacterDefinition> unlockedCharacters = null,
            ContinuousFixtureEnemySpawner enemySpawner = null,
            IPlaytestSession playtest = null, IBossEncounterRuntime bosses = null, IPickupRuntime pickups = null, ITravelerRuntime travelers = null)
        {
            if (_initialized)
                throw new InvalidOperationException("Gameplay UI root is already initialized.");

            var visualTree = Resources.Load<VisualTreeAsset>("UI/GameplayUi");
            var styleSheet = Resources.Load<StyleSheet>("UI/GameplayUiStyles");
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
            // Separate panels require their own render and input order (IP-26).
            _panelSettings.sortingOrder = _document.sortingOrder;
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
                enemySpawner, bosses);
            _presenter = new GameplayUiPresenter(_model, _view, registry);
            _presenter.Start();
            _playtestView = new UiToolkitPlaytestView(_document.rootVisualElement);
            _playtestPresenter = new PlaytestPresenter(Debug.isDebugBuild || Application.isEditor ? playtest : null, _playtestView);
            _pickupView = new UiToolkitPickupView(_document.rootVisualElement);
            _pickupPresenter = new PickupPresenter(pickups, _pickupView, Debug.isDebugBuild || Application.isEditor);
            _travelerView = new UiToolkitTravelerView(_document.rootVisualElement);
            var camera = Camera.main;
            _travelerPresenter = new TravelerPresenter(travelers, _travelerView,
                position => camera != null ? camera.WorldToViewportPoint(position) : Vector3.zero, Debug.isDebugBuild || Application.isEditor);
            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized)
                return;
            _travelerPresenter.Refresh();
            _hudRefreshRemaining -= Time.unscaledDeltaTime;
            if (_hudRefreshRemaining > 0f)
                return;
            _hudRefreshRemaining = HudRefreshIntervalSeconds;
            _presenter.RefreshHud();
            _playtestPresenter.Refresh();
        }

        public void Shutdown()
        {
            if (!_initialized)
                return;

            _presenter?.Dispose();
            _travelerPresenter?.Dispose();
            _travelerView?.Dispose();
            _pickupPresenter?.Dispose();
            _pickupView?.Dispose();
            _playtestPresenter?.Dispose();
            _playtestView?.Dispose();
            _view?.Dispose();
            _model?.Dispose();
            if (_document != null)
            {
                _document.visualTreeAsset = null;
                _document.panelSettings = null;
            }
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
            _travelerPresenter?.Dispose(); _travelerView?.Dispose();
            _playtestPresenter?.Dispose();
            _playtestView?.Dispose();
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
