using System;
using System.Collections.Generic;
using Game.Traps.Json;
using UnityEngine;

namespace Game.Traps
{
    /// <summary>A turret that stands at a fixed offset from the player's start; it counts towards its type's total.</summary>
    public sealed class TrapStartDefinition
    {
        public TrapTypeDefinition Type { get; }
        public Vector2 Offset { get; }
        /// <summary>Key of the 3D model it is drawn with, or null for the placeholder shape.</summary>
        public string ModelKey { get; }

        public TrapStartDefinition(TrapStartData data, IReadOnlyList<TrapTypeDefinition> types,
            IReadOnlyDictionary<string, TrapModelDefinition> models)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            TrapTypeDefinition found = null;
            foreach (var type in types) if (type.Id.ToString() == data.Type) found = type;
            Type = found ?? throw new ArgumentException($"Start trap references unknown type '{data.Type}'.");
            Offset = new Vector2(data.OffsetX ?? throw new ArgumentException("startTraps.offsetX is required."),
                data.OffsetY ?? throw new ArgumentException("startTraps.offsetY is required."));
            if (Offset.sqrMagnitude <= 0f) throw new ArgumentException("A start trap cannot stand on the player's start.");
            ModelKey = string.IsNullOrWhiteSpace(data.Model) ? null : data.Model;
            if (ModelKey != null && !models.ContainsKey(ModelKey))
                throw new ArgumentException($"Start trap references unknown model '{ModelKey}'.");
        }
    }
}
