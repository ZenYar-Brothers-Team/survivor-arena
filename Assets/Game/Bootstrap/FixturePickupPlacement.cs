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
        /// <summary>Inner rectangle between the four boundary walls (world pickups clamp into it, DECISION-0075).</summary>
        public static Rect ArenaBounds(FieldEnvironmentDefinition environment, Scene scene)
        {
            FieldEnvironmentBinding.Validate(environment, scene);
            Physics2D.SyncTransforms();
            var walls = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<BoxCollider2D>())
                .Where(box => box.name.StartsWith("Wall_", StringComparison.Ordinal))
                .ToDictionary(box => box.name, box => box.bounds);
            return Rect.MinMaxRect(walls["Wall_Left"].max.x, walls["Wall_Bottom"].max.y,
                walls["Wall_Right"].min.x, walls["Wall_Top"].min.y);
        }

        public static IPickupPlacement Create(FieldEnvironmentDefinition environment, Scene scene, Collider2D player,
            float skin, float minimumHalfSize = 0, IEnumerable<Collider2D> additionalObstacles = null)
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
                Rect.MinMaxRect(pair.Value.min.x, pair.Value.min.y, pair.Value.max.x, pair.Value.max.y)).ToList();
            if (additionalObstacles != null)
            {
                foreach (var collider in additionalObstacles)
                {
                    if (collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy)
                        throw new InvalidOperationException("Additional fixture obstacles must be active colliders.");
                    var bounds = collider.bounds;
                    obstacles.Add(Rect.MinMaxRect(bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y));
                }
            }
            return new BoxPickupPlacement(arena, obstacles, Vector2.Max(player.bounds.extents, Vector2.one * minimumHalfSize), spawn.position, skin);
        }
    }
}
