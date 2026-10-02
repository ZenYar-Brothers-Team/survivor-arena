using Game.Presentation;
using UnityEngine;

namespace Game.ActiveSkill
{
    /// <summary>
    /// One live beam drawn for the whole <see cref="BeamEffect.DurationSeconds"/>: it follows the owner every frame
    /// (and the tracked target for a tracking beam) instead of leaving per-tick pulses at the launch point.
    /// Presentation only; damage ticks resolve the same pose through the shared <see cref="BeamTargetTracker"/>.
    /// </summary>
    internal sealed class SkillBeamVisualState
    {
        private readonly ActiveSkillActivation _activation;
        private readonly BeamEffect _effect;
        private readonly SkillWorldEffectProfile _profile;
        private readonly SkillWorldEffectPresenter _presenter;
        private readonly BeamTargetTracker _tracker;
        private readonly object _handle;
        private readonly float _length;
        private readonly float _width;
        private float _elapsed;

        public SkillBeamVisualState(ActiveSkillActivation activation, BeamEffect effect, SkillWorldEffectProfile profile,
            SkillWorldEffectPresenter presenter, BeamTargetTracker tracker)
        {
            _tracker = tracker;
            _activation = activation;
            _effect = effect;
            _profile = profile;
            _presenter = presenter;
            _length = effect.Range * activation.RangeMultiplier;
            _width = effect.Width * activation.SizeMultiplier;
            tracker.Resolve(activation, effect, out var origin, out var direction);
            _handle = presenter.BeginBeam(profile, origin, direction, _length, _width);
        }

        /// <summary>Advances the beam; returns true once its duration is over and it has been released to fade.</summary>
        public bool Tick(float deltaTime)
        {
            _elapsed += deltaTime;
            if (_elapsed >= _effect.DurationSeconds)
            {
                _presenter.Release(_handle, _profile.FadeSeconds);
                return true;
            }
            _tracker.Resolve(_activation, _effect, out var origin, out var direction);
            _presenter.UpdateBeam(_handle, origin, direction, _length, _width, _profile, _elapsed);
            return false;
        }
    }
}
