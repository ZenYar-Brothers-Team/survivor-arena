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
using UnityEngine.Rendering;
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
        private Camera _anchorCamera;
        private SpriteRenderer _playerBody;

        public UIDocument Document => _document;
        public bool IsInitialized => _initialized;
        public VisualElement PauseFooter => _document?.rootVisualElement.Q(GameplayUiElementIds.PauseFooter);
        public bool ConsumePauseShortcut(bool space) => _view?.ConsumePauseShortcut(space) == true;

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
            IPlaytestSession playtest = null, IBossEncounterRuntime bosses = null, IPickupRuntime pickups = null, ITravelerRuntime travelers = null,
            IReadOnlyList<BuildEntryDefinition> allDraftEntries = null)
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
            _panelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
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
            _playerBody = presentation.GetComponent<SpritePresentationRig>().BodyRenderer;
            _model = new GameplayUiRuntimeModel(
                player,
                experience,
                draft,
                run,
                presentation,
                Debug.isDebugBuild || Application.isEditor,
                unlockedCharacters,
                enemySpawner, bosses, allDraftEntries);
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
            BindHealthAnchor(camera);
        }

        // Rebind after render-only camera effects so projection observes the actual
        // presentation pose without leaking shake into gameplay coordinates.
        public void BindHealthAnchor(Camera camera)
        {
            RenderPipelineManager.beginCameraRendering -= OnCameraRendering;
            _anchorCamera = camera;
            RenderPipelineManager.beginCameraRendering += OnCameraRendering;
        }

        private void LateUpdate() => RefreshHealthAnchor();
        private void OnCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == _anchorCamera) RefreshHealthAnchor();
        }

        public void RefreshHealthAnchor()
        {
            if (!_initialized || _document?.rootVisualElement.panel == null || _anchorCamera == null || _playerBody == null) return;
            var root = _document.rootVisualElement;
            var bar = root.Q<ProgressBar>(GameplayUiElementIds.HealthBar);
            var bounds = _playerBody.bounds;
            var screen = _anchorCamera.WorldToViewportPoint(new Vector3(bounds.center.x, bounds.max.y, bounds.center.z));
            bar.style.display = _playerBody.enabled && screen.z > 0 && screen.x >= 0 && screen.x <= 1 && screen.y >= 0 && screen.y <= 1
                ? DisplayStyle.Flex : DisplayStyle.None;
            bar.style.left = screen.x * root.layout.width - 32;
            bar.style.top = (1 - screen.y) * root.layout.height - 18;
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
            RenderPipelineManager.beginCameraRendering -= OnCameraRendering;
            _anchorCamera = null; _playerBody = null;
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
            RenderPipelineManager.beginCameraRendering -= OnCameraRendering;
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
