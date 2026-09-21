using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Field.Editor
{
    /// <summary>Bakes JSON dimensions into the existing fixture scene using Unity serialization.</summary>
    public static class FixtureArenaGeometryBaker
    {
        [MenuItem("Game/Fixtures/Bake Arena Geometry")]
        public static void Bake()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Bake requires Edit mode.");
            var config = FixtureArenaGeometryCatalog.Create();
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Single);
            var objects = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>()).ToArray();
            var camera = objects.Single(item => item.name == "Main Camera").GetComponent<Camera>();
            if (!camera.orthographic || !Mathf.Approximately(camera.orthographicSize * 2, config.ReferenceScreenHeight))
                throw new InvalidOperationException("Arena reference screen height must match the gameplay camera.");
            var center = (config.SideLength + config.WallThickness) * .5f;
            var span = config.SideLength + config.WallThickness;
            SetWall("Wall_Top", new Vector2(0, center), new Vector2(span, config.WallThickness));
            SetWall("Wall_Bottom", new Vector2(0, -center), new Vector2(span, config.WallThickness));
            SetWall("Wall_Left", new Vector2(-center, 0), new Vector2(config.WallThickness, span));
            SetWall("Wall_Right", new Vector2(center, 0), new Vector2(config.WallThickness, span));
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Arena save failed.");
            Debug.Log($"Fixture arena baked: {config.SideLength} x {config.SideLength} world units.");

            void SetWall(string name, Vector2 position, Vector2 size)
            {
                var wall = objects.Single(item => item.name == name);
                var box = wall.GetComponent<BoxCollider2D>();
                if (box == null) throw new InvalidOperationException($"Missing wall collider: {name}");
                Undo.RecordObjects(new UnityEngine.Object[] { wall, box }, "Bake fixture arena");
                wall.position = new Vector3(position.x, position.y, wall.position.z);
                box.size = size;
            }
        }
    }
}
