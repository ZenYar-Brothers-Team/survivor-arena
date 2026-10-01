using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Combat
{
    /// <summary>DECISION-0017: additive knockback; strongest slow with independent source expiry.</summary>
    public sealed class CombatControlState
    {
        private readonly Dictionary<SlowSourceKey, SlowState> _slow = new Dictionary<SlowSourceKey, SlowState>();
        private readonly List<SlowSourceKey> _expired = new List<SlowSourceKey>();
        private readonly Dictionary<SlowSourceKey, SlowState> _vulnerable = new Dictionary<SlowSourceKey, SlowState>();
        private float _knockbackX, _knockbackY;
        public float KnockbackRemaining { get; private set; }
        public float MovementMultiplier { get; private set; } = 1f;
        public int SlowSourceCount => _slow.Count;
        public bool IsSlowed => MovementMultiplier < 1f;
        /// <summary>Incoming damage factor from the strongest active vulnerability mark (DECISION-0139); 1 = none.</summary>
        public float DamageTakenMultiplier { get; private set; } = 1f;

        /// <summary>
        /// Presentation-only (DECISION-0108): remaining fraction 0..1 of the active slow that lasts longest,
        /// so a status bar shrinks as the slow runs out. Zero when not slowed.
        /// </summary>
        public float SlowRemaining01
        {
            get
            {
                var remaining = 0f;
                var fraction = 0f;
                foreach (var slow in _slow.Values)
                    if (slow.Remaining > remaining && slow.Duration > 0f) { remaining = slow.Remaining; fraction = slow.Remaining / slow.Duration; }
                return Math.Min(1f, Math.Max(0f, fraction));
            }
        }

        public float Apply(CombatDamageRequest request, float resistance, bool acceptsSlow)
        {
            NumericValidation.ValidateRange(resistance, 0f, 1f, nameof(resistance));
            var profile = request.Controls ?? CombatControlProfile.None;
            var distance = (float)((double)profile.KnockbackDistance * request.OutgoingKnockbackMultiplier * (1d - resistance));
            NumericValidation.ValidateNonNegative(distance, nameof(distance));
            if (distance > 0f && (request.DirectionX != 0f || request.DirectionY != 0f))
            {
                var length = Math.Sqrt((double)request.DirectionX * request.DirectionX + (double)request.DirectionY * request.DirectionY);
                var speed = distance / profile.KnockbackSeconds;
                NumericValidation.ValidateFinite(speed, nameof(speed));
                _knockbackX = (float)(request.DirectionX / length * speed);
                _knockbackY = (float)(request.DirectionY / length * speed);
                KnockbackRemaining = profile.KnockbackSeconds;
            }
            else distance = 0f; // An immune/zero-direction hit does not cancel an earlier valid impulse.
            if (acceptsSlow && profile.SlowFraction > 0f)
            {
                var key = new SlowSourceKey(request);
                if (!_slow.TryGetValue(key, out var slow)) _slow.Add(key, slow = new SlowState());
                slow.Fraction = profile.SlowFraction;
                slow.Remaining = profile.SlowSeconds;
                slow.Duration = profile.SlowSeconds;
                RefreshSlow();
            }
            if (profile.DamageTakenBonus > 0f)
            {
                var key = new SlowSourceKey(request);
                if (!_vulnerable.TryGetValue(key, out var mark)) _vulnerable.Add(key, mark = new SlowState());
                mark.Fraction = profile.DamageTakenBonus;
                mark.Remaining = profile.DamageTakenSeconds;
                mark.Duration = profile.DamageTakenSeconds;
                RefreshVulnerability();
            }
            return distance;
        }

        public ControlMotion Tick(float deltaTime, bool isRunning)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            if (!isRunning || deltaTime == 0f) return new ControlMotion(0f, 0f, MovementMultiplier);
            if (KnockbackRemaining == 0f && _slow.Count == 0 && _vulnerable.Count == 0) return new ControlMotion(0f, 0f, 1f);
            var factor = Math.Min(deltaTime, KnockbackRemaining) / deltaTime;
            var motion = new ControlMotion(_knockbackX * factor, _knockbackY * factor, MovementMultiplier);
            KnockbackRemaining = Math.Max(0f, KnockbackRemaining - deltaTime);
            _expired.Clear();
            foreach (var entry in _slow)
            {
                entry.Value.Remaining -= deltaTime;
                if (entry.Value.Remaining <= 0f) _expired.Add(entry.Key);
            }
            foreach (var key in _expired) _slow.Remove(key);
            if (_expired.Count > 0) RefreshSlow();
            if (_vulnerable.Count > 0)
            {
                _expired.Clear();
                foreach (var entry in _vulnerable)
                {
                    entry.Value.Remaining -= deltaTime;
                    if (entry.Value.Remaining <= 0f) _expired.Add(entry.Key);
                }
                foreach (var key in _expired) _vulnerable.Remove(key);
                if (_expired.Count > 0) RefreshVulnerability();
            }
            return motion;
        }

        public void Reset()
        {
            _slow.Clear();
            _vulnerable.Clear();
            DamageTakenMultiplier = 1f;
            _expired.Clear();
            _knockbackX = _knockbackY = KnockbackRemaining = 0f;
            MovementMultiplier = 1f;
        }

        private void RefreshVulnerability()
        {
            var strongest = 0f;
            foreach (var mark in _vulnerable.Values) strongest = Math.Max(strongest, mark.Fraction);
            DamageTakenMultiplier = 1f + strongest;
        }

        private void RefreshSlow()
        {
            var strongest = 0f;
            foreach (var slow in _slow.Values) strongest = Math.Max(strongest, slow.Fraction);
            MovementMultiplier = 1f - strongest;
        }
    }
}
