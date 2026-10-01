using Game.Character;
using Game.Combat;
using Game.Content;
using Game.Zones;
using UnityEngine;

namespace Game.Bootstrap
{
    /// <summary>Zone-effect view of the player runtime. Damage over time is gathered and applied a few times per second.</summary>
    public sealed class PlayerZoneTarget : IZonePlayerTarget
    {
        private const float DamageIntervalSeconds = 0.25f;

        private readonly PlayerCharacterRuntime _player;
        private readonly Rigidbody2D _body;
        private float _pendingDamage;
        private float _lastDamageTime;

        public PlayerZoneTarget(PlayerCharacterRuntime player, Rigidbody2D body)
        {
            _player = player != null ? player : throw new System.ArgumentNullException(nameof(player));
            _body = body;
        }

        public Vector2 Position => _player.transform.position;
        public bool IsAlive => _player.Health != null && !_player.Health.IsDead;

        public void SetStatModifier(string key, CharacterStatModifier modifier)
        {
            if (_player.Stats != null) _player.SetModifier(key, modifier);
        }

        public void RemoveStatModifier(string key)
        {
            if (_player.Stats != null) _player.RemoveModifier(key);
        }

        public void Damage(float amount, ContentId source)
        {
            if (!IsAlive) return;
            _pendingDamage += amount;
            if (Time.time - _lastDamageTime < DamageIntervalSeconds) return;
            _lastDamageTime = Time.time;
            var damage = _pendingDamage;
            _pendingDamage = 0f;
            _player.ApplyDamage(new CombatDamageRequest(new CombatSource(_player.Identity, source, CombatSourceOrigin.Unknown), damage));
        }

        public void TeleportTo(Vector2 position)
        {
            _player.transform.position = position;
            if (_body == null) return;
            _body.position = position;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
        }
    }
}
