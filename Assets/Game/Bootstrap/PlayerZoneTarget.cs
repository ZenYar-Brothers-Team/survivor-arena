using Game.Character;
using Game.Combat;
using Game.Content;
using Game.Zones;
using UnityEngine;

namespace Game.Bootstrap
{
    /// <summary>Zone-effect view of the player runtime. Damage over time is gathered and applied a few times per second.</summary>
    public sealed class PlayerZoneTarget : IZonePlayerTarget, System.IDisposable
    {
        private const float DamageIntervalSeconds = 0.25f;

        private readonly PlayerCharacterRuntime _player;
        private readonly Rigidbody2D _body;
        private readonly Game.Presentation.SpritePresentationRuntime _quietAreaPresentation;
        private float _pendingDamage;
        private float _lastDamageTime;
        private readonly Game.Run.RunController _run;
        private readonly Game.Presentation.ZoneSealPresentationProfile _portalProfile;
        private readonly Game.Movement.CameraFollowTarget _camera;
        private PortalTransitRuntime _transit;

        public PlayerZoneTarget(PlayerCharacterRuntime player, Rigidbody2D body,
            Game.Presentation.SpritePresentationRuntime quietAreaPresentation = null,
            Game.Run.RunController run = null, Game.Presentation.ZoneSealPresentationProfile portalProfile = null,
            Game.Movement.CameraFollowTarget camera = null)
        {
            _player = player != null ? player : throw new System.ArgumentNullException(nameof(player));
            _body = body;
            _quietAreaPresentation = quietAreaPresentation;
            _run = run; _portalProfile = portalProfile; _camera = camera;
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
            var previous = _quietAreaPresentation != null && _quietAreaPresentation.SuppressDamageFeedback;
            try
            {
                if (_quietAreaPresentation != null) _quietAreaPresentation.SuppressDamageFeedback = true;
                _player.ApplyDamage(new CombatDamageRequest(new CombatSource(_player.Identity, source, CombatSourceOrigin.Unknown), damage));
            }
            finally { if (_quietAreaPresentation != null) _quietAreaPresentation.SuppressDamageFeedback = previous; }
        }

        public void Hit(float amount, ContentId source)
        {
            if (!IsAlive || amount <= 0f) return;
            _player.ApplyDamage(new CombatDamageRequest(new CombatSource(_player.Identity, source, CombatSourceOrigin.Unknown), amount));
        }

        public void HealFraction(float fractionOfMaxHealth)
        {
            if (IsAlive) _player.Health.Heal(_player.Health.MaxHealth * fractionOfMaxHealth);
        }

        public void TeleportTo(Vector2 position)
        {
            if (_portalProfile != null)
            {
                _transit = _player.GetComponent<PortalTransitRuntime>() ?? _player.gameObject.AddComponent<PortalTransitRuntime>();
                _transit.Begin(position, _run, _portalProfile, _player.Health, _quietAreaPresentation, _camera);
                return;
            }
            _player.transform.position = position;
            if (_body == null) return;
            _body.position = position;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
        }
        public void Dispose() { if (_transit != null) _transit.Shutdown(); _pendingDamage = 0f; }
    }
}
