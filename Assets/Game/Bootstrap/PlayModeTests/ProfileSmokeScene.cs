using Game.Meta;
using Game.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Game.Bootstrap.PlayModeTests
{
    public static class ProfileSmokeScene
    {
        private static bool _openSelection;
        public static void Load(bool openSelection = true)
        {
            _openSelection = openSelection;
            SceneManager.sceneLoaded += Configure;
            SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
        }
        private static void Configure(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= Configure;
            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            root.ConfigureSettings(new SettingsService(SettingsConfig.Load(), new MemorySettingsStore(), new FakeVideoDevice()));
            if (_openSelection)
            {
                void Open() { if (!root.AtMainMenu) return; root.NavigationChanged -= Open; root.Play(); }
                root.NavigationChanged += Open;
            }
            root.ConfigureProfile(new ProfileService(MetaCatalog.Load(true), new MemoryProfileStore()));
        }
    }
}
