using System;
using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Content.Json;
using Game.Enemy.Json;

namespace Game.Enemy
{
    public static class FixtureBossCatalog
    {
        public static IReadOnlyList<BossEncounterDefinition> Create(IReadOnlyList<EnemyDefinition> enemies)
            => Load("Content/Bosses/FixtureBosses", enemies);

        public static IReadOnlyList<BossEncounterDefinition> Load(string resourcePath, IReadOnlyList<EnemyDefinition> enemies)
            => FromData(JsonContentFile.Load<BossEncounterData[]>(resourcePath), enemies);

        public static IReadOnlyList<BossEncounterDefinition> FromData(BossEncounterData[] data, IReadOnlyList<EnemyDefinition> enemies)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var byId = enemies.ToDictionary(e => e.Id);
            var definitions = new List<BossEncounterDefinition>();
            var hooks = new HashSet<WaveHookKind>();
            foreach (var entry in data)
            {
                if (entry == null || !Enum.TryParse(entry.Hook, out WaveHookKind hook) || !hooks.Add(hook))
                    throw new ArgumentException("Boss hook is invalid or duplicated.", nameof(data));
                if (entry.SpawnOffsetX == null || entry.SpawnOffsetY == null || entry.Phases == null)
                    throw new ArgumentException("Boss spawn offsets and phases must be explicit.", nameof(data));
                if (entry.Body?.Id != entry.Id) throw new ArgumentException("Body identity must match encounter identity.", nameof(data));
                var owned = new List<EnemyDefinition>();
                var available = new Dictionary<ContentId, EnemyDefinition>(byId);
                foreach (var inline in entry.Attacks ?? Array.Empty<BossAttackData>())
                {
                    if (inline?.Attack == null || string.IsNullOrWhiteSpace(inline.Id))
                        throw new ArgumentException("Inline boss attacks need an id and an attack.", nameof(data));
                    var carrier = FixtureEnemyCatalog.ToDefinition(new EnemyDefinitionData
                    {
                        Id = inline.Id, KnockbackResistance = entry.Body.KnockbackResistance, ContactControls = entry.Body.ContactControls,
                        MaxHealth = entry.Body.MaxHealth, CollisionSize = entry.Body.CollisionSize, MovementSpeed = entry.Body.MovementSpeed,
                        ContactDamage = entry.Body.ContactDamage, ContactDamageInterval = entry.Body.ContactDamageInterval,
                        ExperienceReward = 0, Attack = inline.Attack
                    });
                    if (available.ContainsKey(carrier.Id)) throw new ArgumentException($"Duplicate boss attack '{inline.Id}'.", nameof(data));
                    available.Add(carrier.Id, carrier);
                    owned.Add(carrier);
                }
                var phases = new List<BossPhaseDefinition>();
                foreach (var phase in entry.Phases)
                {
                    if (phase?.HealthThreshold == null || phase.AttackEnemyIds == null)
                        throw new ArgumentException("Phase threshold and attacks must be explicit.", nameof(data));
                    var attacks = new List<EnemyDefinition>();
                    foreach (var id in phase.AttackEnemyIds)
                    {
                        if (!available.TryGetValue(new ContentId(id), out var attack))
                            throw new ArgumentException($"Unknown boss attack enemy '{id}'.", nameof(data));
                        attacks.Add(attack);
                    }
                    phases.Add(new BossPhaseDefinition(phase.Id, phase.HealthThreshold.Value, attacks));
                }
                definitions.Add(new BossEncounterDefinition(entry.Id, entry.DisplayName, hook,
                    FixtureEnemyCatalog.ToDefinition(entry.Body), entry.SpawnOffsetX.Value, entry.SpawnOffsetY.Value, phases,
                    entry.KeepAttackOrderOnPhaseChange ?? false, entry.StrictHealthThreshold ?? false, owned));
            }
            if (!hooks.Contains(WaveHookKind.FinalBoss)) throw new ArgumentException("Final boss definition is required.", nameof(data));
            return definitions.AsReadOnly();
        }
    }
}
