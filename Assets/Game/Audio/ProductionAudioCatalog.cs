using System;
using System.Collections.Generic;
using Game.Audio.Json;
using Game.Content;
using Game.Content.Json;
using UnityEngine;

namespace Game.Audio
{
    /// <summary>Validated presentation content. Every referenced clip must import before a run starts.</summary>
    public sealed class ProductionAudioCatalog
    {
        private readonly Dictionary<string, AudioClip[]> _clips = new Dictionary<string, AudioClip[]>(StringComparer.Ordinal);
        private readonly Dictionary<string, AudioCueData> _cues = new Dictionary<string, AudioCueData>(StringComparer.Ordinal);
        private readonly Dictionary<string, AudioClip> _ambienceClips = new Dictionary<string, AudioClip>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _skillCues = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _screenEventCues = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, float> _ambienceGains = new Dictionary<string, float>(StringComparer.Ordinal);
        public AudioClip MenuMusic { get; }
        public AudioClip BattleMusic { get; }
        public AudioClip BossMusic { get; }
        public AudioClip VictoryMusic { get; }
        public AudioClip DefeatMusic { get; }
        public float RoutineGlobalCooldownSeconds { get; }

        public static ProductionAudioCatalog Load() =>
            new ProductionAudioCatalog(JsonContentFile.Load<AudioCatalogData>("Content/Audio/ProductionAudio"));

        private ProductionAudioCatalog(AudioCatalogData data)
        {
            if (data == null || data.Cues == null) throw new InvalidOperationException("Production audio cues are required.");
            MenuMusic = Require(data.MenuMusic);
            BattleMusic = Require(data.BattleMusic);
            BossMusic = Require(data.BossMusic);
            VictoryMusic = Require(data.VictoryMusic);
            DefeatMusic = Require(data.DefeatMusic);
            if (data.FieldAmbiences == null) throw new InvalidOperationException("fieldAmbiences is required (may be empty).");
            foreach (var ambience in data.FieldAmbiences)
            {
                if (ambience == null || string.IsNullOrWhiteSpace(ambience.FieldId) || !ambience.Gain.HasValue)
                    throw new InvalidOperationException("Field ambience requires fieldId and gain.");
                NumericValidation.ValidateRange(ambience.Gain.Value, 0, 1, "ambience.gain");
                if (_ambienceClips.ContainsKey(ambience.FieldId))
                    throw new InvalidOperationException($"Duplicate field ambience '{ambience.FieldId}'.");
                _ambienceClips.Add(ambience.FieldId, Require(ambience.Clip));
                _ambienceGains.Add(ambience.FieldId, ambience.Gain.Value);
            }
            if (!data.RoutineGlobalCooldownSeconds.HasValue ||
                float.IsNaN(data.RoutineGlobalCooldownSeconds.Value) ||
                data.RoutineGlobalCooldownSeconds.Value < 0 || data.RoutineGlobalCooldownSeconds.Value > 1)
                throw new InvalidOperationException("Invalid routineGlobalCooldownSeconds.");
            RoutineGlobalCooldownSeconds = data.RoutineGlobalCooldownSeconds.Value;
            foreach (var cue in data.Cues)
            {
                if (cue == null || string.IsNullOrWhiteSpace(cue.Id) || cue.Clips == null || cue.Clips.Length == 0 ||
                    !cue.Gain.HasValue || float.IsNaN(cue.Gain.Value) || cue.Gain.Value < 0 || cue.Gain.Value > 1 ||
                    !cue.CooldownSeconds.HasValue || float.IsNaN(cue.CooldownSeconds.Value) || cue.CooldownSeconds.Value < 0 ||
                    !cue.Priority.HasValue || cue.Priority.Value < 0 || cue.Priority.Value > 2 || !cue.Ui.HasValue)
                    throw new InvalidOperationException($"Invalid production audio cue '{cue?.Id}'.");
                if (_cues.ContainsKey(cue.Id)) throw new InvalidOperationException($"Duplicate audio cue '{cue.Id}'.");
                var clips = new AudioClip[cue.Clips.Length];
                for (var i = 0; i < clips.Length; i++) clips[i] = Require(cue.Clips[i]);
                _cues.Add(cue.Id, cue);
                _clips.Add(cue.Id, clips);
            }
            if (data.ScreenEventCues == null) throw new InvalidOperationException("screenEventCues is required (may be empty).");
            foreach (var binding in data.ScreenEventCues)
            {
                if (binding == null || string.IsNullOrWhiteSpace(binding.EventId) || string.IsNullOrWhiteSpace(binding.Cue) ||
                    !_cues.ContainsKey(binding.Cue)) throw new InvalidOperationException("Invalid screen-event cue binding.");
                if (!_screenEventCues.TryAdd(binding.EventId, binding.Cue))
                    throw new InvalidOperationException("Duplicate screen-event cue binding: " + binding.EventId);
            }
            if (data.SkillCues == null) throw new InvalidOperationException("skillCues is required (may be empty).");
            foreach (var binding in data.SkillCues)
            {
                if (binding == null || string.IsNullOrWhiteSpace(binding.SkillId) || string.IsNullOrWhiteSpace(binding.Cue))
                    throw new InvalidOperationException("Skill cue binding requires skillId and cue.");
                if (!_cues.ContainsKey(binding.Cue))
                    throw new InvalidOperationException($"Skill '{binding.SkillId}' binds unknown cue '{binding.Cue}'.");
                if (!_skillCues.TryAdd(binding.SkillId, binding.Cue))
                    throw new InvalidOperationException($"Duplicate skill cue binding '{binding.SkillId}'.");
            }
        }

        /// <summary>The sound family of an active skill's activation; false when the skill has no binding.</summary>
        public bool TryGetSkillCue(string skillId, out string cueId) => _skillCues.TryGetValue(skillId, out cueId);
        /// <summary>Sound family at the start of each hazard's strike, not on player contact.</summary>
        public bool TryGetScreenEventCue(string eventId, out string cueId) => _screenEventCues.TryGetValue(eventId, out cueId);

        public bool TryGet(string id, out AudioCueData cue, out AudioClip[] clips)
        {
            if (!_cues.TryGetValue(id, out cue)) { clips = null; return false; }
            clips = _clips[id];
            return true;
        }

        /// <summary>Fields without a binding have no ambience; shared music remains available.</summary>
        public bool TryGetAmbience(string fieldId, out AudioClip clip, out float gain)
        {
            gain = 0;
            if (!_ambienceClips.TryGetValue(fieldId, out clip)) return false;
            gain = _ambienceGains[fieldId];
            return true;
        }

        private static AudioClip Require(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new InvalidOperationException("Audio clip path is required.");
            var clip = Resources.Load<AudioClip>(path);
            if (clip == null) throw new InvalidOperationException($"Missing production audio clip '{path}'.");
            return clip;
        }
    }
}
