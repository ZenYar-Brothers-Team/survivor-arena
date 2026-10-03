using Game.Character;
using Game.Combat;
using Game.Content;
using Game.ScreenEvents;
using UnityEngine;

namespace Game.Bootstrap
{
    /// <summary>Screen-event view of the player runtime: position, liveness and a hit worth a share of maximum health (DECISION-0157).</summary>
    public sealed class PlayerScreenEventTarget : IScreenEventPlayerTarget
    {
        private readonly PlayerCharacterRuntime _player;

        public PlayerScreenEventTarget(PlayerCharacterRuntime player)
        {
            _player = player != null ? player : throw new System.ArgumentNullException(nameof(player));
        }

        public Vector2 Position => _player.transform.position;
        public bool IsAlive => _player.Health != null && !_player.Health.IsDead;

        public void HitFraction(float fractionOfMaxHealth, ContentId source)
        {
            if (!IsAlive || fractionOfMaxHealth <= 0f) return;
            var amount = _player.Health.MaxHealth * fractionOfMaxHealth;
            _player.ApplyDamage(new CombatDamageRequest(new CombatSource(_player.Identity, source, CombatSourceOrigin.Unknown), amount));
        }
    }
}
