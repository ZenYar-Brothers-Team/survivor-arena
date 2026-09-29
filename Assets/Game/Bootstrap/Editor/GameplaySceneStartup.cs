using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Bootstrap.Editor
{
    /// <summary>Opens the playable scene once per interactive Editor session.</summary>
    [InitializeOnLoad]
    public static class GameplaySceneStartup
    {
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";
        private const string SessionKey = "Game.Bootstrap.Editor.GameplaySceneStartup.Opened";

        static GameplaySceneStartup()
        {
            if (Application.isBatchMode) return;

            var gameplayScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(GameplayScenePath);
            if (gameplayScene == null) return;
            EditorSceneManager.playModeStartScene = gameplayScene;
            if (SessionState.GetBool(SessionKey, false)) return;

            SessionState.SetBool(SessionKey, true);
            EditorApplication.delayCall += OpenGameplayScene;
        }

        private static void OpenGameplayScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (SceneManager.GetActiveScene().path == GameplayScenePath) return;

            // Keep recovered or manually edited scenes intact.
            for (var i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) return;

            EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
        }
    }
}
