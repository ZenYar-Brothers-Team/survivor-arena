using System;
using Game.Content;
using UnityEngine;
namespace Game.Traveler
{
    public readonly struct TravelerSnapshot
    {
        public Guid LifeId { get; }
        public Guid RunId { get; }
        public ContentId Id { get; }
        public string Name { get; }
        public string Marker { get; }
        public TravelerRole Role { get; }
        public Vector2 Position { get; }
        public float Health { get; }
        public float MaxHealth { get; }
        public float SpawnTime { get; }
        public float Deadline { get; }
        public float Scale { get; }
        public int Sequence { get; }
        public TravelerSnapshot(Guid lifeId, Guid runId, ContentId id, string name, string marker, TravelerRole role,
            Vector2 position, float health, float maxHealth, float spawnTime, float deadline, float scale, int sequence)
        {
            LifeId = lifeId; RunId = runId; Id = id; Name = name; Marker = marker; Role = role;
            Position = position; Health = health; MaxHealth = maxHealth; SpawnTime = spawnTime;
            Deadline = deadline; Scale = scale; Sequence = sequence;
        }
        public TravelerSnapshot(TravelerLife life, Guid runId)
        {
            LifeId = life.Actor.LifeId; RunId = runId; Id = life.Definition.Id; Name = life.Definition.Name;
            Marker = life.Definition.Marker; Role = life.Definition.Role; Position = life.Actor.Position;
            Health = life.Actor.Health.CurrentHealth; MaxHealth = life.Actor.Health.MaxHealth;
            SpawnTime = life.SpawnTime; Deadline = life.Deadline; Scale = life.Scale; Sequence = life.Sequence;
        }
    }
}
