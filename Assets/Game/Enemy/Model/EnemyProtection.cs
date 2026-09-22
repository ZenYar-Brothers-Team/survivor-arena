using System;
using System.Collections.Generic;
using Game.Content;
namespace Game.Enemy
{
    /// <summary>DECISION-0035: max-per-channel auras and one source-owned shield; clock supplied by the run.</summary>
    public sealed class EnemyProtection
    {
        private readonly Dictionary<Guid, (float reduction, float resistance, float deadline)> _auras = new Dictionary<Guid, (float, float, float)>();
        private readonly List<Guid> _expired = new List<Guid>();
        public Guid ShieldSource { get; private set; }
        public float ShieldCapacity { get; private set; }
        public float ShieldRemaining { get; private set; }
        public float ShieldDeadline { get; private set; }
        public float Reduction { get { float value = 0; foreach (var aura in _auras.Values) value = Math.Max(value, aura.reduction); return value; } }
        public float ResistanceBonus { get { float value = 0; foreach (var aura in _auras.Values) value = Math.Max(value, aura.resistance); return value; } }
        public void SetAura(Guid source, float reduction, float resistance, float deadline = float.PositiveInfinity)
        {
            if (source == Guid.Empty) throw new ArgumentException("Aura source required.");
            NumericValidation.ValidateRange(reduction, 0, 1, nameof(reduction));
            if (reduction == 1) throw new ArgumentOutOfRangeException(nameof(reduction));
            NumericValidation.ValidateRange(resistance, 0, 1, nameof(resistance));
            _auras[source] = (reduction, resistance, deadline);
        }
        public bool CastShield(Guid source, float capacity, float duration, float now)
        {
            if (source == Guid.Empty) throw new ArgumentException("Shield source required.");
            NumericValidation.ValidatePositive(capacity, nameof(capacity));
            NumericValidation.ValidatePositive(duration, nameof(duration));
            NumericValidation.ValidateNonNegative(now, nameof(now));
            Tick(now);
            if (ShieldRemaining > 0 && capacity < ShieldCapacity) return false;
            ShieldSource = source; ShieldCapacity = ShieldRemaining = capacity; ShieldDeadline = now + duration;
            return true;
        }
        public void Tick(float now)
        {
            if (now >= ShieldDeadline) ClearShield();
            _expired.Clear();
            foreach (var aura in _auras) if (now >= aura.Value.deadline) _expired.Add(aura.Key);
            foreach (var source in _expired) _auras.Remove(source);
        }
        public float Absorb(float amount, float now)
        {
            NumericValidation.ValidateNonNegative(amount, nameof(amount));
            Tick(now);
            var reduced = amount * (1 - Reduction);
            var absorbed = Math.Min(reduced, ShieldRemaining);
            ShieldRemaining -= absorbed;
            if (ShieldRemaining <= 0) ClearShield();
            return reduced - absorbed;
        }
        public void RemoveAura(Guid source) => _auras.Remove(source);
        public void RemoveSource(Guid source) { RemoveAura(source); if (ShieldSource == source) ClearShield(); }
        public void Reset() { _auras.Clear(); ClearShield(); }
        private void ClearShield() { ShieldSource = Guid.Empty; ShieldCapacity = ShieldRemaining = ShieldDeadline = 0; }
    }
}
