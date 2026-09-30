using System;
using System.Collections.Generic;
using Game.Combat;
using Game.Enemy;
using Game.Run;
using Game.Traveler;

namespace Game.Bootstrap
{
    /// <summary>Collects confirmed combat facts for one run; the result snapshot is saved by the profile.</summary>
    public sealed class RunAchievementTracker : IRunOutcomeContributor, IDisposable
    {
        private readonly RunModel _run;
        private readonly ContinuousFixtureEnemySpawner _ordinary;
        private readonly BossEncounterRuntime _bosses;
        private readonly TravelerEncounterRuntime _travelers;
        private readonly Dictionary<string, int> _kills = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<string, double> _damage = new Dictionary<string, double>(StringComparer.Ordinal);
        private int _ordinaryKills;
        private double _activeSkillDamage;
        private double _characterDamage;
        private bool _disposed;

        public string Key => "achievements";

        public RunAchievementTracker(RunModel run, ContinuousFixtureEnemySpawner ordinary,
            BossEncounterRuntime bosses, TravelerEncounterRuntime travelers)
        {
            _run = run ?? throw new ArgumentNullException(nameof(run));
            _ordinary = ordinary ?? throw new ArgumentNullException(nameof(ordinary));
            _bosses = bosses ?? throw new ArgumentNullException(nameof(bosses));
            _travelers = travelers;
            _ordinary.LifeEvent += OnLife;
            _ordinary.CombatResolved += OnCombat;
            _bosses.LifeEvent += OnLife;
            _bosses.CombatResolved += OnCombat;
            if (_travelers != null) _travelers.CombatResolved += OnCombat;
            _run.RegisterOutcomeContributor(this);
        }

        private void OnLife(EnemyLifeEvent life)
        {
            if (life == null || life.RunId != _run.RunId || _run.State != RunState.Running ||
                life.Kind != EnemyLifeEventKind.Died || life.Reason != EnemyLifeReason.Killed) return;
            if (life.Category == EnemyCategory.Ordinary) _ordinaryKills = checked(_ordinaryKills + 1);
            var id = life.ContentId.ToString();
            _kills.TryGetValue(id, out var count);
            _kills[id] = checked(count + 1);
        }

        private void OnCombat(CombatResult result)
        {
            if (_run.State != RunState.Running || result.Target.RunId != _run.RunId ||
                result.Health.IsHealing || result.Health.Actual <= 0f) return;
            var source = result.Source.ContentId?.ToString();
            if (source == null || result.Source.Owner.Category != CombatEntityCategory.Player) return;
            _characterDamage += result.Health.Actual;
            if (!source.StartsWith("SKILL-", StringComparison.Ordinal)) return;
            _activeSkillDamage += result.Health.Actual;
            _damage.TryGetValue(source, out var total);
            _damage[source] = total + result.Health.Actual;
        }

        public RunOutcomeContribution Capture()
        {
            var bySource = new Dictionary<string, long>(StringComparer.Ordinal);
            foreach (var pair in _damage)
                bySource.Add(pair.Key, checked((long)Math.Floor(pair.Value)));
            return new RunOutcomeContribution(achievements: new RunAchievementSnapshot(_ordinaryKills, _kills,
                bySource, checked((long)Math.Floor(_activeSkillDamage)), checked((long)Math.Floor(_characterDamage))));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _run.UnregisterOutcomeContributor(this);
            _ordinary.LifeEvent -= OnLife;
            _ordinary.CombatResolved -= OnCombat;
            _bosses.LifeEvent -= OnLife;
            _bosses.CombatResolved -= OnCombat;
            if (_travelers != null) _travelers.CombatResolved -= OnCombat;
        }
    }
}
