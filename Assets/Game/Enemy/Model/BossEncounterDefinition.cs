using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>Fixture encounter content. Spawn offset is world units relative to player, independent of regular cap.</summary>
    public sealed class BossEncounterDefinition : IContentDefinition, IReferencesContent
    {
        public ContentId Id { get; }
        public string DisplayName { get; }
        public WaveHookKind Hook { get; }
        public EnemyDefinition Body { get; }
        public float SpawnOffsetX { get; }
        public float SpawnOffsetY { get; }
        public IReadOnlyList<BossPhaseDefinition> Phases { get; }
        /// <summary>Phase changes keep the running wind-up and the attack order; only later intervals change (BOSS-001).</summary>
        public bool KeepAttackOrderOnPhaseChange { get; }
        /// <summary>Phase threshold compares strictly below (health &lt; threshold) instead of at-or-below.</summary>
        public bool StrictHealthThreshold { get; }
        /// <summary>Attack carriers authored inline with this encounter; register them with the encounter.</summary>
        public IReadOnlyList<EnemyDefinition> OwnedAttacks { get; }
        /// <summary>Teleport-slam against a player who keeps away from the boss; null when the boss has none (DECISION-0059).</summary>
        public BossTeleportProfile Teleport { get; }

        public BossEncounterDefinition(ContentId id, string displayName, WaveHookKind hook,
            EnemyDefinition body, float spawnOffsetX, float spawnOffsetY, IEnumerable<BossPhaseDefinition> phases,
            bool keepAttackOrderOnPhaseChange = false, bool strictHealthThreshold = false, IEnumerable<EnemyDefinition> ownedAttacks = null,
            BossTeleportProfile teleport = null)
        {
            if (!id.IsValid) throw new ArgumentException("Encounter id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Boss name is required.", nameof(displayName));
            if (!Enum.IsDefined(typeof(WaveHookKind), hook)) throw new ArgumentOutOfRangeException(nameof(hook));
            NumericValidation.ValidateFinite(spawnOffsetX, nameof(spawnOffsetX));
            NumericValidation.ValidateFinite(spawnOffsetY, nameof(spawnOffsetY));
            var copy = new List<BossPhaseDefinition>(phases ?? throw new ArgumentNullException(nameof(phases)));
            if (copy.Count == 0 || copy[0] == null || copy[0].HealthThreshold != 1f)
                throw new ArgumentException("Initial phase must have threshold 1.", nameof(phases));
            var ids = new HashSet<ContentId>();
            for (var i = 0; i < copy.Count; i++)
                if (copy[i] == null || !ids.Add(copy[i].Id) || (i > 0 && copy[i].HealthThreshold >= copy[i - 1].HealthThreshold))
                    throw new ArgumentException("Phase ids must be unique and thresholds strictly descending.", nameof(phases));
            Id = id;
            DisplayName = displayName;
            Hook = hook;
            Body = body ?? throw new ArgumentNullException(nameof(body));
            SpawnOffsetX = spawnOffsetX;
            SpawnOffsetY = spawnOffsetY;
            Phases = copy.AsReadOnly();
            KeepAttackOrderOnPhaseChange = keepAttackOrderOnPhaseChange;
            StrictHealthThreshold = strictHealthThreshold;
            OwnedAttacks = new List<EnemyDefinition>(ownedAttacks ?? Array.Empty<EnemyDefinition>()).AsReadOnly();
            Teleport = teleport;
        }

        public IEnumerable<ContentReference> GetReferencedContent()
        {
            foreach (var reference in Body.GetReferencedContent()) yield return reference;
            foreach (var phase in Phases)
                foreach (var attack in phase.Attacks)
                    yield return new ContentRef<EnemyDefinition>(attack.Id).ToReference();
        }
    }
}
