using System;
using UnityEngine;

namespace Game.ScreenEvents
{
    /// <summary>
    /// One dangerous thing inside a screen event (DECISION-0157). Its own timeline starts <see cref="StartDelay"/> seconds after the
    /// event starts: <see cref="TelegraphSeconds"/> of red warning, then <see cref="StrikeSeconds"/> of danger. It hurts the player at most
    /// once. <see cref="Covers"/> is the single geometric truth used by the runtime, the tests and the survivability checks.
    /// Distances are world units; the spawner converts the screen-relative values of the content.
    /// </summary>
    public sealed class ScreenHazard
    {
        private ScreenHazard() { }

        public ScreenHazardKind Kind { get; private set; }
        public ScreenBurstShape Shape { get; private set; }
        public float StartDelay { get; private set; }
        public float TelegraphSeconds { get; private set; }
        public float StrikeSeconds { get; private set; }
        /// <summary>Damage as a fraction of the player's maximum health (0.2 = 20%).</summary>
        public float DamageFraction { get; private set; }
        /// <summary>Sweep: start of the strip. Rect, Circle, OutsideCircle and Ring: centre.</summary>
        public Vector2 Origin { get; private set; }
        /// <summary>Unit axis: sweep travel, rect long side, or the direction of the ring's gap.</summary>
        public Vector2 Direction { get; private set; }
        /// <summary>Sweep: strip length. Rect: full length along <see cref="Direction"/>.</summary>
        public float Length { get; private set; }
        /// <summary>Sweep / Rect: full width across <see cref="Direction"/>.</summary>
        public float Width { get; private set; }
        /// <summary>Circle / OutsideCircle: radius. Ring: radius at the start of the strike.</summary>
        public float Radius { get; private set; }
        /// <summary>Sweep: length of the travelling bar.</summary>
        public float BodyLength { get; private set; }
        /// <summary>Sweep: world units per second along the strip. Ring: radius growth per second.</summary>
        public float Speed { get; private set; }
        public float Thickness { get; private set; }
        public float GapWidth { get; private set; }
        /// <summary>True once this hazard has hurt the player.</summary>
        public bool Hit { get; set; }

        public float TotalSeconds => StartDelay + TelegraphSeconds + StrikeSeconds;

        public static ScreenHazard Sweep(float startDelay, float telegraphSeconds, float damageFraction, Vector2 origin,
            Vector2 direction, float length, float width, float bodyLength, float speed)
        {
            Game.Content.NumericValidation.ValidatePositive(length, nameof(length));
            Game.Content.NumericValidation.ValidatePositive(width, nameof(width));
            Game.Content.NumericValidation.ValidatePositive(bodyLength, nameof(bodyLength));
            Game.Content.NumericValidation.ValidatePositive(speed, nameof(speed));
            return Create(ScreenHazardKind.Sweep, startDelay, telegraphSeconds, (length + bodyLength) / speed, damageFraction, h =>
            {
                h.Origin = origin; h.Direction = direction.normalized; h.Length = length; h.Width = width;
                h.BodyLength = bodyLength; h.Speed = speed;
            });
        }

        public static ScreenHazard Rect(float startDelay, float telegraphSeconds, float strikeSeconds, float damageFraction,
            Vector2 center, Vector2 direction, float length, float width)
        {
            Game.Content.NumericValidation.ValidatePositive(length, nameof(length));
            Game.Content.NumericValidation.ValidatePositive(width, nameof(width));
            return Create(ScreenHazardKind.Burst, startDelay, telegraphSeconds, strikeSeconds, damageFraction, h =>
            {
                h.Shape = ScreenBurstShape.Rect; h.Origin = center; h.Direction = direction.normalized; h.Length = length; h.Width = width;
            });
        }

        public static ScreenHazard Circle(float startDelay, float telegraphSeconds, float strikeSeconds, float damageFraction,
            Vector2 center, float radius, bool outside = false)
        {
            Game.Content.NumericValidation.ValidatePositive(radius, nameof(radius));
            return Create(ScreenHazardKind.Burst, startDelay, telegraphSeconds, strikeSeconds, damageFraction, h =>
            {
                h.Shape = outside ? ScreenBurstShape.OutsideCircle : ScreenBurstShape.Circle; h.Origin = center; h.Radius = radius;
                h.Direction = Vector2.right;
            });
        }

