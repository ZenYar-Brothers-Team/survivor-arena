using System.Collections.Generic;
using Game.Content;
using Game.Diagnostics;
using Game.Enemy;

namespace Game.ActiveSkill
{
    /// <summary>One ledger per owner life/skill, shared across casts. Time advances only while the run is running.</summary>
    public sealed class SkillHitLedger
    {
        private readonly Dictionary<EnemyTargetLife, double> _until = new Dictionary<EnemyTargetLife, double>();
        private readonly List<EnemyTargetLife> _expired = new List<EnemyTargetLife>();
        public double Elapsed { get; private set; }
        public int Count => _until.Count;

        public void Tick(float deltaTime, bool isRunning)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            if (!isRunning) return;
            Elapsed += deltaTime;
            using var guard = PerfGuard.Measure("SkillHitLedger.Prune", 1f);
            _expired.Clear();
            foreach (var entry in _until)
                if (entry.Value <= Elapsed || !entry.Key.IsAlive) _expired.Add(entry.Key);
            foreach (var key in _expired) _until.Remove(key);
        }

        public bool TryHit(EnemyTargetLife target, float cooldownSeconds)
        {
            NumericValidation.ValidatePositive(cooldownSeconds, nameof(cooldownSeconds));
            if (!target.IsAlive || (_until.TryGetValue(target, out var until) && Elapsed < until)) return false;
            _until[target] = Elapsed + cooldownSeconds;
            return true;
        }

        public void Clear() { _until.Clear(); _expired.Clear(); Elapsed = 0d; }
    }
}
