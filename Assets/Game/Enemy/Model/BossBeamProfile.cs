using System;
using System.Collections.Generic;
using Game.Combat;
using Game.Content;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// Boss beam (DECISION-0066, family F2). At the start a thin line shows every beam for <see cref="TelegraphSeconds"/>;
    /// then the beams are active for <see cref="ActiveSeconds"/>, turning by <see cref="SweepDegrees"/> over that time.
    /// Beams start at the boss, point at <see cref="AnglesDegrees"/> from the direction to the player captured when the
    /// telegraph starts, are <see cref="Length"/> long and <see cref="Width"/> wide, and hit the player at most once per
    /// activation. Distances are world units, times are running seconds.
    /// </summary>
    public sealed class BossBeamProfile
    {
        public IReadOnlyList<float> AnglesDegrees { get; }
        public float Length { get; }
        public float Width { get; }
        public float TelegraphSeconds { get; }
        public float ActiveSeconds { get; }
        public float SweepDegrees { get; }
        public float Damage { get; }
        public CombatControlProfile Controls { get; }
        public Color TelegraphColor { get; }
        public Color BeamColor { get; }

        public BossBeamProfile(IEnumerable<float> anglesDegrees, float length, float width, float telegraphSeconds,
            float activeSeconds, float sweepDegrees, float damage, CombatControlProfile controls, Color telegraphColor, Color beamColor)
        {
            var angles = new List<float>(anglesDegrees ?? throw new ArgumentNullException(nameof(anglesDegrees)));
            if (angles.Count == 0) throw new ArgumentException("A beam attack needs at least one beam.", nameof(anglesDegrees));
            foreach (var angle in angles) NumericValidation.ValidateFinite(angle, nameof(anglesDegrees));
            NumericValidation.ValidatePositive(length, nameof(length));
            NumericValidation.ValidatePositive(width, nameof(width));
            NumericValidation.ValidatePositive(telegraphSeconds, nameof(telegraphSeconds));
            NumericValidation.ValidatePositive(activeSeconds, nameof(activeSeconds));
            NumericValidation.ValidateFinite(sweepDegrees, nameof(sweepDegrees));
            NumericValidation.ValidateNonNegativeFinite(damage, nameof(damage));
            AnglesDegrees = angles.AsReadOnly();
            Length = length;
            Width = width;
            TelegraphSeconds = telegraphSeconds;
            ActiveSeconds = activeSeconds;
            SweepDegrees = sweepDegrees;
            Damage = damage;
            Controls = controls ?? throw new ArgumentNullException(nameof(controls));
            TelegraphColor = telegraphColor;
            BeamColor = beamColor;
        }
    }
}
