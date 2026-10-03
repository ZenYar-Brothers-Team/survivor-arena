using System;
using Game.Traps.Json;
using UnityEngine;

namespace Game.Traps
{
    /// <summary>A barrel that stands at a fixed offset from the player's start; explosive or plain as authored.</summary>
    public sealed class TrapStartBarrelDefinition
    {
        public Vector2 Offset { get; }
        public bool Explosive { get; }

        public TrapStartBarrelDefinition(TrapStartBarrelData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            Offset = new Vector2(data.OffsetX ?? throw new ArgumentException("startBarrels.offsetX is required."),
                data.OffsetY ?? throw new ArgumentException("startBarrels.offsetY is required."));
            if (Offset.sqrMagnitude <= 0f) throw new ArgumentException("A start barrel cannot stand on the player's start.");
            Explosive = data.Explosive ?? throw new ArgumentException("startBarrels.explosive is required.");
        }
    }
}
