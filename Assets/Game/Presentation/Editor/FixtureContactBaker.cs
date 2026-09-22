using System;
using Game.Content;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Presentation.Editor
{
    public static class FixtureContactBaker
    {
        public static void BakePlayer()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Contact bake requires Edit mode.");
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Single);
            var player = GameObject.Find("Player");
            var oldBox = player.GetComponent<BoxCollider2D>();
            if (oldBox != null) UnityEngine.Object.DestroyImmediate(oldBox);
            var circle = player.GetComponent<CircleCollider2D>();
            if (circle == null) circle = player.AddComponent<CircleCollider2D>();
            var sprite = FixtureSpriteCatalog.CreateFor(new ContentId[] { "FIXTURE-CHARACTER-AGILE-VISUAL-BODY" })[0];
            sprite.Contact.Apply(circle);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Contact bake save failed.");
        }
    }
}
