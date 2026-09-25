using UnityEngine;

namespace Game.Presentation
{
    // One reusable world-space particle system lives on each pooled projectile.
    // It emits one soft flash particle plus a tiny 2–4 particle material burst.
    /// <summary>Reusable particle-only impact presenter for pooled projectile objects.</summary>
    public sealed class ProjectileImpactRuntime : MonoBehaviour
    {
        private ParticleSystem _particles;
        private float _remaining;

        /// <summary>True while emitted particles still need manual simulation.</summary>
        public bool IsPlaying => _remaining > 0f;

        /// <summary>Emits one flash and the configured material flecks at a world position.</summary>
        public void Play(ProjectilePresentationProfile profile, Vector2 worldPosition)
        {
            if (profile == null) return;
            EnsureParticles();
            _remaining = Mathf.Max(_remaining, profile.ImpactDurationSeconds);
            Emit(worldPosition, Vector2.zero, profile.FlashSize, profile.ImpactDurationSeconds,
                profile.FlashColor);
            for (var i = 0; i < profile.ParticleCount; i++)
            {
                var angle = (i + .5f) * Mathf.PI * 2f / profile.ParticleCount;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * .65f).normalized;
                Emit(worldPosition, direction * profile.ParticleSpeed, profile.ParticleSize,
                    profile.ImpactDurationSeconds, profile.ParticleColor);
            }
        }

        /// <summary>Advances the effect only while gameplay is running and reports completion.</summary>
        public bool Tick(float deltaTime, bool isRunning)
        {
            if (!IsPlaying) return true;
            if (!isRunning) return false;
            _remaining = Mathf.Max(0f, _remaining - deltaTime);
            _particles.Simulate(deltaTime, false, false, false);
            return !IsPlaying;
        }

        /// <summary>Clears all particles so a pooled owner can be reused without stale visuals.</summary>
        public void ResetPresentation()
        {
            _remaining = 0f;
            if (_particles != null)
                _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void EnsureParticles()
        {
            if (_particles != null) return;
            _particles = gameObject.AddComponent<ParticleSystem>();
            ParticlePresentationMaterial.Apply(_particles, GetComponent<SpriteRenderer>());
            var main = _particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 64;
            var emission = _particles.emission;
            emission.enabled = false;
            var shape = _particles.shape;
            shape.enabled = false;
            var color = _particles.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(new Gradient
            {
                alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) },
                colorKeys = new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) }
            });
            var size = _particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, .35f)));
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void Emit(Vector2 position, Vector2 velocity, float size, float lifetime, Color color)
        {
            var particle = new ParticleSystem.EmitParams
            {
                position = position,
                velocity = velocity,
                startSize = size,
                startLifetime = lifetime,
                startColor = color
            };
            _particles.Emit(particle, 1);
        }
    }
}
