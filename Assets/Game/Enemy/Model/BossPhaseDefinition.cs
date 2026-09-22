using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>IP-15: irreversible phase at health/maxHealth &lt;= threshold; descending fractions, initial = 1.</summary>
    public sealed class BossPhaseDefinition
    {
        public ContentId Id { get; }
        public float HealthThreshold { get; }
        public IReadOnlyList<EnemyDefinition> Attacks { get; }

        public BossPhaseDefinition(ContentId id, float healthThreshold, IEnumerable<EnemyDefinition> attacks)
        {
            if (!id.IsValid) throw new ArgumentException("Phase id is required.", nameof(id));
            NumericValidation.ValidatePositive(healthThreshold, nameof(healthThreshold));
            NumericValidation.ValidateRange(healthThreshold, 0, 1, nameof(healthThreshold));
            var copy = new List<EnemyDefinition>(attacks ?? throw new ArgumentNullException(nameof(attacks)));
            if (copy.Count == 0) throw new ArgumentException("A phase needs an attack sequence.", nameof(attacks));
            foreach (var attack in copy)
                if (attack?.Attack == null || attack.Attack.TelegraphSeconds <= 0)
                    throw new ArgumentException("Every boss attack needs a projectile profile and positive telegraph.", nameof(attacks));
            Id = id;
            HealthThreshold = healthThreshold;
            Attacks = copy.AsReadOnly();
        }
    }
}