        public static ScreenHazard Ring(float startDelay, float telegraphSeconds, float damageFraction, Vector2 center,
            float startRadius, float endRadius, float speed, float thickness, Vector2 gapDirection, float gapWidth)
        {
            Game.Content.NumericValidation.ValidatePositive(startRadius, nameof(startRadius));
            Game.Content.NumericValidation.ValidatePositive(speed, nameof(speed));
            Game.Content.NumericValidation.ValidatePositive(thickness, nameof(thickness));
            Game.Content.NumericValidation.ValidateNonNegative(gapWidth, nameof(gapWidth));
            if (endRadius <= startRadius) throw new ArgumentException("The ring must grow.", nameof(endRadius));
            return Create(ScreenHazardKind.Ring, startDelay, telegraphSeconds, (endRadius - startRadius) / speed, damageFraction, h =>
            {
                h.Origin = center; h.Radius = startRadius; h.Speed = speed; h.Thickness = thickness;
                h.Direction = gapDirection.normalized; h.GapWidth = gapWidth;
            });
        }

        private static ScreenHazard Create(ScreenHazardKind kind, float startDelay, float telegraphSeconds, float strikeSeconds,
            float damageFraction, Action<ScreenHazard> fill)
        {
            Game.Content.NumericValidation.ValidateNonNegative(startDelay, nameof(startDelay));
            Game.Content.NumericValidation.ValidatePositive(telegraphSeconds, nameof(telegraphSeconds));
            Game.Content.NumericValidation.ValidatePositive(strikeSeconds, nameof(strikeSeconds));
            Game.Content.NumericValidation.ValidateRange(damageFraction, 0f, 1f, nameof(damageFraction));
            var hazard = new ScreenHazard
            {
                Kind = kind, StartDelay = startDelay, TelegraphSeconds = telegraphSeconds, StrikeSeconds = strikeSeconds,
                DamageFraction = damageFraction
            };
            fill(hazard);
            return hazard;
        }

        public ScreenHazardPhase PhaseAt(float eventSeconds)
        {
            if (eventSeconds < StartDelay) return ScreenHazardPhase.Waiting;
            if (eventSeconds < StartDelay + TelegraphSeconds) return ScreenHazardPhase.Telegraph;
            return eventSeconds < TotalSeconds ? ScreenHazardPhase.Strike : ScreenHazardPhase.Done;
        }

        /// <summary>0 at the start of the warning, 1 when the strike begins.</summary>
        public float TelegraphProgressAt(float eventSeconds) => Mathf.Clamp01((eventSeconds - StartDelay) / TelegraphSeconds);

        /// <summary>Seconds into the strike (negative before it).</summary>
        public float StrikeTimeAt(float eventSeconds) => eventSeconds - StartDelay - TelegraphSeconds;

        /// <summary>Whether a player circle at <paramref name="point"/> would be hurt <paramref name="strikeTime"/> seconds into the strike.</summary>
        public bool Covers(float strikeTime, Vector2 point, float playerRadius)
        {
            if (strikeTime < 0f || strikeTime > StrikeSeconds) return false;
            var relative = point - Origin;
            switch (Kind)
            {
                case ScreenHazardKind.Sweep:
                {
                    var along = Vector2.Dot(relative, Direction);
                    var across = Mathf.Abs(Direction.x * relative.y - Direction.y * relative.x);
                    var head = Speed * strikeTime;
                    return across <= Width * .5f + playerRadius
                        && along >= head - BodyLength - playerRadius && along <= head + playerRadius
                        && along >= -playerRadius && along <= Length + playerRadius;
                }
                case ScreenHazardKind.Burst:
                    switch (Shape)
                    {
                        case ScreenBurstShape.Rect:
                        {
                            var along = Mathf.Abs(Vector2.Dot(relative, Direction));
                            var across = Mathf.Abs(Direction.x * relative.y - Direction.y * relative.x);
                            return along <= Length * .5f + playerRadius && across <= Width * .5f + playerRadius;
                        }
                        case ScreenBurstShape.Circle:
                            return relative.magnitude <= Radius + playerRadius;
                        default:
                            return relative.magnitude > Radius - playerRadius;
                    }
                default:
                {
                    var distance = relative.magnitude;
                    var ring = Radius + Speed * strikeTime;
                    if (Mathf.Abs(distance - ring) > Thickness * .5f + playerRadius) return false;
                    var along = Vector2.Dot(relative, Direction);
                    var across = Mathf.Abs(Direction.x * relative.y - Direction.y * relative.x);
                    return !(along > 0f && across + playerRadius <= GapWidth * .5f);
                }
            }
        }

        /// <summary>Radius of a ring hazard's centre line, <paramref name="strikeTime"/> seconds into the strike.</summary>
        public float RingRadiusAt(float strikeTime) => Radius + Speed * Mathf.Clamp(strikeTime, 0f, StrikeSeconds);

        /// <summary>Position along the strip of the head of a sweep, <paramref name="strikeTime"/> seconds into the strike.</summary>
        public float SweepHeadAt(float strikeTime) => Speed * Mathf.Clamp(strikeTime, 0f, StrikeSeconds);
    }
}
