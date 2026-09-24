using System;
using UnityEngine;

namespace Game.Presentation
{
    [DisallowMultipleComponent]
    public sealed class EnemyDeathPresentationRuntime : MonoBehaviour
    {
        private Transform _visual;
        private SpriteRenderer _renderer;
        private ParticleSystem _dust;
        private EnemyDeathPresentationProfile _profile;
        private Vector3 _startScale;
        private Color _startColor;
        private float _elapsed;

        public bool IsPlaying { get; private set; }

        public void Begin(EnemyDeathPresentationProfile profile, Transform source, SpriteRenderer sourceRenderer)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (sourceRenderer == null) throw new ArgumentNullException(nameof(sourceRenderer));
            EnsureObjects();
            ResetPresentation();
            _visual.localPosition = transform.InverseTransformPoint(source.position);
            _visual.localRotation = Quaternion.Inverse(transform.rotation) * source.rotation;
            var parentScale = transform.lossyScale;
            var sourceScale = source.lossyScale;
            _visual.localScale = new Vector3(sourceScale.x / parentScale.x, sourceScale.y / parentScale.y, 1f);
            _renderer.sprite = sourceRenderer.sprite;
            _renderer.sharedMaterial = sourceRenderer.sharedMaterial;
            _renderer.sortingLayerID = sourceRenderer.sortingLayerID;
            _renderer.sortingOrder = sourceRenderer.sortingOrder;
            _renderer.flipX = sourceRenderer.flipX;
            _renderer.color = sourceRenderer.color;
            _startScale = _visual.localScale;
            _startColor = _renderer.color;
            sourceRenderer.enabled = false;
            _visual.gameObject.SetActive(true);
            EmitDust();
            IsPlaying = true;
        }

        public bool Tick(float deltaTime)
        {
            if (!IsPlaying) return true;
            if (!float.IsFinite(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            _elapsed = Mathf.Min(_profile.TotalDurationSeconds, _elapsed + deltaTime);
            if (_elapsed <= _profile.SquashDurationSeconds)
            {
                var t = Smooth(_elapsed / _profile.SquashDurationSeconds);
                _visual.localScale = Vector3.Scale(_startScale, new Vector3(
                    Mathf.Lerp(1f, _profile.SquashWidthScale, t),
                    Mathf.Lerp(1f, _profile.SquashHeightScale, t), 1f));
            }
            else
            {
                var t = Smooth((_elapsed - _profile.SquashDurationSeconds) / _profile.FadeDurationSeconds);
                var squash = Vector3.Scale(_startScale,
                    new Vector3(_profile.SquashWidthScale, _profile.SquashHeightScale, 1f));
                _visual.localScale = Vector3.Lerp(squash, _startScale * _profile.EndScale, t);
                _renderer.color = Color.Lerp(_startColor, _profile.EndColor, t);
            }
            _dust.Simulate(deltaTime, false, false, false);
            if (_elapsed < _profile.TotalDurationSeconds) return false;
            IsPlaying = false;
            return true;
        }

        public void ResetPresentation()
        {
            IsPlaying = false;
            _elapsed = 0f;
            if (_dust != null) { _dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }
            if (_renderer != null)
            {
                _renderer.sprite = null;
                _renderer.color = Color.white;
                _renderer.flipX = false;
            }
            if (_visual != null) _visual.gameObject.SetActive(false);
        }

        private void EnsureObjects()
        {
            if (_visual != null) return;
            var visualObject = new GameObject("DeathVisual");
            visualObject.transform.SetParent(transform, false);
            _visual = visualObject.transform;
            _renderer = visualObject.AddComponent<SpriteRenderer>();
            var dustObject = new GameObject("DeathDust");
            dustObject.transform.SetParent(transform, false);
            _dust = dustObject.AddComponent<ParticleSystem>();
            var main = _dust.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 12;
            var emission = _dust.emission;
            emission.enabled = false;
            var shape = _dust.shape;
            shape.enabled = false;
            _dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            visualObject.SetActive(false);
        }

        private void EmitDust()
        {
            var dustTransform = _dust.transform;
            dustTransform.localPosition = _visual.localPosition;
            dustTransform.localRotation = Quaternion.identity;
            dustTransform.localScale = Vector3.one;
            for (var i = 0; i < _profile.DustCount; i++)
            {
                var angle = (i + .5f) * Mathf.PI / _profile.DustCount;
                var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle) * .55f + .2f, 0f).normalized;
                var particle = new ParticleSystem.EmitParams
                {
                    position = Vector3.zero,
                    velocity = direction * _profile.DustSpeed,
                    startLifetime = _profile.DustLifetimeSeconds,
                    startSize = _profile.DustSize,
                    startColor = _profile.DustColor
                };
                _dust.Emit(particle, 1);
            }
        }

        private static float Smooth(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }
    }
}
