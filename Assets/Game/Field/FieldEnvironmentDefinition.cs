using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Field
{
    /// <summary>Scene binding for fixture geometry; names identify existing player-only colliders, not production art.</summary>
    public sealed class FieldEnvironmentDefinition : IContentDefinition
    {
        public ContentId Id { get; }
        public string SceneName { get; }
        public string SpawnPointName { get; }
        public IReadOnlyList<string> ObstacleNames { get; }

        public FieldEnvironmentDefinition(ContentId id, string sceneName, string spawnPointName, IEnumerable<string> obstacleNames)
        {
            if (!id.IsValid || string.IsNullOrWhiteSpace(sceneName) || string.IsNullOrWhiteSpace(spawnPointName))
                throw new ArgumentException("Environment requires id, scene and spawn point.");
            var names = new List<string>(obstacleNames ?? throw new ArgumentNullException(nameof(obstacleNames)));
            if (names.Count == 0 || names.Exists(string.IsNullOrWhiteSpace) || new HashSet<string>(names).Count != names.Count)
                throw new ArgumentException("Environment requires unique obstacle names.", nameof(obstacleNames));
            Id = id;
            SceneName = sceneName;
            SpawnPointName = spawnPointName;
            ObstacleNames = names.AsReadOnly();
        }
    }
}
