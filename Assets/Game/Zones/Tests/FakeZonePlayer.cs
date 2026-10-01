using System.Collections.Generic;
using Game.Character;
using Game.Content;
using UnityEngine;

namespace Game.Zones.Tests
{
    internal sealed class FakeZonePlayer : IZonePlayerTarget
    {
        public Vector2 Position { get; set; }
        public bool IsAlive { get; set; } = true;
        public readonly Dictionary<string, CharacterStatModifier> Modifiers = new Dictionary<string, CharacterStatModifier>();
        public float DamageTaken;
        public readonly List<Vector2> Teleports = new List<Vector2>();

        public void SetStatModifier(string key, CharacterStatModifier modifier) => Modifiers[key] = modifier;
        public void RemoveStatModifier(string key) => Modifiers.Remove(key);
        public void Damage(float amount, ContentId source) => DamageTaken += amount;

        public void TeleportTo(Vector2 position)
        {
            Teleports.Add(position);
            Position = position;
        }

        public CharacterStatModifier Zone => Modifiers.TryGetValue(ZoneRuntime.ModifierKey, out var modifier) ? modifier : default;
    }
}
