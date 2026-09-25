using Game.Enemy;
using UnityEngine;
namespace Game.Traveler
{
    public sealed class TravelerLife
    {
        public EnemyRuntime Actor { get; }
        public TravelerDefinition Definition { get; }
        public float SpawnTime { get; }
        public float Deadline { get; }
        public float Scale { get; }
        public int Sequence { get; }
        public float NextSupportTime { get; set; }
        public Vector2 LastReachablePosition { get; set; }
        public TravelerLife(EnemyRuntime actor, TravelerDefinition definition, float spawnTime, float scale, int sequence)
        { Actor = actor; Definition = definition; SpawnTime = spawnTime; Deadline = spawnTime + definition.PresenceSeconds; Scale = scale; Sequence = sequence; NextSupportTime = spawnTime; LastReachablePosition = actor.Position; }
    }
}
