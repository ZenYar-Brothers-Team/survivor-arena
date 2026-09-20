using System;
using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    public sealed class ProceduralSpriteAnimator
    {
        private const float TwoPi = Mathf.PI * 2f;

        private readonly SpriteMotionProfile _profile;
        private float _idlePhase;
        private float _locomotionPhase;
        private float _hitRemaining;
        private float _flashRemaining;
        private float _spawnRemaining;
        private Vector2 _velocity;
        private int _facing = 1;

        public SpritePose CurrentPose => ComposePose();

        public ProceduralSpriteAnimator(SpriteMotionProfile profile)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            Reset();
        }

        public SpritePose Tick(float deltaTime, bool isRunning, Vector2 velocity)
        {
            NumericValidation.ValidateNonNegativeFinite(deltaTime, nameof(deltaTime));
            NumericValidation.ValidateFinite(velocity.x, nameof(velocity));
            NumericValidation.ValidateFinite(velocity.y, nameof(velocity));

            if (!isRunning)
                return ComposePose();

            _velocity = velocity;
            if (Mathf.Abs(velocity.x) > 0.0001f)
                _facing = velocity.x < 0f ? -1 : 1;

            _idlePhase = Mathf.Repeat(
                _idlePhase + deltaTime * _profile.IdleFrequency * TwoPi,
                TwoPi);
            _locomotionPhase = Mathf.Repeat(
                _locomotionPhase + deltaTime * _profile.BobFrequency * TwoPi,
                TwoPi);
            _hitRemaining = Mathf.Max(0f, _hitRemaining - deltaTime);
            _flashRemaining = Mathf.Max(0f, _flashRemaining - deltaTime);
            _spawnRemaining = Mathf.Max(0f, _spawnRemaining - deltaTime);
            return ComposePose();
        }

        public void PlayHit(float appliedDamage)
        {
            NumericValidation.ValidatePositive(appliedDamage, nameof(appliedDamage));
            _hitRemaining = _profile.HitDurationSeconds;
            _flashRemaining = _profile.HitFlashDurationSeconds;
        }

        public void PlaySpawn()
        {
            _spawnRemaining = _profile.SpawnDurationSeconds;
        }

        public void Reset()
        {
            _idlePhase = 0f;
            _locomotionPhase = 0f;
            _hitRemaining = 0f;
            _flashRemaining = 0f;
            _spawnRemaining = _profile.SpawnDurationSeconds;
            _velocity = Vector2.zero;
            _facing = 1;
        }

        private SpritePose ComposePose()
        {
            var speedRatio = Mathf.Clamp01(_velocity.magnitude / _profile.ReferenceSpeed);
            var idleWeight = 1f - speedRatio;
            var idleWave = Mathf.Sin(_idlePhase);
            var idleSwayWave = (Mathf.Sin(_idlePhase) * 0.75f) +
                               (Mathf.Sin(_idlePhase * 2f) * 0.25f);
            var locomotionWave = Mathf.Sin(_locomotionPhase);
            var stretchEnvelope = Mathf.Abs(locomotionWave) * speedRatio;
            var positionOffset = new Vector3(
                0f,
                (idleWave * _profile.IdleBobAmplitude * idleWeight) +
                (locomotionWave * _profile.BobAmplitude * speedRatio),
                0f);
            var scale = new Vector2(
                1f - (_profile.IdleBreathStretch * 0.4f * idleWave * idleWeight) -
                (_profile.LocomotionStretch * 0.5f * stretchEnvelope),
                1f + (_profile.IdleBreathStretch * idleWave * idleWeight) +
                (_profile.LocomotionStretch * stretchEnvelope));
            var rotation = (_profile.IdleSwayDegrees * idleSwayWave * idleWeight) -
                           (Mathf.Clamp(_velocity.x / _profile.ReferenceSpeed, -1f, 1f) *
                            _profile.MaxTiltDegrees);

            if (_hitRemaining > 0f)
            {
                var hitEnvelope = _hitRemaining / _profile.HitDurationSeconds;
                hitEnvelope *= hitEnvelope;
                scale.x *= 1f + (_profile.HitSquash * hitEnvelope);
                scale.y *= 1f - (_profile.HitSquash * hitEnvelope);
                rotation -= _facing * _profile.HitTiltDegrees * hitEnvelope;
            }

            rotation = Mathf.Clamp(rotation, -8f, 8f);

            if (_spawnRemaining > 0f)
            {
                var spawnProgress = 1f - (_spawnRemaining / _profile.SpawnDurationSeconds);
                var easedProgress = spawnProgress * spawnProgress * (3f - (2f * spawnProgress));
                var spawnScale = Mathf.Lerp(_profile.SpawnScaleFrom, 1f, easedProgress);
                scale *= spawnScale;
            }

            var flashAmount = _flashRemaining > 0f
                ? Mathf.Clamp01(_flashRemaining / _profile.HitFlashDurationSeconds)
                : 0f;

            return new SpritePose(positionOffset, rotation, scale, _facing < 0, flashAmount);
        }
    }
}
