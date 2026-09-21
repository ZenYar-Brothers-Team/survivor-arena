using Game.Meta;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Game.Bootstrap.PlayModeTests
{
    public static class ProfileSmokeScene
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
            root.ConfigureProfile(new ProfileService(MetaCatalog.Load(true), new MemoryProfileStore()));
        }
    }
}
