using Game.Meta;
using Game.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Bootstrap.PlayModeTests
{
    /// <summary>Loads Gameplay with an isolated in-memory production profile (FIELD-001 startup content).</summary>
    public static class ProductionSmokeScene
    {
        public static void Load()
        {
            SceneManager.sceneLoaded += Configure;
            SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
        }

        private static void Configure(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= Configure;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            root.ConfigureSettings(new SettingsService(SettingsConfig.Load(), new MemorySettingsStore(), new FakeVideoDevice()));
            void Open() { if (!root.AtMainMenu) return; root.NavigationChanged -= Open; root.Play(); }
            root.NavigationChanged += Open;
            root.ConfigureProfile(new ProfileService(MetaCatalog.Load(), new MemoryProfileStore()));
        }
    }
}
