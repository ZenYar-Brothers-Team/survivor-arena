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
        /// <summary>Movement driver of non-combat Travelers (null for combat Travelers); teleport requests are read from it.</summary>
        public TravelerMovementDriver Driver { get; set; }
        /// <summary>Steady aura ring shown around an Aura protector (returned to the pool with the Traveler).</summary>
        public TravelerPulseEffect AuraEffect { get; set; }
        public TravelerLife(EnemyRuntime actor, TravelerDefinition definition, float spawnTime, float scale, int sequence)
        { Actor = actor; Definition = definition; SpawnTime = spawnTime; Deadline = spawnTime + definition.PresenceSeconds; Scale = scale; Sequence = sequence; NextSupportTime = spawnTime; }
    }
}
