using System.Collections.Generic;
using UnityEngine;

namespace Game.Traps
{
    /// <summary>One turret of the run: where it stands and the live state of its firing cycle.</summary>
    public sealed class TrapPlacement
    {
        public TrapTypeDefinition Type { get; }
        public Vector2 Center { get; }
        /// <summary>Rotation of a fixed-heading trap, degrees counter-clockwise from world +X (0 for aimed traps).</summary>
        public float RotationDegrees { get; }
        /// <summary>Key of the 3D model this turret is drawn with, or null for the placeholder shape.</summary>
        public string ModelKey { get; }
        /// <summary>Current pose of the head, degrees; turns while a spinning trap rests, frozen for a volley.</summary>
        public float HeadAngleDegrees { get; internal set; }

        public TrapState State { get; internal set; }
        public float CooldownRemaining { get; internal set; }
        public float TelegraphRemaining { get; internal set; }
        /// <summary>Heading the current or last volley uses, locked when the telegraph starts.</summary>
        public float LockedHeadingDegrees { get; internal set; }
        public int VolleyIndex { get; internal set; }
        /// <summary>Angle offset (from the volley heading) of the shot that left last; a series' head follows it.</summary>
        public float LastShotAngleDegrees { get; internal set; }
        /// <summary>Seconds since the last volley started leaving (large before the first one).</summary>
        public float SecondsSinceVolley { get; internal set; } = 999f;
        /// <summary>Seconds the trap has been continuously active (the player within the radius).</summary>
        public float RageSeconds { get; internal set; }
        public float RageProgress { get; internal set; }
        public float RageMultiplier { get; internal set; } = 1f;
        public bool IsActive { get; internal set; }
        internal List<TrapPendingShot> Pending { get; } = new List<TrapPendingShot>();

        /// <summary>0..1 progress of the wind-up; 0 outside a telegraph.</summary>
        public float TelegraphProgress => State == TrapState.Telegraph
            ? 1f - Mathf.Clamp01(TelegraphRemaining / Type.TelegraphSeconds) : 0f;

        /// <summary>
        /// Where the head points now (world degrees). A spinning head follows its pose; during a volley it shows the locked heading, and
        /// for a series with delayed shots (spiral, fan) it sweeps with the shot that left last. At rest an aimed head tracks the player,
        /// but only after <see cref="TrapTypeDefinition.AimResumeDelaySeconds"/> past the volley, so it turns just after the shot rather
        /// than at it; a fixed head shows the pose of its next volley.
        /// </summary>
        public float HeadTargetDegrees(Vector2 player)
        {
            if (Type.SpinDegreesPerSecond != 0f) return HeadAngleDegrees;
            var series = Type.LastShotDelaySeconds > 0f;
            if (State != TrapState.Idle) return LockedHeadingDegrees + (State == TrapState.Firing && series ? LastShotAngleDegrees : 0f);
            if (Type.Heading == TrapHeadingMode.Aim)
            {
                if (SecondsSinceVolley < Type.AimResumeDelaySeconds)
                    return LockedHeadingDegrees + (series ? LastShotAngleDegrees : 0f);
                return Mathf.Atan2(player.y - Center.y, player.x - Center.x) * Mathf.Rad2Deg;
            }
            // A fixed head that steps between volleys also waits the same short moment after the shot before turning to its next pose.
            var shown = VolleyIndex > 0 && SecondsSinceVolley < Type.AimResumeDelaySeconds ? VolleyIndex - 1 : VolleyIndex;
            return RotationDegrees + Type.HeadingOffset(shown);
        }

        public TrapPlacement(TrapTypeDefinition type, Vector2 center, float rotationDegrees, float initialCooldownSeconds, string modelKey = null)
        {
            Type = type ?? throw new System.ArgumentNullException(nameof(type));
            Center = center;
            RotationDegrees = rotationDegrees;
            HeadAngleDegrees = rotationDegrees;
            ModelKey = modelKey;
            CooldownRemaining = initialCooldownSeconds;
            LockedHeadingDegrees = rotationDegrees;
        }
    }
}
