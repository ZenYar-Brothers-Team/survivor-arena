using System;
using System.Collections.Generic;
using System.Linq;
using Game.Field;
using Game.Pickup;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Game.Bootstrap
{
    /// <summary>Adapter for the IP-16 axis-aligned fixture arena; production geometry supplies its own IPickupPlacement.</summary>
    public static class FixturePickupPlacement
    {
        public static IPickupPlacement Create(FieldEnvironmentDefinition environment, Scene scene, Collider2D player, float skin, float minimumHalfSize = 0)
        {
            var spawn = FieldEnvironmentBinding.Validate(environment, scene);
            Physics2D.SyncTransforms();
            var transforms = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>()).ToArray();
            var boxes = new Dictionary<string, Bounds>();
            foreach (var name in environment.ObstacleNames)
            {
                var item = transforms.Single(value => value.name == name);
                var box = item.GetComponent<BoxCollider2D>();
                if (box == null || Mathf.Abs(Mathf.DeltaAngle(item.eulerAngles.z, 0)) > .001f)
                    throw new InvalidOperationException("Fixture placement supports axis-aligned boxes only.");
                boxes.Add(name, box.bounds);
            }
            var arena = Rect.MinMaxRect(boxes["Wall_Left"].max.x, boxes["Wall_Bottom"].max.y,
                boxes["Wall_Right"].min.x, boxes["Wall_Top"].min.y);
            var boundaryNames = new[] { "Wall_Left", "Wall_Right", "Wall_Top", "Wall_Bottom" };
            var obstacles = boxes.Where(pair => !boundaryNames.Contains(pair.Key)).Select(pair =>
                Rect.MinMaxRect(pair.Value.min.x, pair.Value.min.y, pair.Value.max.x, pair.Value.max.y));
            return new BoxPickupPlacement(arena, obstacles, Vector2.Max(player.bounds.extents, Vector2.one * minimumHalfSize), spawn.position, skin);
        }
    }
}
