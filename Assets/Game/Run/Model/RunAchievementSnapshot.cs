using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Game.Run
{
    /// <summary>Immutable, per-run combat facts used for persistent unlock progress.</summary>
    public sealed class RunAchievementSnapshot
    {
        public int OrdinaryKills { get; }
        public IReadOnlyDictionary<string, int> KillsById { get; }
        public IReadOnlyDictionary<string, long> DamageBySource { get; }
        public long ActiveSkillDamage { get; }
        public long CharacterDamage { get; }

        public RunAchievementSnapshot(int ordinaryKills, IDictionary<string, int> killsById,
            IDictionary<string, long> damageBySource, long activeSkillDamage, long characterDamage)
        {
            if (ordinaryKills < 0 || activeSkillDamage < 0 || characterDamage < 0)
                throw new ArgumentOutOfRangeException(nameof(ordinaryKills));
            OrdinaryKills = ordinaryKills;
            KillsById = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(killsById ?? throw new ArgumentNullException(nameof(killsById)), StringComparer.Ordinal));
            DamageBySource = new ReadOnlyDictionary<string, long>(new Dictionary<string, long>(damageBySource ?? throw new ArgumentNullException(nameof(damageBySource)), StringComparer.Ordinal));
            ActiveSkillDamage = activeSkillDamage;
            CharacterDamage = characterDamage;
        }
    }
}
