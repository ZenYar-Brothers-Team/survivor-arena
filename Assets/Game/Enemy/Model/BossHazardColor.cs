using System;
using Game.Content;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>RGBA authoring helper for boss hazard presentation colors (four channels 0…1, all required).</summary>
    public static class BossHazardColor
    {
        public static Color Require(float[] value, string owner)
        {
            if (value == null || value.Length != 4)
                throw new InvalidOperationException($"{owner} requires RGBA.");
            foreach (var channel in value) NumericValidation.ValidateRange(channel, 0f, 1f, owner);
            return new Color(value[0], value[1], value[2], value[3]);
        }
    }
}
