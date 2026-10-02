using System;
using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>One connected branch and its book at the circular end center.</summary>
    public sealed class FieldRoadDeadEnd
    {
        public Vector2 Entrance { get; }
        public Vector2 EndCenter { get; }
        public float Length { get; }
        public FieldRoadDeadEnd(Vector2 entrance, Vector2 endCenter, float length)
        {
            NumericValidation.ValidatePositive(length, nameof(length));
            for (var i = 0; i < 2; i++) { NumericValidation.ValidateFinite(entrance[i], nameof(entrance)); NumericValidation.ValidateFinite(endCenter[i], nameof(endCenter)); }
            if (entrance == endCenter) throw new ArgumentException("A branch must have length.");
            Entrance = entrance; EndCenter = endCenter; Length = length;
        }
    }
}
