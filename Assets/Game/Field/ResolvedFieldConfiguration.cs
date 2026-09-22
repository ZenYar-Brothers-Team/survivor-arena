using System.Collections.Generic;
using Game.Enemy;

namespace Game.Field
{
    /// <summary>Validated, immutable configuration consumed by one composition attempt.</summary>
    public sealed class ResolvedFieldConfiguration
    {
        public FieldDefinition Field { get; }
        public FieldEnvironmentDefinition Environment { get; }
        public WaveTimelineDefinition Timeline { get; }
        public IReadOnlyList<EnemyDefinition> Enemies { get; }
        public IReadOnlyList<BossEncounterDefinition> Bosses { get; }
        public FieldTravelerScheduleDefinition Travelers { get; }
        internal ResolvedFieldConfiguration(FieldDefinition field, FieldEnvironmentDefinition environment,
            WaveTimelineDefinition timeline, List<EnemyDefinition> enemies, List<BossEncounterDefinition> bosses,
            FieldTravelerScheduleDefinition travelers)
        {
            Field = field; Environment = environment; Timeline = timeline;
            Enemies = enemies.AsReadOnly(); Bosses = bosses.AsReadOnly(); Travelers = travelers;
        }
    }
}
