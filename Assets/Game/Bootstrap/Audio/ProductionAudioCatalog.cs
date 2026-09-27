using System;
using System.Collections.Generic;
using Game.Content.Json;
using UnityEngine;

namespace Game.Bootstrap.Audio
{
    /// <summary>Validated presentation content. Every referenced clip must import before a run starts.</summary>
    public sealed class ProductionAudioCatalog
    {
        private readonly Dictionary<string, AudioClip[]> _clips = new Dictionary<string, AudioClip[]>(StringComparer.Ordinal);
        private readonly Dictionary<string, AudioCueData> _cues = new Dictionary<string, AudioCueData>(StringComparer.Ordinal);
        public AudioClip MenuMusic { get; }
        public AudioClip BattleMusic { get; }
        public AudioClip BossMusic { get; }
        public AudioClip VictoryMusic { get; }
        public AudioClip DefeatMusic { get; }
        public AudioClip FieldAmbience { get; }
        public float AmbienceGain { get; }
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
            FieldAmbience = Require(data.FieldAmbience);
            if (!data.AmbienceGain.HasValue || float.IsNaN(data.AmbienceGain.Value) ||
                data.AmbienceGain.Value < 0 || data.AmbienceGain.Value > 1)
                throw new InvalidOperationException("Invalid ambienceGain.");
            AmbienceGain = data.AmbienceGain.Value;
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
        }

        public bool TryGet(string id, out AudioCueData cue, out AudioClip[] clips)
        {
            if (!_cues.TryGetValue(id, out cue)) { clips = null; return false; }
            clips = _clips[id];
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
