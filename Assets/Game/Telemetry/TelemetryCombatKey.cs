using System;
using Game.Combat;
using Game.Content;

namespace Game.Telemetry
{
    /// <summary>Bounded aggregation key; no per-hit strings or retained pooled objects.</summary>
    public readonly struct TelemetryCombatKey : IEquatable<TelemetryCombatKey>
    {
        public ContentId? Source { get; }
        public int? Level { get; }
        public CombatSourceOrigin Origin { get; }
        public CombatEntityCategory Target { get; }
        public bool Healing { get; }
        public TelemetryCombatKey(CombatResult result)
        {
            Source = result.Source.ContentId; Level = result.Source.SkillLevel;
            Origin = result.Source.Origin; Target = result.Target.Category; Healing = result.Health.IsHealing;
        }
        public bool Equals(TelemetryCombatKey other) => Source == other.Source && Level == other.Level &&
            Origin == other.Origin && Target == other.Target && Healing == other.Healing;
        public override bool Equals(object obj) => obj is TelemetryCombatKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Source, Level, Origin, Target, Healing);
    }
}
