using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>
    /// IP-15: irreversible phase at health/maxHealth &lt;= threshold; descending fractions, initial = 1.
    /// Each sequence step is a carrier: either a projectile attack or, with a matching <see cref="Specials"/> entry,
    /// a zone/beam/summon (DECISION-0066). <see cref="MovementOverride"/> replaces the body movement while this phase
    /// is active (dash series by phase, DECISION-0066 E3); null keeps the body movement.
    /// </summary>
    public sealed class BossPhaseDefinition
    {
        public ContentId Id { get; }
        public float HealthThreshold { get; }
        public IReadOnlyList<EnemyDefinition> Attacks { get; }
        /// <summary>Aligned with <see cref="Attacks"/>: the special of each step, null for projectile steps.</summary>
        public IReadOnlyList<BossSpecialAttack> Specials { get; }
        public EnemyMovementProfile MovementOverride { get; }

        public BossPhaseDefinition(ContentId id, float healthThreshold, IEnumerable<EnemyDefinition> attacks,
            IEnumerable<BossSpecialAttack> specials = null, EnemyMovementProfile movementOverride = null)
        {
            if (!id.IsValid) throw new ArgumentException("Phase id is required.", nameof(id));
            NumericValidation.ValidatePositive(healthThreshold, nameof(healthThreshold));
            NumericValidation.ValidateRange(healthThreshold, 0, 1, nameof(healthThreshold));
            var copy = new List<EnemyDefinition>(attacks ?? throw new ArgumentNullException(nameof(attacks)));
            var special = specials == null ? new List<BossSpecialAttack>(new BossSpecialAttack[copy.Count]) : new List<BossSpecialAttack>(specials);
            if (special.Count != copy.Count) throw new ArgumentException("Specials must align with the attack sequence.", nameof(specials));
            // An empty sequence is a boss without ranged attacks (MIDBOSS-001 dashes only).
            for (var i = 0; i < copy.Count; i++)
            {
                var attack = copy[i];
                if (attack == null) throw new ArgumentException("Sequence steps cannot be null.", nameof(attacks));
                if (special[i] != null ? attack.Attack != null : attack.Attack == null || attack.Attack.TelegraphSeconds <= 0)
                    throw new ArgumentException(
                        "Every boss step is either a projectile attack with a positive telegraph or a special without projectiles.",
                        nameof(attacks));
            }
            Id = id;
            HealthThreshold = healthThreshold;
            Attacks = copy.AsReadOnly();
            Specials = special.AsReadOnly();
            MovementOverride = movementOverride;
        }
    }
}
