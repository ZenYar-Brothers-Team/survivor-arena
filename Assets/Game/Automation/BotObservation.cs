using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Automation
{
    /// <summary>One read-only, main-thread snapshot of what the bot can currently observe.</summary>
    public sealed class BotObservation
    {
        public Vector2 Position { get; }
        public float MovementSpeed { get; }
        public float PlayerRadius { get; }
        public Rect ArenaBounds { get; }
        public IReadOnlyList<BotThreat> Threats { get; }
        public IReadOnlyList<BotPickup> Pickups { get; }
        public IReadOnlyList<BotObstacle> Obstacles { get; }
        public IReadOnlyList<BotBeam> Beams { get; }
        public bool CoverageComplete { get; }
        public float HealthFraction { get; }

        public BotObservation(Vector2 position, float movementSpeed, float playerRadius, Rect arenaBounds,
            IReadOnlyList<BotThreat> threats, IReadOnlyList<BotPickup> pickups, IReadOnlyList<BotObstacle> obstacles,
            IReadOnlyList<BotBeam> beams,
            bool coverageComplete, float healthFraction = 1f)
        {
            Position = position;
            MovementSpeed = movementSpeed;
            PlayerRadius = playerRadius;
            ArenaBounds = arenaBounds;
            Threats = threats ?? throw new ArgumentNullException(nameof(threats));
            Pickups = pickups ?? throw new ArgumentNullException(nameof(pickups));
            Obstacles = obstacles ?? throw new ArgumentNullException(nameof(obstacles));
            Beams = beams ?? throw new ArgumentNullException(nameof(beams));
            CoverageComplete = coverageComplete;
            HealthFraction = Mathf.Clamp01(healthFraction);
        }
    }
}
