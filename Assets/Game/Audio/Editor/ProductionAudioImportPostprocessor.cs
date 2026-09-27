using System;
using UnityEditor;
using UnityEngine;

namespace Game.Audio.Editor
{
    /// <summary>Stream long shared tracks; keep short one-shot cues ready in memory.</summary>
    public sealed class ProductionAudioImportPostprocessor : AssetPostprocessor
    {
        private const string Root = "Assets/Resources/Audio/";

        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(Root, StringComparison.Ordinal)) return;
            var importer = (AudioImporter)assetImporter;
            var settings = importer.defaultSampleSettings;
            var longTrack = assetPath.EndsWith("_music.ogg", StringComparison.Ordinal) ||
                assetPath.EndsWith("field_ambience.ogg", StringComparison.Ordinal);
            settings.loadType = longTrack ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = .75f;
            importer.defaultSampleSettings = settings;
        }
    }
}
