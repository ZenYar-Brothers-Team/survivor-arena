using System;
using Game.Content.Json;
using Game.Content;
using UnityEngine;

namespace Game.UI
{
    // Presentation-only values from the approved Menu E browser review, in seconds/pixels/fractions.
    public sealed class MenuArtProfile
    {
        public string backgroundId, foregroundId;
        public float driftSeconds, backgroundDrift, foregroundDrift, backgroundPointer, foregroundPointer, pointerResponse;
        public float rayAngle, dustMinSize, dustMaxSize, dustOpacity;
        public float raySpeed, dustSpeed, dustWander, dustWanderSeconds;
        public float dustDriftX, dustDriftY;
        public int dustCount;
        public static MenuArtProfile Load()
        {
            var value = JsonContentFile.Load<MenuArtProfile>("Content/Presentation/MenuArtProfile");
            value.Validate(); return value;
        }
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(backgroundId) || string.IsNullOrWhiteSpace(foregroundId))
                throw new InvalidOperationException("Menu layers require visual IDs.");
            NumericValidation.ValidateRange(driftSeconds, 10, 120, nameof(driftSeconds));
            NumericValidation.ValidateRange(pointerResponse, .1f, 20, nameof(pointerResponse));
            NumericValidation.ValidateRange(backgroundDrift, 0, .02f, nameof(backgroundDrift));
            NumericValidation.ValidateRange(foregroundDrift, 0, .02f, nameof(foregroundDrift));
            NumericValidation.ValidateRange(backgroundPointer, 0, .02f, nameof(backgroundPointer));
            NumericValidation.ValidateRange(foregroundPointer, 0, .02f, nameof(foregroundPointer));
            NumericValidation.ValidateRange(rayAngle, 0, 5, nameof(rayAngle));
            NumericValidation.ValidateRange(raySpeed, .05f, 2, nameof(raySpeed));
            NumericValidation.ValidateRange(dustSpeed, .05f, 2, nameof(dustSpeed));
            NumericValidation.ValidateRange(dustWander, 0, .05f, nameof(dustWander));
            NumericValidation.ValidateRange(dustWanderSeconds, 10, 120, nameof(dustWanderSeconds));
            NumericValidation.ValidateRange(Mathf.Abs(dustDriftX), .05f, 1, nameof(dustDriftX));
            NumericValidation.ValidateRange(dustDriftY, -.5f, .5f, nameof(dustDriftY));
            NumericValidation.ValidateRange(dustCount, 0, 32, nameof(dustCount));
            NumericValidation.ValidateRange(dustMinSize, .1f, 32, nameof(dustMinSize));
            NumericValidation.ValidateRange(dustMaxSize, dustMinSize, 32, nameof(dustMaxSize));
            NumericValidation.ValidateRange(dustOpacity, 0, .6f, nameof(dustOpacity));
        }

        public float DustCycle(int index, float seconds) =>
            Mathf.Repeat(seconds * dustSpeed / (11 + index % 7) + index * .137f, 1);

        public Vector2 DustPosition(int index, float seconds)
        {
            var cycle = DustCycle(index, seconds);
            var phase = seconds * Mathf.PI * 2 / dustWanderSeconds * (1 + index % 5 * .11f);
            var seed = index * 2.399963f;
            var sway = Mathf.Sin(phase + seed) * .7f + Mathf.Sin(phase * .613f + seed * 1.7f) * .3f;
            return new Vector2(.48f + (index % 2) * .21f + (index % 5) * .043f + (cycle - .5f) * dustDriftX + sway * dustWander,
                .3f + Mathf.Repeat(index * .618034f, 1) * .65f + (cycle - .5f) * dustDriftY +
                Mathf.Sin(phase * .79f + seed * 1.3f) * dustWander * .35f);
        }

        public float RayPhase(int index, float seconds) =>
            Mathf.Sin(seconds * raySpeed * Mathf.PI / (10 + index * 1.5f) + index * 1.7f);
    }
}
