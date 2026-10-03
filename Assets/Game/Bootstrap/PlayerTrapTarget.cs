using Game.Character;
using Game.Combat;
using Game.Content;
using Game.Traps;
using UnityEngine;

namespace Game.Bootstrap
{
    /// <summary>Trap view of the player runtime: its position, liveness and a one-off hit (DECISION-0156).</summary>
    public sealed class PlayerTrapTarget : ITrapPlayerTarget
    {
        private readonly PlayerCharacterRuntime _player;

        public PlayerTrapTarget(PlayerCharacterRuntime player)
        {
            _player = player != null ? player : throw new System.ArgumentNullException(nameof(player));
        }

        public Vector2 Position => _player.transform.position;
        public bool IsAlive => _player.Health != null && !_player.Health.IsDead;

        public void Hit(float amount, ContentId source)
        {
            if (!IsAlive || amount <= 0f) return;
            _player.ApplyDamage(new CombatDamageRequest(new CombatSource(_player.Identity, source, CombatSourceOrigin.Unknown), amount));
        }
    }
}
