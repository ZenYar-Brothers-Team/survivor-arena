using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Reusable world-space explosion burst that owns no gameplay timing or damage.</summary>
    public sealed class ExplosionBurstRuntime : MonoBehaviour
    {
        private ParticleSystem _particles;
        private float _remaining;

        /// <summary>True while the manually simulated burst still has visible particles.</summary>
        public bool IsPlaying => _remaining > 0f;

        /// <summary>Emits one soft core flash and a small radial material burst at the supplied blast radius.</summary>
        public void Play(ExplosionPresentationProfile profile, Vector2 worldPosition, float blastRadius)
        {
            if (profile == null || blastRadius <= 0f) return;
            EnsureParticles();
            _remaining = Mathf.Max(_remaining, profile.DurationSeconds);
            Emit(worldPosition, Vector2.zero, blastRadius * profile.FlashSizeMultiplier,
                profile.DurationSeconds, profile.FlashColor);
            for (var i = 0; i < profile.ParticleCount; i++)
            {
                var angle = (i + .5f) * Mathf.PI * 2f / profile.ParticleCount;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * .72f).normalized;
                Emit(worldPosition, direction * (blastRadius * profile.ParticleSpeedMultiplier),
                    blastRadius * profile.ParticleSizeMultiplier, profile.DurationSeconds, profile.ParticleColor);
            }
        }

        /// <summary>Advances only while the run is active and reports whether the burst has completed.</summary>
        public bool Tick(float deltaTime, bool isRunning)
        {
            if (!IsPlaying) return true;
            if (!isRunning) return false;
            _remaining = Mathf.Max(0f, _remaining - deltaTime);
            _particles.Simulate(deltaTime, false, false, false);
            return !IsPlaying;
        }

        /// <summary>Clears all particles and restores the component for pooled reuse.</summary>
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
                alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(.82f, .45f), new GradientAlphaKey(0f, 1f) },
                colorKeys = new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) }
            });
            var size = _particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(new Keyframe(0f, .45f), new Keyframe(.2f, 1f), new Keyframe(1f, .15f)));
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void Emit(Vector2 position, Vector2 velocity, float size, float lifetime, Color color)
        {
            _particles.Emit(new ParticleSystem.EmitParams
            {
                position = position,
                velocity = velocity,
                startSize = size,
                startLifetime = lifetime,
                startColor = color
            }, 1);
        }
    }
}
