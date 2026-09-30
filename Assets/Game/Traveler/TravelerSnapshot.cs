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
        /// <summary>World point just above the body top; the on-screen HP bar sits here (DECISION-0109).</summary>
        public Vector2 HeadPosition { get; }
        public float Health { get; }
        public float MaxHealth { get; }
        public float SpawnTime { get; }
        public float Deadline { get; }
        public float Scale { get; }
        public int Sequence { get; }
        public TravelerSnapshot(Guid lifeId, Guid runId, ContentId id, string name, string marker, TravelerRole role,
            Vector2 position, float health, float maxHealth, float spawnTime, float deadline, float scale, int sequence,
            Vector2? headPosition = null)
        {
            LifeId = lifeId; RunId = runId; Id = id; Name = name; Marker = marker; Role = role;
            Position = position; HeadPosition = headPosition ?? position; Health = health; MaxHealth = maxHealth; SpawnTime = spawnTime;
            Deadline = deadline; Scale = scale; Sequence = sequence;
        }
        public TravelerSnapshot(TravelerLife life, Guid runId)
        {
            LifeId = life.Actor.LifeId; RunId = runId; Id = life.Definition.Id; Name = life.Definition.Name;
            Marker = life.Definition.Marker; Role = life.Definition.Role; Position = life.Actor.Position;
            var body = life.Actor.BodyPresentation?.Rig.BodyRenderer;
            HeadPosition = body != null && body.enabled ? new Vector2(body.bounds.center.x, body.bounds.max.y) : Position + Vector2.up;
            Health = life.Actor.Health.CurrentHealth; MaxHealth = life.Actor.Health.MaxHealth;
            SpawnTime = life.SpawnTime; Deadline = life.Deadline; Scale = life.Scale; Sequence = life.Sequence;
        }
    }
}
