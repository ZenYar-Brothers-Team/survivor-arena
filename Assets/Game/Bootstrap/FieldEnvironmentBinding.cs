using System;
using System.Collections.Generic;
using System.Linq;
using Game.Field;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Game.Bootstrap
{
    /// <summary>Read-only adapter for the existing fixture arena. Validate before initializing the run (DECISION-0003).</summary>
    public static class FieldEnvironmentBinding
    {
        public static Transform Validate(FieldEnvironmentDefinition environment, Scene scene)
        {
            if (scene.name != environment.SceneName) throw new InvalidOperationException("Field environment requires a different scene.");
            var objects = new List<Transform>();
            foreach (var root in scene.GetRootGameObjects()) objects.AddRange(root.GetComponentsInChildren<Transform>(true));
            var spawn = RequireUnique(objects, environment.SpawnPointName);
            var playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer < 0) throw new InvalidOperationException("Player layer is missing.");
            foreach (var name in environment.ObstacleNames)
            {
                var obstacle = RequireUnique(objects, name);
                var collider = obstacle.GetComponent<Collider2D>();
                if (collider == null || !collider.enabled || !obstacle.gameObject.activeInHierarchy || collider.isTrigger ||
                    collider.excludeLayers.value != ~(1 << playerLayer))
                    throw new InvalidOperationException($"Field obstacle '{name}' requires active player-only collision.");
            }
            return spawn;
        }
        private static Transform RequireUnique(List<Transform> objects, string name)
        {
            var matches = objects.Where(value => value.name == name).ToList();
            if (matches.Count != 1) throw new InvalidOperationException($"Environment binding '{name}' must occur exactly once.");
            return matches[0];
        }
    }
}
